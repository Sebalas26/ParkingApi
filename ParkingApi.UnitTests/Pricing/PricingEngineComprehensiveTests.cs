using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Core.Services.Tickets;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;
using PaymentMethodEnum = ParkingApi.Domain.Common.Enums.PaymentMethod;
using Xunit;

namespace ParkingApi.UnitTests.Pricing;

public class PricingEngineComprehensiveTests
{
    private readonly Mock<IParkingTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IVehicleRateRepository> _rateRepoMock = new();
    private readonly Mock<IStoreRepository> _storeRepoMock = new();
    private readonly Mock<ICommercialAgreementRepository> _agreementRepoMock = new();
    private readonly Mock<ITicketDiscountRepository> _discountRepoMock = new();
    private readonly Mock<IVehicleIncidentRepository> _incidentRepoMock = new();
    private readonly Mock<IBillingResolutionRepository> _resolutionRepoMock = new();
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ParkingTicketService>> _loggerMock = new();

    private ParkingTicketService CreateService()
    {
        return new ParkingTicketService(
            _ticketRepoMock.Object,
            _rateRepoMock.Object,
            _storeRepoMock.Object,
            _agreementRepoMock.Object,
            _discountRepoMock.Object,
            _incidentRepoMock.Object,
            _resolutionRepoMock.Object,
            _branchRepoMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData(10, 0)] // Dentro de gracia (15 min) -> $0
    [InlineData(60, 4000)] // 1 hora -> $4.000
    [InlineData(120, 8000)] // 2 horas -> $8.000
    [InlineData(180, 16000)] // 3 horas (Umbral de activación alcanzado) -> Tarifa Plena $16.000
    [InlineData(300, 16000)] // 5 horas (Dentro de cobertura de 8h) -> Tarifa Plena $16.000
    [InlineData(480, 16000)] // 8 horas (Límite cobertura Plena 1) -> Tarifa Plena $16.000
    [InlineData(510, 18000)] // 8h 30m (Plena 1 + 30 min a $2.000) -> $18.000
    [InlineData(540, 20000)] // 9 horas (Plena 1 + 1 hora a $4.000) -> $20.000
    [InlineData(600, 24000)] // 10 horas (Plena 1 + 2 horas a $8.000) -> $24.000
    [InlineData(660, 32000)] // 11 horas (Plena 1 + Excedente cumplió 3h -> Plena 2) -> $32.000
    [InlineData(840, 32000)] // 14 horas (Dentro de cobertura Plena 2) -> $32.000
    [InlineData(960, 32000)] // 16 horas (Límite cobertura Plena 2) -> $32.000
    [InlineData(1020, 36000)] // 17 horas (Plena 1 + Plena 2 + 1 hora) -> $36.000
    public async Task CheckOut_FullDayRecurrentCycles_CalculatesExactGrossFee(int durationMinutes, decimal expectedGross)
    {
        // Arrange: Tarifa Plena de $16.000, umbral activación 3h (180m), cobertura 8h (480m), hora $4.000, minuto $66.67
        var branch = new Branch
        {
            Id = 1,
            CompanyId = 10,
            AllowChargeByMinute = true,
            AllowChargeByHour = true,
            AllowChargeByDay = true,
            FullDayApplicableDays = "All"
        };

        var rate = new VehicleRate
        {
            RateId = Guid.NewGuid(),
            BranchId = 1,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            HourRate = 4000m,
            MinuteRate = 4000m / 60m,
            FullDayRate = 16000m,
            FullDayThresholdMinutes = 180, // 3 horas de activación
            FullDayCoverageMinutes = 480, // 8 horas de cobertura máxima
            GracePeriodMinutes = 15,
            IsActive = true
        };

        var entryUtc = new DateTime(2026, 9, 7, 13, 0, 0, DateTimeKind.Utc); // 08:00 a.m. COT
        var exitUtc = entryUtc.AddMinutes(durationMinutes);
        var ticketId = Guid.NewGuid();

        var ticket = new ParkingTicket
        {
            TicketId = ticketId,
            TicketNumber = "T-001",
            BranchId = 1,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            PlateNumber = "XYZ123",
            EntryTimeUtc = entryUtc,
            Status = TicketStatus.Active
        };

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _rateRepoMock.Setup(r => r.GetByTypeAsync(VehicleType.Car, 1, 10, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);
        _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ParkingTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var request = new CheckOutRequestDto
        {
            TicketId = ticketId,
            BranchId = 1,
            CompanyId = 10,
            ExitTimeUtc = exitUtc,
            PaymentMethod = PaymentMethodEnum.Cash
        };

        var result = await service.CheckOutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(expectedGross);
    }

    [Fact]
    public async Task CheckOut_NightStay_WhenExceedsMinStay_AppliesNightRate()
    {
        // Arrange: Noche de 20:00 a 06:00, Tarifa Nocturna $15.000, Mín. Permanencia 120 min (2h)
        var branch = new Branch
        {
            Id = 2,
            CompanyId = 10,
            AllowChargeByNight = true,
            NightApplicableDays = "All",
            NightStartTime = new TimeSpan(20, 0, 0),
            NightEndTime = new TimeSpan(6, 0, 0),
            NightStayMinMinutes = 120
        };

        var rate = new VehicleRate
        {
            RateId = Guid.NewGuid(),
            BranchId = 2,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            HourRate = 3500m,
            NightRate = 15000m,
            NightStartTime = new TimeSpan(20, 0, 0),
            NightEndTime = new TimeSpan(6, 0, 0),
            NightStayMinMinutes = 120,
            GracePeriodMinutes = 15,
            IsActive = true
        };

        var entryUtc = new DateTime(2026, 9, 7, 2, 0, 0, DateTimeKind.Utc); // 21:00 COT
        var exitUtc = entryUtc.AddMinutes(210); // 3h 30m
        var ticketId = Guid.NewGuid();

        var ticket = new ParkingTicket
        {
            TicketId = ticketId,
            TicketNumber = "T-002",
            BranchId = 2,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            PlateNumber = "NOC123",
            EntryTimeUtc = entryUtc,
            Status = TicketStatus.Active
        };

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _branchRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _rateRepoMock.Setup(r => r.GetByTypeAsync(VehicleType.Car, 2, 10, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);
        _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ParkingTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.CheckOutAsync(new CheckOutRequestDto
        {
            TicketId = ticketId,
            BranchId = 2,
            CompanyId = 10,
            ExitTimeUtc = exitUtc,
            PaymentMethod = PaymentMethodEnum.Cash
        });

        // Assert: Cobró Tarifa Nocturna completa ($15.000)
        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(15000m);
    }

    [Fact]
    public async Task CheckOut_NightStay_WhenBelowMinStay_ChargesRegularHoursNotNightRate()
    {
        // Arrange: Noche de 20:00 a 06:00, Tarifa Nocturna $15.000, Mín. Permanencia 120 min
        var branch = new Branch
        {
            Id = 2,
            CompanyId = 10,
            AllowChargeByNight = true,
            NightApplicableDays = "All",
            NightStartTime = new TimeSpan(20, 0, 0),
            NightEndTime = new TimeSpan(6, 0, 0),
            NightStayMinMinutes = 120
        };

        var rate = new VehicleRate
        {
            RateId = Guid.NewGuid(),
            BranchId = 2,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            HourRate = 3500m,
            NightRate = 15000m,
            NightStartTime = new TimeSpan(20, 0, 0),
            NightEndTime = new TimeSpan(6, 0, 0),
            NightStayMinMinutes = 120,
            GracePeriodMinutes = 15,
            IsActive = true
        };

        var entryUtc = new DateTime(2026, 9, 7, 1, 15, 0, DateTimeKind.Utc); // 20:15 COT
        var exitUtc = entryUtc.AddMinutes(60); // 1 hora
        var ticketId = Guid.NewGuid();

        var ticket = new ParkingTicket
        {
            TicketId = ticketId,
            TicketNumber = "T-003",
            BranchId = 2,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            PlateNumber = "CORTO1",
            EntryTimeUtc = entryUtc,
            Status = TicketStatus.Active
        };

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _branchRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _rateRepoMock.Setup(r => r.GetByTypeAsync(VehicleType.Car, 2, 10, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);
        _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ParkingTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.CheckOutAsync(new CheckOutRequestDto
        {
            TicketId = ticketId,
            BranchId = 2,
            CompanyId = 10,
            ExitTimeUtc = exitUtc,
            PaymentMethod = PaymentMethodEnum.Cash
        });

        // Assert: NO cobró los $15.000 de nocturna, cobró 1 hora normal ($3.500)
        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(3500m);
    }

    [Fact]
    public async Task CheckOut_SegmentedFullDayRulesJson_ResolvesCorrectRuleByDay()
    {
        // Arrange: Sede con JSON segmentado: L-V (días 1-5) umbral 3h / cobertura 8h; Sáb-Dom (6, 0) umbral 4h / cobertura 12h
        string rulesJson = @"[
            { ""days"": ""1,2,3,4,5"", ""triggerMinutes"": 180, ""coverageMinutes"": 480 },
            { ""days"": ""6,0"", ""triggerMinutes"": 240, ""coverageMinutes"": 720 }
        ]";

        var branch = new Branch
        {
            Id = 3,
            CompanyId = 10,
            AllowChargeByDay = true,
            FullDayRulesJson = rulesJson
        };

        var rate = new VehicleRate
        {
            RateId = Guid.NewGuid(),
            BranchId = 3,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            HourRate = 4000m,
            FullDayRate = 20000m,
            GracePeriodMinutes = 15,
            IsActive = true
        };

        // Martes (DayOfWeek = 2, L-V): 3.5 horas de permanencia -> supera trigger de 3h -> cobra Plena $20.000
        var entryTuesdayUtc = new DateTime(2026, 9, 8, 13, 0, 0, DateTimeKind.Utc); // Martes 8am COT
        var exitTuesdayUtc = entryTuesdayUtc.AddMinutes(210); // 3.5 horas
        var ticketId = Guid.NewGuid();

        var ticket = new ParkingTicket
        {
            TicketId = ticketId,
            TicketNumber = "T-004",
            BranchId = 3,
            CompanyId = 10,
            VehicleType = VehicleType.Car,
            PlateNumber = "SEG123",
            EntryTimeUtc = entryTuesdayUtc,
            Status = TicketStatus.Active
        };

        _ticketRepoMock.Setup(r => r.GetByIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _branchRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);
        _rateRepoMock.Setup(r => r.GetByTypeAsync(VehicleType.Car, 3, 10, It.IsAny<DayOfWeek>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);
        _ticketRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ParkingTicket>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.CheckOutAsync(new CheckOutRequestDto
        {
            TicketId = ticketId,
            BranchId = 3,
            CompanyId = 10,
            ExitTimeUtc = exitTuesdayUtc,
            PaymentMethod = PaymentMethodEnum.Cash
        });

        // Assert: El martes con 3.5 horas aplica Plena ($20.000)
        result.Should().NotBeNull();
        result!.GrossAmount.Should().Be(20000m);
    }
}
