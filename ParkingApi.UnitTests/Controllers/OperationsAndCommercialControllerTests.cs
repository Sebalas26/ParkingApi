using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Dtos.ParkingLots;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Interfaces.Services.Analytics;
using ParkingApi.Domain.Interfaces.Services.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Services.ParkingLots;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Interfaces.Services.Sync;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class OperationsAndCommercialControllerTests
{
    private readonly Mock<IStoreService> _storeServiceMock = new();
    private readonly Mock<ICommercialAgreementService> _agreementServiceMock = new();
    private readonly Mock<IParkingLotService> _parkingLotServiceMock = new();
    private readonly Mock<IMonthlySubscriptionService> _subscriptionServiceMock = new();
    private readonly Mock<IIdentificationTypeService> _identificationTypeServiceMock = new();
    private readonly Mock<IAnalyticsService> _analyticsServiceMock = new();
    private readonly Mock<ISyncService> _syncServiceMock = new();
    private readonly Mock<IRealtimeNotificationService> _notifierMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private readonly Mock<ILogger<StoresController>> _storesLoggerMock = new();
    private readonly Mock<ILogger<AgreementsController>> _agreementsLoggerMock = new();
    private readonly Mock<ILogger<ParkingLotsController>> _lotsLoggerMock = new();
    private readonly Mock<ILogger<MonthlySubscriptionsController>> _subsLoggerMock = new();
    private readonly Mock<ILogger<IdentificationTypesController>> _idTypesLoggerMock = new();
    private readonly Mock<ILogger<AnalyticsController>> _analyticsLoggerMock = new();
    private readonly Mock<ILogger<SyncController>> _syncLoggerMock = new();
    private readonly Mock<ILogger<HealthController>> _healthLoggerMock = new();

    [Fact]
    public async Task StoresController_GetAll_ShouldReturnStores()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new StoresController(
            _storeServiceMock.Object,
            _currentUserMock.Object,
            _storesLoggerMock.Object);

        var stores = new List<Store>
        {
            new() { StoreId = Guid.NewGuid(), Name = "Cafetería Central", TaxId = "900111222" }
        };

        _storeServiceMock.Setup(s => s.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stores);

        // Act
        var result = await controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(stores);
    }

    [Fact]
    public async Task AgreementsController_GetAll_ShouldReturnAgreements()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new AgreementsController(
            _agreementServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _agreementsLoggerMock.Object);

        var agreements = new List<CommercialAgreement>
        {
            new() { AgreementId = Guid.NewGuid(), Name = "Convenio Éxito", DiscountPercentage = 50 }
        };

        _agreementServiceMock.Setup(a => a.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreements);

        // Act
        var result = await controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(agreements);
    }

    [Fact]
    public async Task ParkingLotsController_GetParkingLots_ShouldReturnParkingLots()
    {
        // Arrange
        var controller = new ParkingLotsController(
            _lotsLoggerMock.Object,
            _parkingLotServiceMock.Object);

        var lots = new List<ParkingLotDto>
        {
            new() { Id = 1, Name = "Plaza A-01", Description = "Parqueadero Principal" }
        };

        _parkingLotServiceMock.Setup(p => p.GetParkingLotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);

        // Act
        var result = await controller.GetParkingLots(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(lots);
    }

    [Fact]
    public async Task MonthlySubscriptionsController_GetAll_ShouldReturnSubscriptions()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new MonthlySubscriptionsController(
            _subsLoggerMock.Object,
            _subscriptionServiceMock.Object,
            _currentUserMock.Object);

        var subs = new List<MonthlySubscriptionDto>
        {
            new() { SubscriptionId = Guid.NewGuid(), CustomerName = "Juan Lopez", PlateNumber = "MNO456", MonthlyFee = 150000 }
        };

        _subscriptionServiceMock.Setup(s => s.GetAllAsync(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subs);

        // Act
        var result = await controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(subs);
    }

    [Fact]
    public async Task IdentificationTypesController_Get_ShouldReturnTypes()
    {
        // Arrange
        var controller = new IdentificationTypesController(
            _idTypesLoggerMock.Object,
            _identificationTypeServiceMock.Object);

        var types = new List<GetIdentificationTypeDto>
        {
            new() { Id = 1, Name = "Cédula de Ciudadanía" }
        };

        _identificationTypeServiceMock.Setup(i => i.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(types);

        // Act
        var result = await controller.GetIdentificationTypes(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(types);
    }

    [Fact]
    public async Task AnalyticsController_GetDailySummary_ShouldReturnSummary()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new AnalyticsController(
            _analyticsServiceMock.Object,
            _currentUserMock.Object,
            _analyticsLoggerMock.Object);

        var summary = new FinancialSummaryDto
        {
            TotalRevenueToday = 500000,
            ActiveVehiclesCount = 45,
            CompletedTransactionsToday = 40
        };

        _analyticsServiceMock.Setup(a => a.GetDailySummaryAsync(null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await controller.GetDailySummary(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(summary);
    }

    [Fact]
    public async Task SyncController_GetBootstrap_ShouldReturnBootstrapData()
    {
        // Arrange
        var controller = new SyncController(_syncServiceMock.Object, _syncLoggerMock.Object);
        var syncData = new BootstrapSyncDto { ServerTimeUtc = DateTime.UtcNow };

        _syncServiceMock.Setup(s => s.GetBootstrapDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(syncData);

        // Act
        var result = await controller.GetBootstrap(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(syncData);
    }

    [Fact]
    public void HealthController_Check_ShouldReturnHealthy()
    {
        // Arrange
        var controller = new HealthController(_healthLoggerMock.Object);

        // Act
        var result = controller.Check();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
