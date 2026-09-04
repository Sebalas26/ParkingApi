using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.UserRoles;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class UserRoleControllerTests
{
    private readonly Mock<IUserRoleService> _userRoleServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<UserRoleController>> _loggerMock;
    private readonly UserRoleController _controller;

    public UserRoleControllerTests()
    {
        _userRoleServiceMock = new Mock<IUserRoleService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<UserRoleController>>();

        _controller = new UserRoleController(
            _loggerMock.Object,
            _userRoleServiceMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task GetUsersRoles_WhenSuccessful_ShouldReturnRoles()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var roles = new List<GetUserRoleDto>
        {
            new() { IdUserRol = 1, RoleName = "Administrador" }
        };
        _userRoleServiceMock.Setup(r => r.GetUserRoles(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await _controller.GetUsersRoles(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task GetUsersRoles_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _userRoleServiceMock.Setup(r => r.GetUserRoles(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetUsersRoles(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUserRoleById_WhenFoundAndAuthorized_ShouldReturnRole()
    {
        // Arrange
        var role = new GetUserRoleDto { IdUserRol = 1, RoleName = "Cajero", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _userRoleServiceMock.Setup(r => r.GetUserRoleById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _controller.GetUserRoleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(role);
    }

    [Fact]
    public async Task GetUserRoleById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.GetUserRoleById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserRoleDto?)null);

        // Act
        var result = await _controller.GetUserRoleById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserRoleById_WhenCompanyForbidden_ShouldReturn404()
    {
        // Arrange
        var role = new GetUserRoleDto { IdUserRol = 1, RoleName = "Operador", CompanyId = 99 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _userRoleServiceMock.Setup(r => r.GetUserRoleById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _controller.GetUserRoleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserRoleById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.GetUserRoleById(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetUserRoleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditUserRole_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var role = new GetUserRoleDto { IdUserRol = 1, RoleName = "Supervisor", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _userRoleServiceMock.Setup(r => r.SaveOrEditUserRole(role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // Act
        var result = await _controller.SaveOrEditUserRole(role, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(role);
    }

    [Fact]
    public async Task SaveOrEditUserRole_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var role = new GetUserRoleDto { IdUserRol = 1, RoleName = "Error" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _userRoleServiceMock.Setup(r => r.SaveOrEditUserRole(role, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failure"));

        // Act
        var result = await _controller.SaveOrEditUserRole(role, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteUserRole_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.DeleteUserRole(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUserRole(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteUserRole_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.DeleteUserRole(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUserRole(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUserRole_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.DeleteUserRole(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No se puede eliminar rol con usuarios asociados"));

        // Act
        var result = await _controller.DeleteUserRole(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteUserRole_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _userRoleServiceMock.Setup(r => r.DeleteUserRole(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete fault"));

        // Act
        var result = await _controller.DeleteUserRole(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
