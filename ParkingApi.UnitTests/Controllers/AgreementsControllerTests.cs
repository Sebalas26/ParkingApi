using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class AgreementsControllerTests
{
    private readonly Mock<ICommercialAgreementService> _agreementServiceMock;
    private readonly Mock<IRealtimeNotificationService> _realtimeNotifierMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<AgreementsController>> _loggerMock;
    private readonly AgreementsController _controller;

    public AgreementsControllerTests()
    {
        _agreementServiceMock = new Mock<ICommercialAgreementService>();
        _realtimeNotifierMock = new Mock<IRealtimeNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<AgreementsController>>();

        _controller = new AgreementsController(
            _agreementServiceMock.Object,
            _realtimeNotifierMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithAgreements()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var agreements = new List<CommercialAgreement>
        {
            new() { AgreementId = Guid.NewGuid(), Name = "Convenio Cine", DiscountPercentage = 20 }
        };
        _agreementServiceMock.Setup(s => s.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreements);

        // Act
        var result = await _controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(agreements);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _agreementServiceMock.Setup(s => s.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error retrieving agreements"));

        // Act
        var result = await _controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFoundAndAuthorized_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement
        {
            AgreementId = id,
            Name = "Convenio Gimnasio",
            Store = new Store { CompanyId = 1 }
        };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreement);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(agreement);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommercialAgreement?)null);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenCompanyAccessForbidden_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement
        {
            AgreementId = id,
            Name = "Convenio Privado",
            Store = new Store { CompanyId = 99 }
        };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreement);

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
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failure"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var agreement = new CommercialAgreement { Name = "Convenio Restaurante", DiscountPercentage = 15 };
        _agreementServiceMock.Setup(s => s.CreateAsync(agreement, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreement);

        // Act
        var result = await _controller.Create(agreement, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(agreement);
        _realtimeNotifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var agreement = new CommercialAgreement { Name = "Convenio Error" };
        _agreementServiceMock.Setup(s => s.CreateAsync(agreement, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Create(agreement, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOkAndNotify()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement { AgreementId = id, Name = "Convenio Actualizado" };
        _agreementServiceMock.Setup(s => s.UpdateAsync(agreement, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(id, agreement, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(agreement);
        _realtimeNotifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement { AgreementId = id, Name = "Convenio Inexistente" };
        _agreementServiceMock.Setup(s => s.UpdateAsync(agreement, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(id, agreement, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement { AgreementId = id, Name = "Convenio Error" };
        _agreementServiceMock.Setup(s => s.UpdateAsync(agreement, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        var result = await _controller.Update(id, agreement, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOkAndInactivate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement { AgreementId = id, Name = "Convenio a Desactivar", IsActive = true };
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreement);
        _agreementServiceMock.Setup(s => s.UpdateAsync(It.Is<CommercialAgreement>(a => !a.IsActive), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _realtimeNotifierMock.Verify(n => n.NotifyGlobalConfigChangedAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommercialAgreement?)null);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenUpdateFails_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var agreement = new CommercialAgreement { AgreementId = id, Name = "Convenio Fallido", IsActive = true };
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agreement);
        _agreementServiceMock.Setup(s => s.UpdateAsync(agreement, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        _agreementServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Inactivation error"));

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
