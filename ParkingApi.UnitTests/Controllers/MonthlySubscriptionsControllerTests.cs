using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class MonthlySubscriptionsControllerTests
{
    private readonly Mock<IMonthlySubscriptionService> _subscriptionServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<MonthlySubscriptionsController>> _loggerMock;
    private readonly MonthlySubscriptionsController _controller;

    public MonthlySubscriptionsControllerTests()
    {
        _subscriptionServiceMock = new Mock<IMonthlySubscriptionService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<MonthlySubscriptionsController>>();

        _controller = new MonthlySubscriptionsController(
            _loggerMock.Object,
            _subscriptionServiceMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var subscriptions = new List<MonthlySubscriptionDto>
        {
            new() { SubscriptionId = Guid.NewGuid(), CustomerName = "Pedro", PlateNumber = "XYZ123" }
        };
        _subscriptionServiceMock.Setup(s => s.GetAllAsync(1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(subscriptions);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _subscriptionServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database down"));

        // Act
        var result = await _controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnOkWithActiveList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var subscriptions = new List<MonthlySubscriptionDto>
        {
            new() { SubscriptionId = Guid.NewGuid(), CustomerName = "Maria", PlateNumber = "ABC987" }
        };
        _subscriptionServiceMock.Setup(s => s.GetActiveAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subscriptions);

        // Act
        var result = await _controller.GetActive(2, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(subscriptions);
    }

    [Fact]
    public async Task GetActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _subscriptionServiceMock.Setup(s => s.GetActiveAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Query error"));

        // Act
        var result = await _controller.GetActive(null, null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetByPlate_WhenFound_ShouldReturnOkWithSubscription()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var sub = new MonthlySubscriptionDto { SubscriptionId = Guid.NewGuid(), PlateNumber = "ABC123" };
        _subscriptionServiceMock.Setup(s => s.GetActiveByPlateAsync("ABC123", 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        var result = await _controller.GetByPlate("ABC123", null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(sub);
    }

    [Fact]
    public async Task GetByPlate_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _subscriptionServiceMock.Setup(s => s.GetActiveByPlateAsync("NOTFOUND", 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySubscriptionDto?)null);

        // Act
        var result = await _controller.GetByPlate("NOTFOUND", null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByPlate_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _subscriptionServiceMock.Setup(s => s.GetActiveByPlateAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Lookup failure"));

        // Act
        var result = await _controller.GetByPlate("ERR123", null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFoundAndAuthorized_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sub = new MonthlySubscriptionDto { SubscriptionId = id, CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _subscriptionServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(sub);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _subscriptionServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySubscriptionDto?)null);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenCompanyUnauthorized_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var sub = new MonthlySubscriptionDto { SubscriptionId = id, CompanyId = 99 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _subscriptionServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);

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
        _subscriptionServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = new CreateMonthlySubscriptionDto { CustomerName = "Laura", PlateNumber = "JKL456", CompanyId = 1 };
        var created = new MonthlySubscriptionDto { SubscriptionId = Guid.NewGuid(), CustomerName = "Laura", PlateNumber = "JKL456" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _subscriptionServiceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task Create_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new CreateMonthlySubscriptionDto { CustomerName = "Laura", PlateNumber = "JKL456" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _subscriptionServiceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Ya existe mensualidad vigente para la placa"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new CreateMonthlySubscriptionDto { CustomerName = "Laura" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _subscriptionServiceMock.Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Renew_WhenSuccessful_ShouldReturnOkWithRenewed()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new RenewSubscriptionDto { AdditionalMonths = 1, AmountPaid = 150000 };
        var renewed = new MonthlySubscriptionDto { SubscriptionId = id, MonthlyFee = 150000 };
        _subscriptionServiceMock.Setup(s => s.RenewAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(renewed);

        // Act
        var result = await _controller.Renew(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(renewed);
    }

    [Fact]
    public async Task Renew_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new RenewSubscriptionDto { AdditionalMonths = 1 };
        _subscriptionServiceMock.Setup(s => s.RenewAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonthlySubscriptionDto?)null);

        // Act
        var result = await _controller.Renew(id, dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Renew_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _subscriptionServiceMock.Setup(s => s.RenewAsync(id, It.IsAny<RenewSubscriptionDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Renewal failure"));

        // Act
        var result = await _controller.Renew(id, new RenewSubscriptionDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Cancel_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _subscriptionServiceMock.Setup(s => s.CancelAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Cancel(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Cancel_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _subscriptionServiceMock.Setup(s => s.CancelAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Cancel(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Cancel_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _subscriptionServiceMock.Setup(s => s.CancelAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cancel error"));

        // Act
        var result = await _controller.Cancel(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
