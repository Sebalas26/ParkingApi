using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Interfaces.Services.Operations;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class OperationControllerTests
{
    private readonly Mock<IOperationService> _operationServiceMock;
    private readonly Mock<ILogger<OperationController>> _loggerMock;
    private readonly OperationController _controller;

    public OperationControllerTests()
    {
        _operationServiceMock = new Mock<IOperationService>();
        _loggerMock = new Mock<ILogger<OperationController>>();
        _controller = new OperationController(_loggerMock.Object, _operationServiceMock.Object);
    }

    [Fact]
    public async Task GetOperations_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var list = new List<GetOperationDto>
        {
            new() { Id = 1, Name = "CREAR" }
        };
        _operationServiceMock.Setup(s => s.GetOperations(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetOperations(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetOperations_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _operationServiceMock.Setup(s => s.GetOperations(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetOperations(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetOperationById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var operation = new GetOperationDto { Id = 1, Name = "VER" };
        _operationServiceMock.Setup(s => s.GetOperationsById(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var result = await _controller.GetOperationById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(operation);
    }

    [Fact]
    public async Task GetOperationById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _operationServiceMock.Setup(s => s.GetOperationsById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetOperationDto?)null);

        // Act
        var result = await _controller.GetOperationById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetOperationById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _operationServiceMock.Setup(s => s.GetOperationsById(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetOperationById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditOperation_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var operation = new GetOperationDto { Id = 1, Name = "EDITAR" };
        _operationServiceMock.Setup(s => s.SaveOrEditOperation(operation, It.IsAny<CancellationToken>()))
            .ReturnsAsync(operation);

        // Act
        var result = await _controller.SaveOrEditOperation(operation, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(operation);
    }

    [Fact]
    public async Task SaveOrEditOperation_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var operation = new GetOperationDto { Id = 1, Name = "Error" };
        _operationServiceMock.Setup(s => s.SaveOrEditOperation(operation, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Write failure"));

        // Act
        var result = await _controller.SaveOrEditOperation(operation, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
