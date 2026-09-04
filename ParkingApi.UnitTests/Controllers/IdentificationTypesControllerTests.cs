using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services.IdentificationTypes;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class IdentificationTypesControllerTests
{
    private readonly Mock<IIdentificationTypeService> _serviceMock;
    private readonly Mock<ILogger<IdentificationTypesController>> _loggerMock;
    private readonly IdentificationTypesController _controller;

    public IdentificationTypesControllerTests()
    {
        _serviceMock = new Mock<IIdentificationTypeService>();
        _loggerMock = new Mock<ILogger<IdentificationTypesController>>();
        _controller = new IdentificationTypesController(_loggerMock.Object, _serviceMock.Object);
    }

    [Fact]
    public async Task GetIdentificationTypes_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var list = new List<GetIdentificationTypeDto>
        {
            new() { Id = 1, Name = "Cédula de Ciudadanía", IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetIdentificationTypes(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetIdentificationTypes_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetIdentificationTypes(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetIdentificationTypesActive_WhenSuccessful_ShouldReturnOkWithActiveList()
    {
        // Arrange
        var list = new List<GetIdentificationTypeDto>
        {
            new() { Id = 2, Name = "NIT", IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetIdentificationTypesActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetIdentificationTypesActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service down"));

        // Act
        var result = await _controller.GetIdentificationTypesActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetIdentificationTypeById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var dto = new GetIdentificationTypeDto { Id = 1, Name = "Pasaporte" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetIdentificationTypeById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetIdentificationTypeById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetIdentificationTypeDto?)null);

        // Act
        var result = await _controller.GetIdentificationTypeById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetIdentificationTypeById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database down"));

        // Act
        var result = await _controller.GetIdentificationTypeById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditIdentificationTypes_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new GetIdentificationTypeDto { Id = 1, Name = "Cédula Extranjería" };
        _serviceMock.Setup(s => s.CreateOrEditIdentificationType(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.SaveOrEditIdentificationTypes(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task SaveOrEditIdentificationTypes_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new GetIdentificationTypeDto { Id = 1, Name = "Error" };
        _serviceMock.Setup(s => s.CreateOrEditIdentificationType(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Write failure"));

        // Act
        var result = await _controller.SaveOrEditIdentificationTypes(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
