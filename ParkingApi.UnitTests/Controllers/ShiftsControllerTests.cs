using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Shifts;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Shifts;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ShiftsControllerTests
{
    private readonly Mock<IShiftService> _shiftServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<ShiftsController>> _loggerMock;
    private readonly ShiftsController _controller;

    public ShiftsControllerTests()
    {
        _shiftServiceMock = new Mock<IShiftService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<ShiftsController>>();

        _controller = new ShiftsController(
            _shiftServiceMock.Object,
            _currentUserMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "operador1"),
            new Claim(ClaimTypes.Role, "Administrador")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task OpenShift_WhenSuccessful_ShouldReturnOkWithShift()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        var request = new OpenShiftRequestDto { BranchId = 1, BaseAmount = 50000 };
        var shift = new WorkShiftDto { ShiftId = Guid.NewGuid(), Status = ShiftStatus.Open, BaseAmount = 50000 };
        _shiftServiceMock.Setup(s => s.OpenShiftAsync(1, "operador1", request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        // Act
        var result = await _controller.OpenShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(shift);
    }

    [Fact]
    public async Task OpenShift_WhenServiceReturnsNull_ShouldReturnBadRequest()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        var request = new OpenShiftRequestDto { BranchId = 1 };
        _shiftServiceMock.Setup(s => s.OpenShiftAsync(It.IsAny<int>(), It.IsAny<string>(), request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkShiftDto?)null);

        // Act
        var result = await _controller.OpenShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OpenShift_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        var request = new OpenShiftRequestDto { BranchId = 1 };
        _shiftServiceMock.Setup(s => s.OpenShiftAsync(It.IsAny<int>(), It.IsAny<string>(), request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ya existe un turno activo"));

        // Act
        var result = await _controller.OpenShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task OpenShift_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        _shiftServiceMock.Setup(s => s.OpenShiftAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<OpenShiftRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.OpenShift(new OpenShiftRequestDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenFound_ShouldReturnShift()
    {
        // Arrange
        var shift = new WorkShiftDto { ShiftId = Guid.NewGuid(), Status = ShiftStatus.Open };
        _shiftServiceMock.Setup(s => s.GetActiveShiftAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shift);

        // Act
        var result = await _controller.GetActive(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(shift);
    }

    [Fact]
    public async Task GetActive_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _shiftServiceMock.Setup(s => s.GetActiveShiftAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkShiftDto?)null);

        // Act
        var result = await _controller.GetActive(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _shiftServiceMock.Setup(s => s.GetActiveShiftAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error getting active shift"));

        // Act
        var result = await _controller.GetActive(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActiveList_WhenSuccessful_ShouldReturnShifts()
    {
        // Arrange
        var list = new List<WorkShiftDto> { new() { ShiftId = Guid.NewGuid(), Status = ShiftStatus.Open } };
        _shiftServiceMock.Setup(s => s.GetActiveShiftsAsync(null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetActiveList(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetActiveList_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _shiftServiceMock.Setup(s => s.GetActiveShiftsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetActiveList(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetSummary_WhenFound_ShouldReturnSummary()
    {
        // Arrange
        var id = Guid.NewGuid();
        var summary = new ShiftSummaryDto { ShiftId = id, TotalCashCollected = 20000 };
        _shiftServiceMock.Setup(s => s.GetShiftSummaryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        // Act
        var result = await _controller.GetSummary(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(summary);
    }

    [Fact]
    public async Task GetSummary_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _shiftServiceMock.Setup(s => s.GetShiftSummaryAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShiftSummaryDto?)null);

        // Act
        var result = await _controller.GetSummary(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetSummary_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _shiftServiceMock.Setup(s => s.GetShiftSummaryAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetSummary(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CloseShift_WhenSuccessful_ShouldReturnClosedShift()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        var request = new CloseShiftRequestDto { ShiftId = Guid.NewGuid(), ActualCashCounted = 60000 };
        var closed = new WorkShiftDto { ShiftId = request.ShiftId, Status = ShiftStatus.Closed };
        _shiftServiceMock.Setup(s => s.CloseShiftAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closed);

        // Act
        var result = await _controller.CloseShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(closed);
    }

    [Fact]
    public async Task CloseShift_WhenServiceReturnsNull_ShouldReturnBadRequest()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        var request = new CloseShiftRequestDto { ShiftId = Guid.NewGuid() };
        _shiftServiceMock.Setup(s => s.CloseShiftAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkShiftDto?)null);

        // Act
        var result = await _controller.CloseShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CloseShift_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");
        _shiftServiceMock.Setup(s => s.CloseShiftAsync(It.IsAny<int>(), It.IsAny<CloseShiftRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.CloseShift(new CloseShiftRequestDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetHistory_WhenSuccessful_ShouldReturnList()
    {
        // Arrange
        var list = new List<WorkShiftDto> { new() { ShiftId = Guid.NewGuid(), Status = ShiftStatus.Closed } };
        _shiftServiceMock.Setup(s => s.GetHistoryAsync(null, null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetHistory(null, null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetHistory_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _shiftServiceMock.Setup(s => s.GetHistoryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Query error"));

        // Act
        var result = await _controller.GetHistory(null, null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
