using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingApi.Controllers;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Shifts;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Shifts;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Models;
using Xunit;

namespace ParkingApi.UnitTests.Controllers;

public class ShiftsAndTicketsControllerTests
{
    private readonly Mock<IShiftService> _shiftServiceMock = new();
    private readonly Mock<IParkingTicketService> _ticketServiceMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IParkingTicketRepository> _ticketRepoMock = new();
    private readonly Mock<IVehicleRateRepository> _rateRepoMock = new();
    private readonly Mock<ILogger<ShiftsController>> _shiftsLoggerMock = new();
    private readonly Mock<ILogger<TicketsController>> _ticketsLoggerMock = new();
    private readonly Mock<ILogger<PublicTicketsController>> _publicTicketsLoggerMock = new();

    private static ControllerContext CreateControllerContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Sid, "1"),
            new Claim(ClaimTypes.Name, "operador1"),
            new Claim(ClaimTypes.Role, "Administrador")
        }, "TestAuth"));

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ShiftsController_OpenShift_ShouldReturnOkWithCreatedShift()
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns("1");

        var controller = new ShiftsController(
            _shiftServiceMock.Object,
            _currentUserMock.Object,
            _userRepositoryMock.Object,
            _shiftsLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        var request = new OpenShiftRequestDto
        {
            BranchId = 1,
            CompanyId = 10,
            CashRegisterName = "Caja Principal",
            BaseAmount = 50000
        };

        var shiftDto = new WorkShiftDto
        {
            ShiftId = Guid.NewGuid(),
            CashRegisterName = "Caja Principal",
            BaseAmount = 50000,
            Status = ShiftStatus.Open
        };

        _shiftServiceMock.Setup(s => s.OpenShiftAsync(It.IsAny<int>(), It.IsAny<string>(), request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shiftDto);

        // Act
        var result = await controller.OpenShift(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(shiftDto);
    }

    [Fact]
    public async Task ShiftsController_GetActiveList_ShouldReturnActiveShifts()
    {
        // Arrange
        _currentUserMock.Setup(c => c.IsSuperAdmin).Returns(true);
        var controller = new ShiftsController(
            _shiftServiceMock.Object,
            _currentUserMock.Object,
            _userRepositoryMock.Object,
            _shiftsLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        var shifts = new List<WorkShiftDto>
        {
            new() { ShiftId = Guid.NewGuid(), CashRegisterName = "Caja 1", Status = ShiftStatus.Open },
            new() { ShiftId = Guid.NewGuid(), CashRegisterName = "Caja 2", Status = ShiftStatus.Open }
        };

        _shiftServiceMock.Setup(s => s.GetActiveShiftsAsync(It.IsAny<int?>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shifts);

        // Act
        var result = await controller.GetActiveList(null, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(shifts);
    }

    [Fact]
    public async Task TicketsController_CheckIn_ShouldReturnOkWithTicket()
    {
        // Arrange
        var controller = new TicketsController(
            _ticketServiceMock.Object,
            _currentUserMock.Object,
            _ticketsLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        var checkInReq = new CheckInRequestDto { PlateNumber = "ABC123", VehicleType = VehicleType.Car, BranchId = 1 };
        var ticket = new ParkingTicket { TicketId = Guid.NewGuid(), PlateNumber = "ABC123", Status = TicketStatus.Active };

        _ticketServiceMock.Setup(s => s.CheckInAsync(checkInReq, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await controller.CheckIn(checkInReq, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(ticket);
    }

    [Fact]
    public async Task PublicTicketsController_GetTicketStatus_WhenNotFound_ShouldReturnOkWithIsFoundFalse()
    {
        // Arrange
        var controller = new PublicTicketsController(
            _ticketRepoMock.Object,
            _rateRepoMock.Object,
            _publicTicketsLoggerMock.Object)
        {
            ControllerContext = CreateControllerContext()
        };

        _ticketRepoMock.Setup(r => r.GetByTicketNumberAsync("NON_EXISTING", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ParkingTicket?)null);

        // Act
        var result = await controller.GetTicketStatus(null, "NON_EXISTING", CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<PublicTicketStatusDto>()
            .Which.IsFound.Should().BeFalse();
    }
}
