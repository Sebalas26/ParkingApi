using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Notifications;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Notifications;
using ParkingApi.Infrastructure.Data;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class NotificationsControllerTests
{
    private readonly Mock<IPushNotificationService> _pushServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<NotificationsController>> _loggerMock;
    private readonly NotificationsController _controller;

    public NotificationsControllerTests()
    {
        _pushServiceMock = new Mock<IPushNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<NotificationsController>>();

        _controller = new NotificationsController(
            _pushServiceMock.Object,
            _currentUserMock.Object,
            null!, // DataContext not needed for broadcast
            _configMock.Object,
            _loggerMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task BroadcastVersion_WithInvalidSecret_ShouldReturnUnauthorized()
    {
        // Arrange
        _configMock.Setup(c => c["Deploy:SecretKey"]).Returns("CORRECT_SECRET");
        _controller.HttpContext.Request.Headers["X-Deploy-Key"] = "WRONG_SECRET";

        var dto = new BroadcastVersionRequestDto { Version = "0.0.168 Dev" };

        // Act
        var result = await _controller.BroadcastVersion(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task BroadcastVersion_WithValidSecret_ShouldCallServiceAndReturnOk()
    {
        // Arrange
        var secret = "PARKFLOW_DEPLOY_KEY_2026_AUTOMATION_SECRET";
        _configMock.Setup(c => c["Deploy:SecretKey"]).Returns(secret);
        _controller.HttpContext.Request.Headers["X-Deploy-Key"] = secret;

        var dto = new BroadcastVersionRequestDto { Version = "0.0.168 Dev" };
        _pushServiceMock.Setup(s => s.BroadcastVersionNotificationAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.BroadcastVersion(dto, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        _pushServiceMock.Verify(s => s.BroadcastVersionNotificationAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
    }
}
