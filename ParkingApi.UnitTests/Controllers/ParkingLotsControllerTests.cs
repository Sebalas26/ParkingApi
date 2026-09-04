using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.ParkingLots;
using ParkingApi.Domain.Interfaces.Services.ParkingLots;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ParkingLotsControllerTests
{
    private readonly Mock<IParkingLotService> _parkingLotServiceMock;
    private readonly Mock<ILogger<ParkingLotsController>> _loggerMock;
    private readonly ParkingLotsController _controller;

    public ParkingLotsControllerTests()
    {
        _parkingLotServiceMock = new Mock<IParkingLotService>();
        _loggerMock = new Mock<ILogger<ParkingLotsController>>();
        _controller = new ParkingLotsController(_loggerMock.Object, _parkingLotServiceMock.Object);
    }

    [Fact]
    public async Task GetParkingLots_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var lots = new List<ParkingLotDto>
        {
            new() { Id = 1, Name = "Plaza Central", Description = "Parqueadero Principal" }
        };
        _parkingLotServiceMock.Setup(s => s.GetParkingLotsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(lots);

        // Act
        var result = await _controller.GetParkingLots(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(lots);
    }

    [Fact]
    public async Task GetParkingLots_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.GetParkingLotsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failure"));

        // Act
        var result = await _controller.GetParkingLots(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetParkingLotById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var lot = new ParkingLotDto { Id = 1, Name = "Plaza Norte" };
        _parkingLotServiceMock.Setup(s => s.GetParkingLotByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lot);

        // Act
        var result = await _controller.GetParkingLotById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(lot);
    }

    [Fact]
    public async Task GetParkingLotById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.GetParkingLotByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingLotDto?)null);

        // Act
        var result = await _controller.GetParkingLotById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetParkingLotById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.GetParkingLotByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Timeout"));

        // Act
        var result = await _controller.GetParkingLotById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEdit_WhenNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SaveParkingLotDto { Name = "" };

        // Act
        var result = await _controller.SaveOrEdit(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SaveOrEdit_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new SaveParkingLotDto { Name = "Plaza VIP", Description = "Zona VIP" };
        var saved = new ParkingLotDto { Id = 5, Name = "Plaza VIP", Description = "Zona VIP" };
        _parkingLotServiceMock.Setup(s => s.SaveOrEditParkingLotAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(saved);

        // Act
        var result = await _controller.SaveOrEdit(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(saved);
    }

    [Fact]
    public async Task SaveOrEdit_WhenServiceReturnsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SaveParkingLotDto { Name = "Plaza Error" };
        _parkingLotServiceMock.Setup(s => s.SaveOrEditParkingLotAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingLotDto?)null);

        // Act
        var result = await _controller.SaveOrEdit(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SaveOrEdit_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new SaveParkingLotDto { Name = "Plaza Error" };
        _parkingLotServiceMock.Setup(s => s.SaveOrEditParkingLotAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Insert fault"));

        // Act
        var result = await _controller.SaveOrEdit(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Deactivate_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.DeactivateParkingLotAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.DeactivateParkingLotAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Deactivate(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Deactivate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _parkingLotServiceMock.Setup(s => s.DeactivateParkingLotAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Deactivation crash"));

        // Act
        var result = await _controller.Deactivate(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
