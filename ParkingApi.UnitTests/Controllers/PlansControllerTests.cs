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
using ParkingApi.Domain.Dtos.Plans;
using ParkingApi.Domain.Interfaces.Services.Plans;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class PlansControllerTests
{
    private readonly Mock<IPlanService> _planServiceMock;
    private readonly Mock<ILogger<PlansController>> _loggerMock;
    private readonly PlansController _controller;

    public PlansControllerTests()
    {
        _planServiceMock = new Mock<IPlanService>();
        _loggerMock = new Mock<ILogger<PlansController>>();

        _controller = new PlansController(_planServiceMock.Object, _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "superadmin"),
            new Claim(ClaimTypes.Role, "Super Administrador")
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithPlans()
    {
        // Arrange
        var plans = new List<PlanDto>
        {
            new() { Id = 1, Name = "Plan Básico", PriceCop = 50000m, MaxBranches = 1, MaxUsers = 3 }
        };
        _planServiceMock.Setup(s => s.GetAllPlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(plans);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(plans);
    }

    [Fact]
    public async Task GetAll_WhenExceptionOccurs_ShouldReturn500()
    {
        // Arrange
        _planServiceMock.Setup(s => s.GetAllPlansAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnOkWithActivePlans()
    {
        // Arrange
        var activePlans = new List<PlanDto>
        {
            new() { Id = 1, Name = "Plan Pro", PriceCop = 120000m, IsActive = true }
        };
        _planServiceMock.Setup(s => s.GetActivePlansAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activePlans);

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(activePlans);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnOkWithPlan()
    {
        // Arrange
        var plan = new PlanDto { Id = 2, Name = "Plan Garita", PriceCop = 80000m };
        _planServiceMock.Setup(s => s.GetPlanByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _controller.GetById(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(plan);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _planServiceMock.Setup(s => s.GetPlanByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlanDto?)null);

        // Act
        var result = await _controller.GetById(99, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WhenNameIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreatePlanDto { Name = "", PriceCop = 50000m };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenSuccessful_ShouldReturnCreated()
    {
        // Arrange
        var dto = new CreatePlanDto { Name = "Plan Cloud", PriceCop = 60000m, MaxBranches = 2, MaxUsers = 5 };
        var created = new PlanDto { Id = 10, Name = "Plan Cloud", PriceCop = 60000m, MaxBranches = 2, MaxUsers = 5 };

        _planServiceMock.Setup(s => s.CreatePlanAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Create_WhenArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreatePlanDto { Name = "Plan Negativo", PriceCop = -100m };
        _planServiceMock.Setup(s => s.CreatePlanAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("El precio mensual en COP no puede ser negativo."));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOkWithUpdatedPlan()
    {
        // Arrange
        var dto = new UpdatePlanDto { Name = "Plan Cloud Pro", PriceCop = 75000m, MaxBranches = 3, MaxUsers = 8, IsActive = true };
        var updated = new PlanDto { Id = 1, Name = "Plan Cloud Pro", PriceCop = 75000m, MaxBranches = 3, MaxUsers = 8 };

        _planServiceMock.Setup(s => s.UpdatePlanAsync(1, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(1, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var dto = new UpdatePlanDto { Name = "No existe", PriceCop = 50000m };
        _planServiceMock.Setup(s => s.UpdatePlanAsync(99, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Plan con ID 99 no encontrado."));

        // Act
        var result = await _controller.Update(99, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ToggleStatus_WhenFound_ShouldReturnOk()
    {
        // Arrange
        _planServiceMock.Setup(s => s.TogglePlanStatusAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ToggleStatus(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ToggleStatus_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _planServiceMock.Setup(s => s.TogglePlanStatusAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleStatus(99, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _planServiceMock.Setup(s => s.DeletePlanAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenAssignedToCompanies_ShouldReturnBadRequest()
    {
        // Arrange
        _planServiceMock.Setup(s => s.DeletePlanAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No se puede eliminar el plan porque está asignado a empresas."));

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
