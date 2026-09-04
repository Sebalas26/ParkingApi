using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Analytics;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class AnalyticsControllerTests
{
    private readonly Mock<IAnalyticsService> _analyticsServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<AnalyticsController>> _loggerMock;
    private readonly AnalyticsController _controller;

    public AnalyticsControllerTests()
    {
        _analyticsServiceMock = new Mock<IAnalyticsService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<AnalyticsController>>();

        _controller = new AnalyticsController(
            _analyticsServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetDailySummary_WhenSuccessful_ShouldReturnOkWithSummary()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var summary = new FinancialSummaryDto
        {
            TotalRevenueToday = 350000,
            ActiveVehiclesCount = 12,
            CompletedTransactionsToday = 25
        };
        _analyticsServiceMock.Setup(s => s.GetDailySummaryAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _controller.GetDailySummary(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(summary);
    }

    [Fact]
    public async Task GetDailySummary_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _analyticsServiceMock.Setup(s => s.GetDailySummaryAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database timeout"));

        // Act
        var result = await _controller.GetDailySummary(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetOccupancy_WhenSuccessful_ShouldReturnOkWithStats()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(2);
        var stats = new OccupancyStatsDto
        {
            TotalCapacity = 100,
            OccupiedSpots = 45
        };
        _analyticsServiceMock.Setup(s => s.GetOccupancyStatsAsync(null, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetOccupancy(null, 2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(stats);
    }

    [Fact]
    public async Task GetOccupancy_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(2);
        _analyticsServiceMock.Setup(s => s.GetOccupancyStatsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calculation error"));

        // Act
        var result = await _controller.GetOccupancy(null, 2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetPeakTraffic_WhenSuccessful_ShouldReturnOkWithReport()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var report = new PeakTrafficReportDto
        {
            Period = "today",
            PeakHourLabel = "14:00 - 15:00"
        };
        _analyticsServiceMock.Setup(s => s.GetPeakTrafficAsync("today", 1, 1, 300, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Act
        var result = await _controller.GetPeakTraffic("today", 1, 1, 300, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(report);
    }

    [Fact]
    public async Task GetPeakTraffic_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _analyticsServiceMock.Setup(s => s.GetPeakTrafficAsync(It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Analytics engine failure"));

        // Act
        var result = await _controller.GetPeakTraffic("today", 1, 1, 300, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
