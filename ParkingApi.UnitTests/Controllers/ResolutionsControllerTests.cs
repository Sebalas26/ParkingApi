using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Billing;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Billing;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ResolutionsControllerTests
{
    private readonly Mock<IBillingResolutionService> _resolutionServiceMock;
    private readonly Mock<IRealtimeNotificationService> _notifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<ResolutionsController>> _loggerMock;
    private readonly ResolutionsController _controller;

    public ResolutionsControllerTests()
    {
        _resolutionServiceMock = new Mock<IBillingResolutionService>();
        _notifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<ResolutionsController>>();

        _controller = new ResolutionsController(
            _resolutionServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnResolutions()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var resolutions = new List<BillingResolutionDto>
        {
            new() { ResolutionId = Guid.NewGuid(), ResolutionNumber = "18760000001", Prefix = "POS", FromNumber = 1, ToNumber = 10000 }
        };
        _resolutionServiceMock.Setup(r => r.GetAllAsync(null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolutions);

        // Act
        var result = await _controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resolutions);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _resolutionServiceMock.Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnActiveResolutions()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var resolutions = new List<BillingResolutionDto>
        {
            new() { ResolutionId = Guid.NewGuid(), ResolutionNumber = "18760000002", Prefix = "POS" }
        };
        _resolutionServiceMock.Setup(r => r.GetActiveAsync(null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolutions);

        // Act
        var result = await _controller.GetActive(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resolutions);
    }

    [Fact]
    public async Task GetActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _resolutionServiceMock.Setup(r => r.GetActiveAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Query error"));

        // Act
        var result = await _controller.GetActive(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetByBranch_WhenSuccessful_ShouldReturnResolutionsForBranch()
    {
        // Arrange
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        var resolutions = new List<BillingResolutionDto>
        {
            new() { ResolutionId = Guid.NewGuid(), BranchId = 2, Prefix = "POS" }
        };
        _resolutionServiceMock.Setup(r => r.GetActiveAsync(2, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolutions);

        // Act
        var result = await _controller.GetByBranch(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resolutions);
    }

    [Fact]
    public async Task GetByBranch_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _resolutionServiceMock.Setup(r => r.GetActiveAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Branch query failure"));

        // Act
        var result = await _controller.GetByBranch(2, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnResolution()
    {
        // Arrange
        var id = Guid.NewGuid();
        var resolution = new BillingResolutionDto { ResolutionId = id, Prefix = "POS" };
        _resolutionServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolution);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resolution);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _resolutionServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BillingResolutionDto?)null);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _resolutionServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenFieldsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SaveBillingResolutionDto { Name = "", Prefix = "", ResolutionNumber = "" };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenFromNumberGreaterThanToNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SaveBillingResolutionDto
        {
            Name = "Res 1",
            Prefix = "A",
            ResolutionNumber = "123",
            FromNumber = 500,
            ToNumber = 100
        };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtActionAndNotify()
    {
        // Arrange
        var dto = new SaveBillingResolutionDto
        {
            Name = "Resolución 2026",
            Prefix = "FE",
            ResolutionNumber = "18760000001",
            FromNumber = 1,
            ToNumber = 10000,
            BranchId = 1
        };
        var created = new BillingResolutionDto { ResolutionId = Guid.NewGuid(), Name = dto.Name, Prefix = dto.Prefix };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _resolutionServiceMock.Setup(r => r.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            "Resolución de Facturación Creada",
            It.IsAny<string>(),
            "ResolutionsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new SaveBillingResolutionDto
        {
            Name = "Resolución Error",
            Prefix = "FE",
            ResolutionNumber = "18760000001",
            FromNumber = 1,
            ToNumber = 10000
        };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _resolutionServiceMock.Setup(r => r.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Insert failure"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenValid_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveBillingResolutionDto
        {
            Name = "Resolución Modificada",
            Prefix = "FE",
            ResolutionNumber = "18760000001",
            FromNumber = 1,
            ToNumber = 20000,
            BranchId = 2
        };
        var updated = new BillingResolutionDto { ResolutionId = id, Name = dto.Name };
        _resolutionServiceMock.Setup(r => r.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(updated);
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            2,
            "Resolución de Facturación Actualizada",
            It.IsAny<string>(),
            "ResolutionsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenFieldsMissing_ShouldReturnBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveBillingResolutionDto { Name = "", Prefix = "", ResolutionNumber = "" };

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenFromNumberGreaterThanToNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveBillingResolutionDto
        {
            Name = "Res 1",
            Prefix = "A",
            ResolutionNumber = "123",
            FromNumber = 500,
            ToNumber = 100
        };

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveBillingResolutionDto
        {
            Name = "Resolución",
            Prefix = "FE",
            ResolutionNumber = "18760000001",
            FromNumber = 1,
            ToNumber = 1000
        };
        _resolutionServiceMock.Setup(r => r.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BillingResolutionDto?)null);

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new SaveBillingResolutionDto
        {
            Name = "Resolución",
            Prefix = "FE",
            ResolutionNumber = "18760000001",
            FromNumber = 1,
            ToNumber = 1000
        };
        _resolutionServiceMock.Setup(r => r.UpdateAsync(id, dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        var result = await _controller.Update(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Deactivate_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        var resolution = new BillingResolutionDto { ResolutionId = id, BranchId = 1 };
        _resolutionServiceMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolution);
        _resolutionServiceMock.Setup(r => r.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _notifierMock.Verify(n => n.NotifyBranchConfigChangedAsync(
            1,
            "Resolución Inactivada",
            It.IsAny<string>(),
            "ResolutionsChanged",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Deactivate_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _resolutionServiceMock.Setup(r => r.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Deactivate(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Deactivate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _resolutionServiceMock.Setup(r => r.DeactivateAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Deactivate error"));

        // Act
        var result = await _controller.Deactivate(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
