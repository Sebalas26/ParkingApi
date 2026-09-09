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
using ParkingApi.Domain.Interfaces.Services.Billing;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class DianDocumentTypesControllerTests
{
    private readonly Mock<IDianDocumentTypeService> _serviceMock;
    private readonly Mock<ILogger<DianDocumentTypesController>> _loggerMock;
    private readonly DianDocumentTypesController _controller;

    public DianDocumentTypesControllerTests()
    {
        _serviceMock = new Mock<IDianDocumentTypeService>();
        _loggerMock = new Mock<ILogger<DianDocumentTypesController>>();
        _controller = new DianDocumentTypesController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAll_WhenSuccessful_ShouldReturnOkWithList()
    {
        // Arrange
        var list = new List<DianDocumentTypeDto>
        {
            new() { Id = 1, Name = "Factura Electrónica", Code = "01", DefaultPrefix = "FM", RequiresTechnicalKey = true, IsActive = true },
            new() { Id = 2, Name = "Documento Equivalente POS", Code = "POS", DefaultPrefix = "POS", RequiresTechnicalKey = false, IsActive = true }
        };
        _serviceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnOkWithActiveList()
    {
        // Arrange
        var activeList = new List<DianDocumentTypeDto>
        {
            new() { Id = 1, Name = "Factura Electrónica", Code = "01", DefaultPrefix = "FM", IsActive = true }
        };
        _serviceMock.Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeList);

        // Act
        var result = await _controller.GetActive(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(activeList);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnOkWithItem()
    {
        // Arrange
        var item = new DianDocumentTypeDto { Id = 1, Name = "Factura Electrónica", Code = "01", DefaultPrefix = "FM" };
        _serviceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _controller.GetById(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(item);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DianDocumentTypeDto?)null);

        // Act
        var result = await _controller.GetById(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = new CreateDianDocumentTypeDto { Name = "Tiquete", Code = "TIQ", DefaultPrefix = "TIQ" };
        var created = new DianDocumentTypeDto { Id = 10, Name = "Tiquete", Code = "TIQ", DefaultPrefix = "TIQ", IsActive = true };

        _serviceMock.Setup(s => s.CreateAsync(dto, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>()
            .Which.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task ToggleStatus_WhenFound_ShouldReturnOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.ToggleStatusAsync(1, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ToggleStatus(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenFound_ShouldReturnOk()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
