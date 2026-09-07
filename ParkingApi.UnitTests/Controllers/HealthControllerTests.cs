using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class HealthControllerTests
{
    private readonly Mock<ILogger<HealthController>> _loggerMock;
    private readonly HealthController _controller;

    public HealthControllerTests()
    {
        _loggerMock = new Mock<ILogger<HealthController>>();
        _controller = new HealthController(_loggerMock.Object);
    }

    [Fact]
    public void Check_WhenHealthy_ShouldReturnOk()
    {
        // Act
        var result = _controller.Check();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void VapidKeys_ShouldBeValid()
    {
        var pub = "BATQc7nVdBRKAeCGfR-5fPsHAKEcLqen4tHHHlV_9XkzRSQpzXQyQf0SxZ_7YjTQtijdLeqbMc9k7riX6mHBgAE";
        var priv = "MrwFiv50yI6Ac8SFwJdPE_R18cModfpT--8Md1BjKP0";
        var vapid = new WebPush.VapidDetails("mailto:soporte@parking-flow.com", pub, priv);
        vapid.PublicKey.Should().Be(pub);
        vapid.PrivateKey.Should().Be(priv);
    }
}
