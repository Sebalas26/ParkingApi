using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Interfaces.Services.Modules;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ModuleControllerTests
{
    private readonly Mock<IModuleService> _moduleServiceMock;
    private readonly Mock<ILogger<ModuleController>> _loggerMock;
    private readonly ModuleController _controller;

    public ModuleControllerTests()
    {
        _moduleServiceMock = new Mock<IModuleService>();
        _loggerMock = new Mock<ILogger<ModuleController>>();
        _controller = new ModuleController(_loggerMock.Object, _moduleServiceMock.Object);
    }

    [Fact]
    public async Task GetModules_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var modules = new List<GetModuleDto>
        {
            new() { Id = 1, Name = "Entradas", IsActive = true }
        };
        _moduleServiceMock.Setup(s => s.GetModules(It.IsAny<CancellationToken>()))
            .ReturnsAsync(modules);

        // Act
        var result = await _controller.GetModules(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(modules);
    }

    [Fact]
    public async Task GetModules_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _moduleServiceMock.Setup(s => s.GetModules(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database down"));

        // Act
        var result = await _controller.GetModules(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetModuleById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var module = new GetModuleDto { Id = 1, Name = "Salidas" };
        _moduleServiceMock.Setup(s => s.GetModuleById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);

        // Act
        var result = await _controller.GetModuleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(module);
    }

    [Fact]
    public async Task GetModuleById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _moduleServiceMock.Setup(s => s.GetModuleById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetModuleDto?)null);

        // Act
        var result = await _controller.GetModuleById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetModuleById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _moduleServiceMock.Setup(s => s.GetModuleById(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal error"));

        // Act
        var result = await _controller.GetModuleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveModule_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var module = new GetModuleDto { Id = 1, Name = "Configuración" };
        _moduleServiceMock.Setup(s => s.SaveOrEditModule(module, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);

        // Act
        var result = await _controller.SaveModule(module, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(module);
    }

    [Fact]
    public async Task SaveModule_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var module = new GetModuleDto { Id = 1, Name = "Error" };
        _moduleServiceMock.Setup(s => s.SaveOrEditModule(module, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var result = await _controller.SaveModule(module, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
