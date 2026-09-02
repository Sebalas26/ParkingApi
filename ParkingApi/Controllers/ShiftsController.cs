using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Shifts;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Shifts;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ShiftsController> _logger;

    public ShiftsController(
        IShiftService shiftService,
        ICurrentUserService currentUser,
        ILogger<ShiftsController> logger)
    {
        _shiftService = shiftService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int userId = 1;
            if (int.TryParse(_currentUser?.UserId, out int parsedId) && parsedId > 0)
            {
                userId = parsedId;
            }

            var operatorName = User.FindFirstValue(ClaimTypes.Name) ?? "Operador de Turno";
            var result = await _shiftService.OpenShiftAsync(userId, operatorName, dto, cancellationToken);
            if (result == null)
            {
                return BadRequest(new { message = "No se pudo abrir el turno o ya existe un turno activo para este operador." });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar apertura de turno");
            return StatusCode(500, new { message = "Error interno del servidor al abrir el turno." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] int? userId, [FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        try
        {
            int? queryUser = userId;
            if (!queryUser.HasValue && int.TryParse(_currentUser?.UserId, out int parsedId) && parsedId > 0)
            {
                queryUser = parsedId;
            }

            var shift = await _shiftService.GetActiveShiftAsync(queryUser, branchId, cancellationToken);
            if (shift == null)
            {
                return NotFound(new { message = "No hay turno activo para el operador." });
            }

            return Ok(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar turno activo");
            return StatusCode(500, new { message = "Error interno al consultar el turno activo." });
        }
    }

    [HttpGet("summary/{shiftId}")]
    public async Task<IActionResult> GetSummary(Guid shiftId, CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _shiftService.GetShiftSummaryAsync(shiftId, cancellationToken);
            if (summary == null)
            {
                return NotFound(new { message = "Turno no encontrado." });
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar resumen de turno {ShiftId}", shiftId);
            return StatusCode(500, new { message = "Error interno al calcular el resumen de turno." });
        }
    }

    [HttpPost("close")]
    public async Task<IActionResult> CloseShift([FromBody] CloseShiftRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int userId = 1;
            if (int.TryParse(_currentUser?.UserId, out int parsedId) && parsedId > 0)
            {
                userId = parsedId;
            }

            var closedShift = await _shiftService.CloseShiftAsync(userId, dto, cancellationToken);
            if (closedShift == null)
            {
                return BadRequest(new { message = "No se pudo cerrar el turno. Verifique que el turno exista y esté abierto." });
            }

            return Ok(closedShift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar cierre de turno");
            return StatusCode(500, new { message = "Error interno al liquidar y cerrar el turno." });
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var history = await _shiftService.GetHistoryAsync(fromDate, toDate, branchId, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar historial de turnos");
            return StatusCode(500, new { message = "Error interno al consultar historial de turnos." });
        }
    }
}
