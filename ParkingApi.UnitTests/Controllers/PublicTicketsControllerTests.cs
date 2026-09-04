using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class PublicTicketsControllerTests
{
    private readonly Mock<IParkingTicketRepository> _ticketRepoMock;
    private readonly Mock<IVehicleRateRepository> _rateRepoMock;
    private readonly Mock<ILogger<PublicTicketsController>> _loggerMock;
    private readonly PublicTicketsController _controller;

    public PublicTicketsControllerTests()
    {
        _ticketRepoMock = new Mock<IParkingTicketRepository>();
        _rateRepoMock = new Mock<IVehicleRateRepository>();
        _loggerMock = new Mock<ILogger<PublicTicketsController>>();

        _controller = new PublicTicketsController(
            _ticketRepoMock.Object,
            _rateRepoMock.Object,
            _loggerMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.ControllerContext.HttpContext.Request.Scheme = "https";
        _controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost", 5001);
    }

    [Fact]
    public async Task GetTicketStatus_WhenPlateAndTicketAreEmpty_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetTicketStatus(null, "", CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeOfType<PublicTicketStatusDto>()
            .Which.IsFound.Should().BeFalse();
    }

    [Fact]
    public async Task GetTicketStatus_WhenTicketNotFound_ShouldReturnOkWithIsFoundFalse()
    {
        // Arrange
        _ticketRepoMock.Setup(r => r.GetByTicketNumberAsync("TICKET999", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);

        // Act
        var result = await _controller.GetTicketStatus(null, "TICKET999", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<PublicTicketStatusDto>()
            .Which.IsFound.Should().BeFalse();
    }

    [Fact]
    public async Task GetTicketStatus_WhenActiveTicketFound_ShouldReturnOkWithCalculatedRate()
    {
        // Arrange
        var ticket = new ParkingTicket
        {
            TicketId = Guid.NewGuid(),
            TicketNumber = "PK-12345",
            PlateNumber = "ABC123",
            VehicleType = VehicleType.Car,
            Status = TicketStatus.Active,
            EntryTimeUtc = DateTime.UtcNow.AddMinutes(-45),
            HourlyRate = 3000
        };

        _ticketRepoMock.Setup(r => r.GetByTicketNumberAsync("PK-12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);
        _rateRepoMock.Setup(r => r.GetByTypeAsync(VehicleType.Car, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VehicleRate { HourRate = 3000 });

        // Act
        var result = await _controller.GetTicketStatus(null, "PK-12345", CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var statusDto = okResult.Value.Should().BeOfType<PublicTicketStatusDto>().Subject;
        statusDto.IsFound.Should().BeTrue();
        statusDto.TicketNumber.Should().Be("PK-12345");
        statusDto.Status.Should().Be((int)TicketStatus.Active);
        statusDto.EstimatedAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTicketStatus_WhenCompletedTicketFoundByPlate_ShouldReturnOkWithAmountPaid()
    {
        // Arrange
        var ticket = new ParkingTicket
        {
            TicketId = Guid.NewGuid(),
            TicketNumber = "PK-COMPLETED",
            PlateNumber = "XYZ789",
            VehicleType = VehicleType.Motorcycle,
            Status = TicketStatus.Completed,
            EntryTimeUtc = DateTime.UtcNow.AddHours(-2),
            ExitTimeUtc = DateTime.UtcNow,
            AmountPaid = 5000,
            NetAmount = 5000
        };

        _ticketRepoMock.Setup(r => r.GetActiveByPlateAsync("XYZ789", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);
        _ticketRepoMock.Setup(r => r.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ParkingTicket> { ticket });

        // Act
        var result = await _controller.GetTicketStatus("XYZ789", null, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var statusDto = okResult.Value.Should().BeOfType<PublicTicketStatusDto>().Subject;
        statusDto.IsFound.Should().BeTrue();
        statusDto.PlateNumber.Should().Be("XYZ789");
        statusDto.Status.Should().Be((int)TicketStatus.Completed);
        statusDto.TotalPaid.Should().Be(5000);
    }

    [Fact]
    public async Task GetTicketStatus_WhenExceptionThrown_ShouldReturn500WithIsFoundFalse()
    {
        // Arrange
        _ticketRepoMock.Setup(r => r.GetByTicketNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database fault"));

        // Act
        var result = await _controller.GetTicketStatus(null, "ERR-TICKET", CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(500);
        objResult.Value.Should().BeOfType<PublicTicketStatusDto>()
            .Which.IsFound.Should().BeFalse();
    }
}
