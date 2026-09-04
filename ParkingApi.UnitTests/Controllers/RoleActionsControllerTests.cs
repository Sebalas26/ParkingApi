using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.RoleActions;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class RoleActionsControllerTests
{
    private readonly Mock<IRoleActionService> _roleActionServiceMock;
    private readonly Mock<IRealtimeNotificationService> _realtimeNotifierMock;
    private readonly Mock<ILogger<RoleActionsController>> _loggerMock;
    private readonly RoleActionsController _controller;

    public RoleActionsControllerTests()
    {
        _roleActionServiceMock = new Mock<IRoleActionService>();
        _realtimeNotifierMock = new Mock<IRealtimeNotificationService>();
        _loggerMock = new Mock<ILogger<RoleActionsController>>();

        _controller = new RoleActionsController(
            _loggerMock.Object,
            _roleActionServiceMock.Object,
            _realtimeNotifierMock.Object);
    }

    [Fact]
    public async Task GetRoleActions_WhenSuccessful_ShouldReturnActions()
    {
        // Arrange
        var actions = new List<string> { "checkin.view", "checkin.create" };
        _roleActionServiceMock.Setup(r => r.GetActionsByRoleIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        // Act
        var result = await _controller.GetRoleActions(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(actions);
    }

    [Fact]
    public async Task GetRoleActions_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _roleActionServiceMock.Setup(r => r.GetActionsByRoleIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetRoleActions(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task PermissionRole_WhenSuccessful_ShouldReturnPermissions()
    {
        // Arrange
        var permissions = new List<ActionsRoleDto>
        {
            new() { ActionId = 1, ActionName = "Ver Tiquetes", IsActive = true }
        };
        _roleActionServiceMock.Setup(r => r.GetActionsByRoleAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        // Act
        var result = await _controller.PermissionRole(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(permissions);
    }

    [Fact]
    public async Task PermissionRole_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _roleActionServiceMock.Setup(r => r.GetActionsByRoleAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Permission service error"));

        // Act
        var result = await _controller.PermissionRole(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task AssignRolePermissions_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var dto = new AssignRolePermissionsDto { RoleId = 2, ActionIds = new List<int> { 1, 2 } };
        _roleActionServiceMock.Setup(r => r.AssignRolePermissionsAsync(2, dto.ActionIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignRolePermissions(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _realtimeNotifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "PermissionsChanged",
            "Permisos Actualizados",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRolePermissions_WhenFailed_ShouldReturnOkWithSuccessFalse()
    {
        // Arrange
        var dto = new AssignRolePermissionsDto { RoleId = 2, ActionIds = new List<int> { 1 } };
        _roleActionServiceMock.Setup(r => r.AssignRolePermissionsAsync(2, dto.ActionIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignRolePermissions(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssignRolePermissions_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new AssignRolePermissionsDto { RoleId = 2 };
        _roleActionServiceMock.Setup(r => r.AssignRolePermissionsAsync(It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Assignment failure"));

        // Act
        var result = await _controller.AssignRolePermissions(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
