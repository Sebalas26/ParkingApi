using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Services.Tickets;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IParkingTicketService _ticketService;

    public TicketsController(IParkingTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.CheckInAsync(dto, cancellationToken);
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto dto, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.CheckOutAsync(dto, cancellationToken);
        if (ticket == null)
        {
            return NotFound(new { message = "Tiquete no encontrado o ya liquidado." });
        }
        return Ok(ticket);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var tickets = await _ticketService.GetActiveTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
        if (ticket == null) return NotFound(new { message = "Tiquete no encontrado." });
        return Ok(ticket);
    }

    [HttpGet("find/{ticketNumber}")]
    public async Task<IActionResult> GetByNumber(string ticketNumber, CancellationToken cancellationToken)
    {
        var ticket = await _ticketService.GetByTicketNumberAsync(ticketNumber, cancellationToken);
        if (ticket == null) return NotFound(new { message = "Tiquete no encontrado." });
        return Ok(ticket);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var history = await _ticketService.GetHistoryAsync(targetDate, cancellationToken);
        return Ok(history);
    }
}
