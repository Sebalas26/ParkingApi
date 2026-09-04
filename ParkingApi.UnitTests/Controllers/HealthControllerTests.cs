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
}
