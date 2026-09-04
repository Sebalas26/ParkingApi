using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.VehicleRates;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class VehicleRatesControllerTests
{
    private readonly Mock<IVehicleRateService> _rateServiceMock;
    private readonly Mock<IRealtimeNotificationService> _notifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<VehicleRatesController>> _loggerMock;
    private readonly VehicleRatesController _controller;

    public VehicleRatesControllerTests()
    {
        _rateServiceMock = new Mock<IVehicleRateService>();
        _notifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<VehicleRatesController>>();

        _controller = new VehicleRatesController(
            _rateServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnRates()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var rates = new List<VehicleRate>
        {
            new() { RateId = Guid.NewGuid(), DisplayName = "Carro", HourRate = 3500 }
        };
        _rateServiceMock.Setup(r => r.GetAllRatesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rates);

        // Act
        var result = await _controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(rates);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _rateServiceMock.Setup(r => r.GetAllRatesAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFoundAndAuthorized_ShouldReturnRate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rate = new VehicleRate { RateId = id, DisplayName = "Moto", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(rate);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleRate?)null);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenCompanyForbidden_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rate = new VehicleRate { RateId = id, DisplayName = "Moto", CompanyId = 99 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenSuccessfulWithBranch_ShouldReturnCreatedAtActionAndNotifyBranch()
    {
        // Arrange
        var rate = new VehicleRate { RateId = Guid.NewGuid(), BranchId = 2, DisplayName = "Camioneta", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _rateServiceMock.Setup(r => r.CreateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _controller.Create(rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(rate);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            2,
            "Tarifa de Vehículos Creada",
            It.IsAny<string>(),
            "RatesChanged",
            It.IsAny<CancellationToken>()), Times.Once);
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenSuccessfulWithoutBranch_ShouldNotNotifyBranch()
    {
        // Arrange
        var rate = new VehicleRate { RateId = Guid.NewGuid(), BranchId = null, DisplayName = "Moto Catalogo", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _rateServiceMock.Setup(r => r.CreateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _controller.Create(rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(rate);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var rate = new VehicleRate { DisplayName = "Error" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _rateServiceMock.Setup(r => r.CreateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.Create(rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenSuccessfulWithBranch_ShouldReturnOkAndNotifyBranch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rate = new VehicleRate { RateId = id, BranchId = 3, DisplayName = "Carro Actualizado" };
        _rateServiceMock.Setup(r => r.UpdateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rate);

        // Act
        var result = await _controller.Update(id, rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(rate);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            3,
            "Tarifas Actualizadas",
            It.IsAny<string>(),
            "RatesChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenKeyNotFoundException_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rate = new VehicleRate { RateId = id };
        _rateServiceMock.Setup(r => r.UpdateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Tarifa no encontrada"));

        // Act
        var result = await _controller.Update(id, rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var rate = new VehicleRate { RateId = id };
        _rateServiceMock.Setup(r => r.UpdateRateAsync(rate, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Update(id, rate, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessfulWithBranch_ShouldReturnOkAndNotifyBranch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new VehicleRate { RateId = id, BranchId = 4, DisplayName = "Tarifa Borrada" };
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _rateServiceMock.Setup(r => r.DeleteRateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            4,
            "Tarifa Eliminada",
            It.IsAny<string>(),
            "RatesChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _rateServiceMock.Setup(r => r.DeleteRateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new VehicleRate { RateId = id, BranchId = 1 };
        _rateServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _rateServiceMock.Setup(r => r.DeleteRateAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
