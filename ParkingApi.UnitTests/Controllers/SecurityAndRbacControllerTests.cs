using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Modules;
using ParkingApi.Domain.Interfaces.Services.Operations;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.RoleActions;
using ParkingApi.Domain.Interfaces.Services.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services.UserRoles;
using ParkingApi.Domain.Interfaces.Services.Users;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class SecurityAndRbacControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IUserRoleService> _userRoleServiceMock = new();
    private readonly Mock<IRoleActionService> _roleActionServiceMock = new();
    private readonly Mock<IModuleService> _moduleServiceMock = new();
    private readonly Mock<IOperationService> _operationServiceMock = new();
    private readonly Mock<IUserRoleModuleService> _userRoleModuleServiceMock = new();
    private readonly Mock<IRealtimeNotificationService> _notifierMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private readonly Mock<ILogger<UsersController>> _usersLoggerMock = new();
    private readonly Mock<ILogger<UserRoleController>> _roleLoggerMock = new();
    private readonly Mock<ILogger<RoleActionsController>> _roleActionsLoggerMock = new();
    private readonly Mock<ILogger<ModuleController>> _moduleLoggerMock = new();
    private readonly Mock<ILogger<OperationController>> _operationLoggerMock = new();
    private readonly Mock<ILogger<UserRoleModuleController>> _urmLoggerMock = new();

    [Fact]
    public async Task UsersController_GetUsers_ShouldReturnUsersList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new UsersController(
            _usersLoggerMock.Object,
            _userServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object);

        var users = new List<GetUsersDto>
        {
            new() { Id = 1, Username = "operador1", FullName = "Carlos Perez" }
        };

        _userServiceMock.Setup(u => u.GetUsers(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await controller.GetUsers(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task UserRoleController_GetUsersRoles_ShouldReturnRoles()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new UserRoleController(
            _roleLoggerMock.Object,
            _userRoleServiceMock.Object,
            _currentUserMock.Object);

        var roles = new List<GetUserRoleDto>
        {
            new() { IdUserRol = 1, RoleName = "Administrador" },
            new() { IdUserRol = 2, RoleName = "Cajero" }
        };

        _userRoleServiceMock.Setup(r => r.GetUserRoles(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        // Act
        var result = await controller.GetUsersRoles(1, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public async Task RoleActionsController_GetRoleActions_ShouldReturnActions()
    {
        // Arrange
        var controller = new RoleActionsController(
            _roleActionsLoggerMock.Object,
            _roleActionServiceMock.Object,
            _notifierMock.Object);

        var actions = new List<string> { "checkin.view", "checkin.create" };

        _roleActionServiceMock.Setup(r => r.GetActionsByRoleIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        // Act
        var result = await controller.GetRoleActions(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(actions);
    }

    [Fact]
    public async Task ModuleController_GetModules_ShouldReturnModules()
    {
        // Arrange
        var controller = new ModuleController(_moduleLoggerMock.Object, _moduleServiceMock.Object);
        var modules = new List<GetModuleDto>
        {
            new() { Id = 1, Name = "Ingreso de Vehículos" },
            new() { Id = 2, Name = "Salida y Cobro" }
        };

        _moduleServiceMock.Setup(m => m.GetModules(It.IsAny<CancellationToken>()))
            .ReturnsAsync(modules);

        // Act
        var result = await controller.GetModules(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(modules);
    }

    [Fact]
    public async Task OperationController_GetOperations_ShouldReturnOperations()
    {
        // Arrange
        var controller = new OperationController(_operationLoggerMock.Object, _operationServiceMock.Object);
        var ops = new List<GetOperationDto>
        {
            new() { Id = 1, Name = "READ" },
            new() { Id = 2, Name = "CREATE" }
        };

        _operationServiceMock.Setup(o => o.GetOperations(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ops);

        // Act
        var result = await controller.GetOperations(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ops);
    }

    [Fact]
    public async Task UserRoleModuleController_GetUserRoleModule_ShouldReturnModulesForRoles()
    {
        // Arrange
        var controller = new UserRoleModuleController(_urmLoggerMock.Object, _userRoleModuleServiceMock.Object);
        var urmList = new List<GetUserRoleModuleDto>
        {
            new()
            {
                Id = 1,
                IsActive = true,
                Role = new GetUserRoleDto { IdUserRol = 2, RoleName = "Cajero" },
                Module = new GetModuleDto { Id = 1, Name = "CheckIn" }
            }
        };

        _userRoleModuleServiceMock.Setup(u => u.GetUserRoleModules(It.IsAny<CancellationToken>()))
            .ReturnsAsync(urmList);

        // Act
        var result = await controller.GetUserRoleModule(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(urmList);
    }
}
