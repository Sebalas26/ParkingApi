using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<IParkingTicketService> _ticketServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<ILogger<TicketsController>> _loggerMock;
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        _ticketServiceMock = new Mock<IParkingTicketService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<TicketsController>>();

        _controller = new TicketsController(
            _ticketServiceMock.Object,
            _currentUserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CheckIn_WhenSuccessful_ShouldReturnOkWithTicket()
    {
        // Arrange
        var request = new CheckInRequestDto { PlateNumber = "ABC123", VehicleType = VehicleType.Car, BranchId = 1 };
        var ticket = new ParkingTicket { TicketId = Guid.NewGuid(), PlateNumber = "ABC123", Status = TicketStatus.Active };
        _ticketServiceMock.Setup(s => s.CheckInAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _controller.CheckIn(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ticket);
    }

    [Fact]
    public async Task CheckIn_WhenInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CheckInRequestDto { PlateNumber = "ABC123" };
        _ticketServiceMock.Setup(s => s.CheckInAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("El vehículo ya se encuentra dentro del parqueadero"));

        // Act
        var result = await _controller.CheckIn(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CheckIn_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _ticketServiceMock.Setup(s => s.CheckInAsync(It.IsAny<CheckInRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.CheckIn(new CheckInRequestDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CheckOut_WhenSuccessful_ShouldReturnOkWithTicket()
    {
        // Arrange
        var request = new CheckOutRequestDto { TicketId = Guid.NewGuid(), AmountPaid = 5000 };
        var ticket = new ParkingTicket { TicketId = request.TicketId, Status = TicketStatus.Completed };
        _ticketServiceMock.Setup(s => s.CheckOutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _controller.CheckOut(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ticket);
    }

    [Fact]
    public async Task CheckOut_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var request = new CheckOutRequestDto { TicketId = Guid.NewGuid() };
        _ticketServiceMock.Setup(s => s.CheckOutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);

        // Act
        var result = await _controller.CheckOut(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CheckOut_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _ticketServiceMock.Setup(s => s.CheckOutAsync(It.IsAny<CheckOutRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Checkout failure"));

        // Act
        var result = await _controller.CheckOut(new CheckOutRequestDto(), CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetActive_WhenSuccessful_ShouldReturnActiveTickets()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var tickets = new List<ParkingTicket>
        {
            new() { TicketId = Guid.NewGuid(), PlateNumber = "ABC123", Status = TicketStatus.Active }
        };
        _ticketServiceMock.Setup(s => s.GetActiveTicketsAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tickets);

        // Act
        var result = await _controller.GetActive(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(tickets);
    }

    [Fact]
    public async Task GetActive_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _ticketServiceMock.Setup(s => s.GetActiveTicketsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Query error"));

        // Act
        var result = await _controller.GetActive(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnTicket()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticket = new ParkingTicket { TicketId = id, PlateNumber = "XYZ789" };
        _ticketServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ticket);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);

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
        _ticketServiceMock.Setup(s => s.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetById(id, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetByNumber_WhenFound_ShouldReturnTicket()
    {
        // Arrange
        var ticket = new ParkingTicket { TicketNumber = "PK-001", PlateNumber = "XYZ789" };
        _ticketServiceMock.Setup(s => s.GetByTicketNumberAsync("PK-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _controller.GetByNumber("PK-001", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ticket);
    }

    [Fact]
    public async Task GetByNumber_WhenNotFound_ShouldReturn404()
    {
        // Arrange
        _ticketServiceMock.Setup(s => s.GetByTicketNumberAsync("NON-EXISTENT", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);

        // Act
        var result = await _controller.GetByNumber("NON-EXISTENT", CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByNumber_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _ticketServiceMock.Setup(s => s.GetByTicketNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetByNumber("ERR", CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetHistory_WhenSuccessful_ShouldReturnHistory()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        var date = DateTime.UtcNow.Date;
        var history = new List<ParkingTicket> { new() { TicketId = Guid.NewGuid(), Status = TicketStatus.Completed } };
        _ticketServiceMock.Setup(s => s.GetHistoryAsync(date, 1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _controller.GetHistory(date, 1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(history);
    }

    [Fact]
    public async Task GetHistory_WhenExceptionThrown_ShouldReturn500()
    {
        // Arrange
        _currentUserMock.Setup(c => c.GetEffectiveCompanyId(It.IsAny<int?>())).Returns(1);
        _ticketServiceMock.Setup(s => s.GetHistoryAsync(It.IsAny<DateTime>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("History query failed"));

        // Act
        var result = await _controller.GetHistory(null, 1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }
}
