using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Services.Sync;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class SyncControllerTests
{
    private readonly Mock<ISyncService> _syncServiceMock;
    private readonly Mock<ILogger<SyncController>> _loggerMock;
    private readonly SyncController _controller;

    public SyncControllerTests()
    {
        _syncServiceMock = new Mock<ISyncService>();
        _loggerMock = new Mock<ILogger<SyncController>>();
        _controller = new SyncController(_syncServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetBootstrap_WhenSuccessful_ShouldReturnOkWithBootstrapData()
    {
        // Arrange
        var data = new BootstrapSyncDto { ServerTimeUtc = DateTime.UtcNow };
        _syncServiceMock.Setup(s => s.GetBootstrapDataAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(data);

        // Act
        var result = await _controller.GetBootstrap(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(data);
    }

    [Fact]
    public async Task GetBootstrap_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _syncServiceMock.Setup(s => s.GetBootstrapDataAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Sync error"));

        // Act
        var result = await _controller.GetBootstrap(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
