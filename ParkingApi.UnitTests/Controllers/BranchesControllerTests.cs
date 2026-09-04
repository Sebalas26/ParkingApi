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
using ParkingApi.Domain.Dtos.Branches;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Branches;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class BranchesControllerTests
{
    private readonly Mock<IBranchService> _branchServiceMock;
    private readonly Mock<IRealtimeNotificationService> _realtimeNotifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<BranchesController>> _loggerMock;
    private readonly BranchesController _controller;

    public BranchesControllerTests()
    {
        _branchServiceMock = new Mock<IBranchService>();
        _realtimeNotifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<BranchesController>>();

        _controller = new BranchesController(
            _branchServiceMock.Object,
            _realtimeNotifierMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim("company_id", "5")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_WhenCompanyIdProvided_ShouldReturnCompanyBranches()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(5)).Returns(5);
        var branches = new List<BranchDto> { new() { Id = 1, Name = "Sede Norte", CompanyId = 5 } };
        _branchServiceMock.Setup(b => b.GetBranchesByCompanyIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _controller.GetAll(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task GetAll_WhenNoCompanyAndNotSuperAdmin_ShouldReturnEmptyList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(null)).Returns((int?)null);
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);

        // Act
        var result = await _controller.GetAll(null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new List<BranchDto>());
    }

    [Fact]
    public async Task GetAll_WhenNoCompanyAndIsSuperAdmin_ShouldReturnAllBranches()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(null)).Returns((int?)null);
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        var branches = new List<BranchDto> { new() { Id = 1, Name = "Sede 1" }, new() { Id = 2, Name = "Sede 2" } };
        _branchServiceMock.Setup(b => b.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _controller.GetAll(null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task GetActive_ShouldReturnActiveBranches()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(5);
        var activeBranches = new List<BranchDto> { new() { Id = 1, Name = "Sede Activa" } };
        _branchServiceMock.Setup(b => b.GetActiveAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeBranches);

        // Act
        var result = await _controller.GetActive(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(activeBranches);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnBranch()
    {
        // Arrange
        var branch = new BranchDto { Id = 1, Name = "Sede Centro" };
        _branchServiceMock.Setup(b => b.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branch);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branch);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _branchServiceMock.Setup(b => b.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BranchDto?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByUser_ShouldReturnUserBranches()
    {
        // Arrange
        var branches = new List<BranchDto> { new() { Id = 1, Name = "Sede Asignada" } };
        _branchServiceMock.Setup(b => b.GetBranchesByUserIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _controller.GetByUser(10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task GetByCompany_ShouldReturnCompanyBranches()
    {
        // Arrange
        var branches = new List<BranchDto> { new() { Id = 3, Name = "Sede Empresa" } };
        _branchServiceMock.Setup(b => b.GetBranchesByCompanyIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await _controller.GetByCompany(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task Create_WhenNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateBranchDto { Name = "", CompanyId = 1 };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenCompanyIdMissing_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // no company claim
        var dto = new CreateBranchDto { Name = "Sede Sin Empresa", CompanyId = null };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtActionAndNotify()
    {
        // Arrange
        var dto = new CreateBranchDto { Name = "Sede Éxito", CompanyId = 5 };
        var created = new BranchDto { Id = 15, Name = dto.Name, CompanyId = 5 };
        _branchServiceMock.Setup(b => b.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
        _realtimeNotifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "BranchCreated",
            "Nueva Sede Creada",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateBranchDto { Name = "Sede Duplicada", CompanyId = 5 };
        _branchServiceMock.Setup(b => b.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ya existe una sede con ese nombre"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new CreateBranchDto { Name = "Sede Error", CompanyId = 5 };
        _branchServiceMock.Setup(b => b.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenFound_ShouldReturnOkAndNotify()
    {
        // Arrange
        var dto = new UpdateBranchDto { Name = "Sede Modificada" };
        var updated = new BranchDto { Id = 1, Name = "Sede Modificada" };
        _branchServiceMock.Setup(b => b.UpdateAsync(1, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(updated);
        _realtimeNotifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            "Sede Actualizada",
            It.IsAny<string>(),
            "BranchConfigChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var dto = new UpdateBranchDto { Name = "Sede Inexistente" };
        _branchServiceMock.Setup(b => b.UpdateAsync(999, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BranchDto?)null);

        // Act
        var result = await _controller.Update(999, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _branchServiceMock.Setup(b => b.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateBranchDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal error"));

        // Act
        var result = await _controller.Update(1, new UpdateBranchDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task AssignUser_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new AssignUserBranchDto { UserId = 2, BranchId = 1 };
        _branchServiceMock.Setup(b => b.AssignUserAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignUser(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task AssignUser_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new AssignUserBranchDto { UserId = 2, BranchId = 1 };
        _branchServiceMock.Setup(b => b.AssignUserAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignUser(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UnassignUser_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new AssignUserBranchDto { UserId = 2, BranchId = 1 };
        _branchServiceMock.Setup(b => b.UnassignUserAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UnassignUser(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UnassignUser_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new AssignUserBranchDto { UserId = 2, BranchId = 1 };
        _branchServiceMock.Setup(b => b.UnassignUserAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UnassignUser(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPaymentMethods_ShouldReturnMethods()
    {
        // Arrange
        var methods = new List<BranchPaymentMethodDto> { new() { Id = 1, PaymentMethodName = "Efectivo", IsActive = true } };
        _branchServiceMock.Setup(b => b.GetPaymentMethodsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(methods);

        // Act
        var result = await _controller.GetPaymentMethods(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(methods);
    }

    [Fact]
    public async Task ConfigurePaymentMethods_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var dto = new ConfigureBranchPaymentMethodsDto { BranchId = 1, PaymentMethodIds = new List<int> { 1, 2 } };
        _branchServiceMock.Setup(b => b.ConfigurePaymentMethodsAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ConfigurePaymentMethods(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _realtimeNotifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            "Medios de Pago Actualizados",
            It.IsAny<string>(),
            "PaymentMethodsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfigurePaymentMethods_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new ConfigureBranchPaymentMethodsDto { BranchId = 1 };
        _branchServiceMock.Setup(b => b.ConfigurePaymentMethodsAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ConfigurePaymentMethods(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetBranchUsers_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<GetUsersDto> { new() { Id = 1, Username = "operario1" } };
        _branchServiceMock.Setup(b => b.GetUsersByBranchIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetBranchUsers(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(users);
    }
}
