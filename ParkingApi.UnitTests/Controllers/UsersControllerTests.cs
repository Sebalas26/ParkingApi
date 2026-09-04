using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.Users;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IRealtimeNotificationService> _notifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<UsersController>> _loggerMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _notifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<UsersController>>();

        _controller = new UsersController(
            _loggerMock.Object,
            _userServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task GetUsers_WhenSuccessful_ShouldReturnUsers()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var users = new List<GetUsersDto> { new() { Id = 1, Username = "operador1" } };
        _userServiceMock.Setup(u => u.GetUsers(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetUsers(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task GetUsers_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _userServiceMock.Setup(u => u.GetUsers(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetUsers(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUserById_WhenFoundAndAuthorized_ShouldReturnUser()
    {
        // Arrange
        var user = new GetUsersDto { Id = 1, Username = "cajero", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _userServiceMock.Setup(u => u.GetUserById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetUserById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetUserById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUsersDto?)null);

        // Act
        var result = await _controller.GetUserById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserById_WhenCompanyForbidden_ShouldReturn404()
    {
        // Arrange
        var user = new GetUsersDto { Id = 1, Username = "otro", CompanyId = 99 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _userServiceMock.Setup(u => u.GetUserById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _userServiceMock.Setup(u => u.GetUserById(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetUserById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditUsers_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var dto = new GetUsersDto { Id = 1, Username = "juan", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _userServiceMock.Setup(u => u.CreateOrEditUser(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.SaveOrEditUsers(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "UsersChanged",
            "Usuarios Actualizados",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveOrEditUsers_WhenServiceReturnsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new GetUsersDto { Id = 1, Username = "juan" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _userServiceMock.Setup(u => u.CreateOrEditUser(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUsersDto?)null);

        // Act
        var result = await _controller.SaveOrEditUsers(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SaveOrEditUsers_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new GetUsersDto { Id = 1, Username = "juan" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _userServiceMock.Setup(u => u.CreateOrEditUser(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.SaveOrEditUsers(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        _userServiceMock.Setup(u => u.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "UsersChanged",
            "Usuario Eliminado",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _userServiceMock.Setup(u => u.DeleteUserAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _userServiceMock.Setup(u => u.DeleteUserAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete fault"));

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
