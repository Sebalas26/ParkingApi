using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services.UserRoleModules;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class UserRoleModuleControllerTests
{
    private readonly Mock<IUserRoleModuleService> _serviceMock;
    private readonly Mock<ILogger<UserRoleModuleController>> _loggerMock;
    private readonly UserRoleModuleController _controller;

    public UserRoleModuleControllerTests()
    {
        _serviceMock = new Mock<IUserRoleModuleService>();
        _loggerMock = new Mock<ILogger<UserRoleModuleController>>();
        _controller = new UserRoleModuleController(_loggerMock.Object, _serviceMock.Object);
    }

    [Fact]
    public async Task GetUserRoleModule_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var list = new List<GetUserRoleModuleDto>
        {
            new() { Id = 1, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetUserRoleModules(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetUserRoleModule(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetUserRoleModule_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetUserRoleModules(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetUserRoleModule(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetUserRoleModuleById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var dto = new GetUserRoleModuleDto { Id = 1, IsActive = true };
        _serviceMock.Setup(s => s.GetUserRoleModuleById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetUserRoleModuleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetUserRoleModuleById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetUserRoleModuleById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetUserRoleModuleDto?)null);

        // Act
        var result = await _controller.GetUserRoleModuleById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserRoleModuleById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetUserRoleModuleById(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _controller.GetUserRoleModuleById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditUserRoleModule_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var saveDto = new SaveUserRoleModuleDto { UserRoleId = 1, ModulesRoleId = 2, IsActive = true };
        var resultDto = new GetUserRoleModuleDto { Id = 1, IsActive = true };
        _serviceMock.Setup(s => s.SaveOrEditUserRoleModule(saveDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.SaveOrEditUserRoleModule(saveDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(resultDto);
    }

    [Fact]
    public async Task SaveOrEditUserRoleModule_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var saveDto = new SaveUserRoleModuleDto { UserRoleId = 1 };
        _serviceMock.Setup(s => s.SaveOrEditUserRoleModule(saveDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Write failure"));

        // Act
        var result = await _controller.SaveOrEditUserRoleModule(saveDto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
