using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Incidents;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Incidents;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.VehicleRates;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class VehicleRatesAndIncidentsControllerTests
{
    private readonly Mock<IVehicleRateService> _rateServiceMock = new();
    private readonly Mock<IVehicleIncidentService> _incidentServiceMock = new();
    private readonly Mock<IRealtimeNotificationService> _notifierMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<VehicleRatesController>> _ratesLoggerMock = new();
    private readonly Mock<ILogger<VehicleIncidentsController>> _incidentsLoggerMock = new();

    [Fact]
    public async Task VehicleRatesController_GetAll_ShouldReturnRates()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new VehicleRatesController(
            _rateServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _ratesLoggerMock.Object);

        var rates = new List<VehicleRate>
        {
            new() { RateId = Guid.NewGuid(), DisplayName = "Carro", HourRate = 3500, MinuteRate = 60, FullDayRate = 25000, NightRate = 12000 },
            new() { RateId = Guid.NewGuid(), DisplayName = "Moto", HourRate = 1800, MinuteRate = 30, FullDayRate = 12000, NightRate = 6000 }
        };

        _rateServiceMock.Setup(r => r.GetAllRatesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        // Act
        var result = await controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(rates);
    }

    [Fact]
    public async Task VehicleRatesController_Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        var controller = new VehicleRatesController(
            _rateServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _ratesLoggerMock.Object);

        var newRate = new VehicleRate
        {
            DisplayName = "Bicicleta",
            VehicleType = VehicleType.Bicycle,
            HourRate = 800,
            NightRate = 1500
        };

        _rateServiceMock.Setup(r => r.CreateRateAsync(newRate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRate);

        // Act
        var result = await controller.Create(newRate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task VehicleIncidentsController_GetAll_ShouldReturnIncidents()
    {
        // Arrange
        var controller = new VehicleIncidentsController(
            _incidentServiceMock.Object,
            _notifierMock.Object,
            _incidentsLoggerMock.Object);

        var incidents = new List<VehicleIncidentDto>
        {
            new() { IncidentId = Guid.NewGuid(), PlateNumber = "XYZ789", Description = "Rayón en puerta", Status = "Activa" }
        };

        _incidentServiceMock.Setup(i => i.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(incidents);

        // Act
        var result = await controller.GetAll(null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(incidents);
    }
}
