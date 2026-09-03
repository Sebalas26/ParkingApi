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
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Billing;
using ParkingApi.Domain.Interfaces.Services.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class BillingAndPaymentControllerTests
{
    private readonly Mock<IBillingResolutionService> _resolutionServiceMock = new();
    private readonly Mock<IPaymentMethodService> _paymentMethodServiceMock = new();
    private readonly Mock<IRealtimeNotificationService> _notifierMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<ILogger<ResolutionsController>> _resolutionsLoggerMock = new();
    private readonly Mock<ILogger<PaymentMethodController>> _paymentLoggerMock = new();

    [Fact]
    public async Task ResolutionsController_GetAll_ShouldReturnResolutions()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new ResolutionsController(
            _resolutionServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object,
            _resolutionsLoggerMock.Object);

        var resolutions = new List<BillingResolutionDto>
        {
            new() { ResolutionId = Guid.NewGuid(), ResolutionNumber = "18760000001", Prefix = "POS", FromNumber = 1, ToNumber = 10000 }
        };

        _resolutionServiceMock.Setup(r => r.GetAllAsync(null, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resolutions);

        // Act
        var result = await controller.GetAll(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resolutions);
    }

    [Fact]
    public async Task PaymentMethodController_GetPaymentMethods_ShouldReturnMethods()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var controller = new PaymentMethodController(
            _paymentLoggerMock.Object,
            _paymentMethodServiceMock.Object,
            _notifierMock.Object,
            _currentUserMock.Object);

        var methods = new List<GetPaymentMethodDto>
        {
            new() { Id = 1, Name = "Efectivo", IsActive = true },
            new() { Id = 2, Name = "Tarjeta Débito", IsActive = true }
        };

        _paymentMethodServiceMock.Setup(p => p.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(methods);

        // Act
        var result = await controller.GetPaymentMethods(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(methods);
    }
}
