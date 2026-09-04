using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Incidents;
using ParkingApi.Domain.Interfaces.Services.Incidents;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class VehicleIncidentsControllerTests
{
    private readonly Mock<IVehicleIncidentService> _incidentServiceMock;
    private readonly Mock<IRealtimeNotificationService> _notifierMock;
    private readonly Mock<ILogger<VehicleIncidentsController>> _loggerMock;
    private readonly VehicleIncidentsController _controller;

    public VehicleIncidentsControllerTests()
    {
        _incidentServiceMock = new Mock<IVehicleIncidentService>();
        _notifierMock = new Mock<IRealtimeNotificationService>();
        _loggerMock = new Mock<ILogger<VehicleIncidentsController>>();

        _controller = new VehicleIncidentsController(
            _incidentServiceMock.Object,
            _notifierMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnIncidents()
    {
        // Arrange
        var list = new List<VehicleIncidentDto>
        {
            new() { IncidentId = Guid.NewGuid(), PlateNumber = "ABC123", Description = "Golpe leve" }
        };
        _incidentServiceMock.Setup(i => i.GetAllAsync(null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetAll(null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _incidentServiceMock.Setup(i => i.GetAllAsync(It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<bool?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetAll(null, null, null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnIncident()
    {
        // Arrange
        var id = Guid.NewGuid();
        var incident = new VehicleIncidentDto { IncidentId = id, PlateNumber = "XYZ789" };
        _incidentServiceMock.Setup(i => i.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(incident);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(incident);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _incidentServiceMock.Setup(i => i.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleIncidentDto?)null);

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
        _incidentServiceMock.Setup(i => i.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CheckPlate_WhenPlateIsEmpty_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.CheckPlate("", null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckPlate_WhenSuccessful_ShouldReturnResult()
    {
        // Arrange
        var checkResult = new PlateCheckResultDto { PlateNumber = "ABC123", IsBlocked = false };
        _incidentServiceMock.Setup(i => i.CheckPlateAsync("ABC123", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkResult);

        // Act
        var result = await _controller.CheckPlate("ABC123", null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(checkResult);
    }

    [Fact]
    public async Task CheckPlate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _incidentServiceMock.Setup(i => i.CheckPlateAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fault"));

        // Act
        var result = await _controller.CheckPlate("ABC123", null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetByPlate_WhenSuccessful_ShouldReturnList()
    {
        // Arrange
        var list = new List<VehicleIncidentDto> { new() { IncidentId = Guid.NewGuid(), PlateNumber = "ABC123" } };
        _incidentServiceMock.Setup(i => i.GetByPlateAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetByPlate("ABC123", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetByPlate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _incidentServiceMock.Setup(i => i.GetByPlateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fault"));

        // Act
        var result = await _controller.GetByPlate("ABC123", CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenFieldsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SaveVehicleIncidentDto { PlateNumber = "", IncidentType = "", Description = "" };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtActionAndNotify()
    {
        // Arrange
        var dto = new SaveVehicleIncidentDto { PlateNumber = "ABC123", IncidentType = "Mecánica", Description = "Varado", BranchId = 1, IsBlocked = true };
        var created = new VehicleIncidentDto { IncidentId = Guid.NewGuid(), PlateNumber = dto.PlateNumber };
        _incidentServiceMock.Setup(i => i.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            It.IsAny<string>(),
            It.IsAny<string>(),
            "IncidentsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new SaveVehicleIncidentDto { PlateNumber = "ABC123", IncidentType = "Daño", Description = "Rayón" };
        _incidentServiceMock.Setup(i => i.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Crash"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenFieldsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveVehicleIncidentDto { PlateNumber = "" };

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenValid_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveVehicleIncidentDto { PlateNumber = "ABC123", IncidentType = "Daño", Description = "Actualizado", BranchId = 1 };
        var updated = new VehicleIncidentDto { IncidentId = id, PlateNumber = dto.PlateNumber };
        _incidentServiceMock.Setup(i => i.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(updated);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            It.IsAny<string>(),
            It.IsAny<string>(),
            "IncidentsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveVehicleIncidentDto { PlateNumber = "ABC123", IncidentType = "Daño", Description = "Actualizado" };
        _incidentServiceMock.Setup(i => i.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VehicleIncidentDto?)null);

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveVehicleIncidentDto { PlateNumber = "ABC123", IncidentType = "Daño", Description = "Actualizado" };
        _incidentServiceMock.Setup(i => i.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Resolve_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new ResolveIncidentDto { ResolvedNotes = "Solucionado" };
        _incidentServiceMock.Setup(i => i.ResolveAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Resolve(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "IncidentsChanged",
            "Novedad Resuelta",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Resolve_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _incidentServiceMock.Setup(i => i.ResolveAsync(id, It.IsAny<ResolveIncidentDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Resolve(id, new ResolveIncidentDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Resolve_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _incidentServiceMock.Setup(i => i.ResolveAsync(id, It.IsAny<ResolveIncidentDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Resolve(id, new ResolveIncidentDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        _incidentServiceMock.Setup(i => i.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "IncidentsChanged",
            "Novedad Eliminada",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _incidentServiceMock.Setup(i => i.DeleteAsync(id, It.IsAny<CancellationToken>()))
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
        _incidentServiceMock.Setup(i => i.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
