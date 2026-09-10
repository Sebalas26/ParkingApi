using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Services.Tickets;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly IParkingTicketService _ticketService;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        IParkingTicketService ticketService,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser,
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.CheckInAsync(dto, cancellationToken);
            if (ticket != null && ticket.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyCustomAsync(new ParkingApi.Domain.Dtos.Realtime.ConfigNotificationDto
                {
                    EventType = "TicketCheckedIn",
                    BranchId = ticket.BranchId,
                    CompanyId = ticket.CompanyId,
                    EntityId = ticket.TicketId,
                    EntityIdentifier = ticket.PlateNumber,
                    Title = "Ingreso de Vehículo",
                    Message = $"Vehículo con placa {ticket.PlateNumber} ingresado.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint check-in");
            return StatusCode(500, new { message = "Error interno del servidor al procesar el ingreso." });
        }
    }

    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var existingTicket = await _ticketService.GetByIdAsync(dto.TicketId, cancellationToken);
            bool wasAlreadyCompleted = existingTicket != null && existingTicket.Status == ParkingApi.Domain.Common.Enums.TicketStatus.Completed;

            var ticket = await _ticketService.CheckOutAsync(dto, cancellationToken);
            if (ticket == null)
            {
                return NotFound(new { message = "Tiquete no encontrado." });
            }

            // Solo emitir notificación SignalR si el tiquete acaba de ser liquidado en esta transacción
            // (evita tormentas de notificaciones durante la sincronización de colas de terminales offline)
            if (!wasAlreadyCompleted && ticket.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyCustomAsync(new ParkingApi.Domain.Dtos.Realtime.ConfigNotificationDto
                {
                    EventType = "TicketCheckedOut",
                    BranchId = ticket.BranchId,
                    CompanyId = ticket.CompanyId,
                    EntityId = ticket.TicketId,
                    EntityIdentifier = ticket.PlateNumber,
                    Title = "Salida de Vehículo",
                    Message = $"Vehículo con placa {ticket.PlateNumber} liquidado.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint check-out para tiquete {TicketId}", dto.TicketId);
            return StatusCode(500, new { message = "Error interno del servidor al liquidar el tiquete." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] int? branchId, [FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var tickets = await _ticketService.GetActiveTicketsAsync(branchId, effectiveCompanyId, cancellationToken);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint active tickets");
            return StatusCode(500, new { message = "Error interno al consultar tiquetes activos." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.GetByIdAsync(id, cancellationToken);
            if (ticket == null)
            {
                return NotFound(new { message = "Tiquete no encontrado." });
            }
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint get ticket {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar el tiquete." });
        }
    }

    [HttpGet("find/{ticketNumber}")]
    public async Task<IActionResult> GetByNumber(string ticketNumber, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.GetByTicketNumberAsync(ticketNumber, cancellationToken);
            if (ticket == null)
            {
                return NotFound(new { message = "Tiquete no encontrado." });
            }
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint get ticket by number {TicketNumber}", ticketNumber);
            return StatusCode(500, new { message = "Error interno al consultar el tiquete." });
        }
    }

    [HttpPost("{id}/emit-electronic-invoice")]
    public async Task<IActionResult> EmitElectronicInvoice(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketService.EmitElectronicInvoiceAsync(id, cancellationToken);
            if (ticket == null)
            {
                return NotFound(new { message = "Tiquete no encontrado." });
            }
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al emitir factura electrónica para tiquete {Id}", id);
            return StatusCode(500, new { message = "Error interno al emitir factura electrónica." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTime? date,
        [FromQuery] int? branchId,
        [FromQuery] int? companyId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            if (from.HasValue && to.HasValue)
            {
                var historyRange = await _ticketService.GetHistoryAsync(null, branchId, effectiveCompanyId, from, to, cancellationToken);
                return Ok(historyRange);
            }

            var targetDate = date ?? DateTime.UtcNow;
            var history = await _ticketService.GetHistoryAsync(targetDate, branchId, effectiveCompanyId, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en endpoint history");
            return StatusCode(500, new { message = "Error interno al consultar historial de tiquetes." });
        }
    }

    public Task<IActionResult> GetHistory(DateTime? date, int? branchId, int? companyId, CancellationToken cancellationToken)
        => GetHistory(date, branchId, companyId, null, null, cancellationToken);
}
