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
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Branches;
using ParkingApi.Domain.Interfaces.Services.Companies;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class CompaniesAndBranchesControllerTests
{
    private readonly Mock<ICompanyService> _companyServiceMock;
    private readonly Mock<IBranchService> _branchServiceMock;
    private readonly Mock<IRealtimeNotificationService> _realtimeNotifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<CompaniesController>> _companiesLoggerMock;
    private readonly Mock<ILogger<BranchesController>> _branchesLoggerMock;

    public CompaniesAndBranchesControllerTests()
    {
        _companyServiceMock = new Mock<ICompanyService>();
        _branchServiceMock = new Mock<IBranchService>();
        _realtimeNotifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _companiesLoggerMock = new Mock<ILogger<CompaniesController>>();
        _branchesLoggerMock = new Mock<ILogger<BranchesController>>();
    }

    private static ControllerContext CreateControllerContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Administrador")
        }, "TestAuth"));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CompaniesController_GetAll_ShouldReturnOkWithList()
    {
        // Arrange
        var controller = new CompaniesController(_companyServiceMock.Object, _companiesLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };
        var list = new List<CompanyDto>
        {
            new() { Id = 1, Name = "Empresa Uno", AllowMultipleSessions = true, RequireOpenShiftToOperate = true },
            new() { Id = 2, Name = "Empresa Dos", AllowMultipleSessions = false, RequireOpenShiftToOperate = false }
        };
        _companyServiceMock.Setup(s => s.GetAllCompaniesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(list);

        // Act
        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task CompaniesController_GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var controller = new CompaniesController(_companyServiceMock.Object, _companiesLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };
        _companyServiceMock.Setup(s => s.GetCompanyByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((CompanyDto?)null);

        // Act
        var result = await controller.GetById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CompaniesController_Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var controller = new CompaniesController(_companyServiceMock.Object, _companiesLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };
        var createDto = new CreateCompanyDto
        {
            Name = "Nueva Empresa Test",
            Nit = "900123456",
            Email = "info@empresa.com",
            AllowMultipleSessions = true,
            MaxActiveSessionsPerUser = 3,
            RequireOpenShiftToOperate = true,
            AllowMultipleOpenShifts = true,
            MaxOpenShiftsPerUser = 2,
            RequireInitialCashAmount = true,
            AdminUsername = "admin_test",
            AdminPassword = "password123",
            AdminFullName = "Admin Test"
        };
        var created = new CompanyDto { Id = 10, Name = createDto.Name, Nit = createDto.Nit };

        _companyServiceMock.Setup(s => s.CreateCompanyAsync(createDto, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await controller.Create(createDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task BranchesController_GetAll_ShouldReturnBranchesForCompany()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(5);
        var controller = new BranchesController(
            _branchServiceMock.Object,
            _realtimeNotifierMock.Object,
            _currentUserMock.Object,
            _branchesLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        var branches = new List<BranchDto>
        {
            new() { Id = 1, CompanyId = 5, Name = "Sede Norte", AllowChargeByMinute = true, AllowChargeByHour = true, AllowChargeByDay = true, AllowChargeByNight = false },
            new() { Id = 2, CompanyId = 5, Name = "Sede Sur", AllowChargeByMinute = false, AllowChargeByHour = true, AllowChargeByDay = true, AllowChargeByNight = true }
        };

        _branchServiceMock.Setup(b => b.GetBranchesByCompanyIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branches);

        // Act
        var result = await controller.GetAll(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(branches);
    }

    [Fact]
    public async Task BranchesController_Create_ShouldReturnCreatedAtAction()
    {
        // Arrange
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        var controller = new BranchesController(
            _branchServiceMock.Object,
            _realtimeNotifierMock.Object,
            _currentUserMock.Object,
            _branchesLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        var createDto = new CreateBranchDto
        {
            CompanyId = 5,
            Name = "Sede Nueva",
            Address = "Calle 100 #10-20",
            AllowChargeByMinute = true,
            AllowChargeByHour = true,
            AllowChargeByDay = true,
            AllowChargeByNight = false,
            DefaultInitialCash = 50000
        };

        var created = new BranchDto { Id = 20, CompanyId = 5, Name = createDto.Name };

        _branchServiceMock.Setup(b => b.CreateAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await controller.Create(createDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }
}
