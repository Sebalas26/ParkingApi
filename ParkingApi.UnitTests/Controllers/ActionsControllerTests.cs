using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Dtos.Actions;
using ParkingApi.Domain.Interfaces.Services.Actions;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ActionsControllerTests
{
    private readonly Mock<IActionService> _actionServiceMock;
    private readonly Mock<ILogger<ActionsController>> _loggerMock;
    private readonly ActionsController _controller;

    public ActionsControllerTests()
    {
        _actionServiceMock = new Mock<IActionService>();
        _loggerMock = new Mock<ILogger<ActionsController>>();
        _controller = new ActionsController(_loggerMock.Object, _actionServiceMock.Object);
    }

    [Fact]
    public async Task GetActions_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var actions = new List<GetActionsDto>
        {
            new() { Id = 1, Name = "Crear", Slug = "actions.create", IsActive = true }
        };
        _actionServiceMock.Setup(s => s.GetActions(It.IsAny<CancellationToken>()))
            .ReturnsAsync(actions);

        // Act
        var result = await _controller.GetActions(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(actions);
    }

    [Fact]
    public async Task GetActions_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _actionServiceMock.Setup(s => s.GetActions(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetActions(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActionsActive_WhenSuccessful_ShouldReturnOkWithActiveList()
    {
        // Arrange
        var activeActions = new List<GetActionsDto>
        {
            new() { Id = 1, Name = "Ver", Slug = "actions.view", IsActive = true }
        };
        _actionServiceMock.Setup(s => s.GetActionsActive(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeActions);

        // Act
        var result = await _controller.GetActionsActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(activeActions);
    }

    [Fact]
    public async Task GetActionsActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _actionServiceMock.Setup(s => s.GetActionsActive(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service exception"));

        // Act
        var result = await _controller.GetActionsActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActionById_WhenActionExists_ShouldReturnOkWithAction()
    {
        // Arrange
        var action = new GetActionsDto { Id = 5, Name = "Editar", Slug = "actions.edit", IsActive = true };
        _actionServiceMock.Setup(s => s.GetActionsById(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(action);

        // Act
        var result = await _controller.GetActionById(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(action);
    }

    [Fact]
    public async Task GetActionById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _actionServiceMock.Setup(s => s.GetActionsById(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetActionsDto?)null);

        // Act
        var result = await _controller.GetActionById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetActionById_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _actionServiceMock.Setup(s => s.GetActionsById(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected failure"));

        // Act
        var result = await _controller.GetActionById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SaveOrEditAction_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var dto = new GetActionsDto { Id = 1, Name = "Eliminar", Slug = "actions.delete", IsActive = true };
        _actionServiceMock.Setup(s => s.SaveOrEditActions(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.SaveOrEditAction(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task SaveOrEditAction_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        var dto = new GetActionsDto { Id = 1, Name = "Eliminar" };
        _actionServiceMock.Setup(s => s.SaveOrEditActions(dto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save error"));

        // Act
        var result = await _controller.SaveOrEditAction(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
