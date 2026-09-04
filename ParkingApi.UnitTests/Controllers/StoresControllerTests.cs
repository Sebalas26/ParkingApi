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
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class StoresControllerTests
{
    private readonly Mock<IStoreService> _storeServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<StoresController>> _loggerMock;
    private readonly StoresController _controller;

    public StoresControllerTests()
    {
        _storeServiceMock = new Mock<IStoreService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<StoresController>>();

        _controller = new StoresController(
            _storeServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithStores()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var stores = new List<Store> { new() { StoreId = Guid.NewGuid(), Name = "Cafetería" } };
        _storeServiceMock.Setup(s => s.GetAllAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stores);

        // Act
        var result = await _controller.GetAll(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(stores);
    }

    [Fact]
    public async Task GetAll_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _storeServiceMock.Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

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
        var store = new Store { StoreId = id, Name = "Lavadero", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(1)).Returns(true);
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(store);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_WhenCompanyMismatch_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var store = new Store { StoreId = id, Name = "Comercio Ajeno", CompanyId = 99 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CanAccessCompany(99)).Returns(false);
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

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
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_WhenSuccessful_ShouldReturnOkWithCreated()
    {
        // Arrange
        var store = new Store { Name = "Supermercado", CompanyId = 1 };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        _storeServiceMock.Setup(s => s.CreateAsync(store, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        // Act
        var result = await _controller.Create(store, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(store);
    }

    [Fact]
    public async Task Create_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var store = new Store { Name = "Supermercado" };
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(false);
        _currentUserMock.Setup(c => c.CompanyId).Returns(1);
        _storeServiceMock.Setup(s => s.CreateAsync(store, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error saving store"));

        // Act
        var result = await _controller.Create(store, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Update_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var store = new Store { StoreId = id, Name = "Editado" };
        _storeServiceMock.Setup(s => s.UpdateAsync(store, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Update(id, store, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(store);
    }

    [Fact]
    public async Task Update_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var store = new Store { StoreId = id, Name = "Editado" };
        _storeServiceMock.Setup(s => s.UpdateAsync(store, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Update(id, store, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var id = Guid.NewGuid();
        var store = new Store { StoreId = id, Name = "Editado" };
        _storeServiceMock.Setup(s => s.UpdateAsync(store, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error updating store"));

        // Act
        var result = await _controller.Update(id, store, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Delete_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var store = new Store { StoreId = id, Name = "Para Inactivar", IsActive = true };
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);
        _storeServiceMock.Setup(s => s.UpdateAsync(It.Is<Store>(st => !st.IsActive), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

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
        var store = new Store { StoreId = id, Name = "Para Inactivar", IsActive = true };
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);
        _storeServiceMock.Setup(s => s.UpdateAsync(store, It.IsAny<CancellationToken>()))
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
        _storeServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
