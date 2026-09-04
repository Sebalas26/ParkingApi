using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class PaymentMethodControllerTests
{
    private readonly Mock<IPaymentMethodService> _serviceMock;
    private readonly Mock<IRealtimeNotificationService> _notifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<PaymentMethodController>> _loggerMock;
    private readonly PaymentMethodController _controller;

    public PaymentMethodControllerTests()
    {
        _serviceMock = new Mock<IPaymentMethodService>();
        _notifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<PaymentMethodController>>();

        _controller = new PaymentMethodController(
            _loggerMock.Object,
            _serviceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task GetPaymentMethods_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var methods = new List<GetPaymentMethodDto>
        {
            new() { Id = 1, Name = "Efectivo", IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(methods);

        // Act
        var result = await _controller.GetPaymentMethods(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(methods);
    }

    [Fact]
    public async Task GetPaymentMethods_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetPaymentMethods(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetPaymentMethodsActive_WhenSuccessful_ShouldReturnOkWithActiveList()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var methods = new List<GetPaymentMethodDto>
        {
            new() { Id = 2, Name = "Transferencia", IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllActiveAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(methods);

        // Act
        var result = await _controller.GetPaymentMethodsActive(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(methods);
    }

    [Fact]
    public async Task GetPaymentMethodsActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _serviceMock.Setup(s => s.GetAllActiveAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service down"));

        // Act
        var result = await _controller.GetPaymentMethodsActive(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetPaymentMethodById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var method = new GetPaymentMethodDto { Id = 1, Name = "Datafono" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(method);

        // Act
        var result = await _controller.GetPaymentMethodById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(method);
    }

    [Fact]
    public async Task GetPaymentMethodById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPaymentMethodDto?)null);

        // Act
        var result = await _controller.GetPaymentMethodById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPaymentMethodById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetPaymentMethodById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateOrEditPaymentMethod_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var dto = new GetPaymentMethodDto { Id = 1, Name = "QR Bancolombia" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _serviceMock.Setup(s => s.CreateOrEditPaymentMethod(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.CreateOrEditPaymentMethod(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
        _notifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            "PaymentMethodsChanged",
            "Medio de Pago Modificado",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrEditPaymentMethod_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new GetPaymentMethodDto { Id = 1, Name = "Error" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _serviceMock.Setup(s => s.CreateOrEditPaymentMethod(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Insert crash"));

        // Act
        var result = await _controller.CreateOrEditPaymentMethod(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
