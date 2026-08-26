using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Incidents;
using ParkingApi.Domain.Interfaces.Services.Incidents;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleIncidentsController : ControllerBase
{
    private readonly IVehicleIncidentService _incidentService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ILogger<VehicleIncidentsController> _logger;

    public VehicleIncidentsController(
        IVehicleIncidentService incidentService, 
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ILogger<VehicleIncidentsController> logger)
    {
        _incidentService = incidentService;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool? isBlocked,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _incidentService.GetAllAsync(branchId, status, isBlocked, search, cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedades de vehículos");
            return StatusCode(500, new { message = "Error interno al consultar novedades." });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _incidentService.GetByIdAsync(id, cancellationToken);
            if (incident == null)
            {
                return NotFound(new { message = "Novedad no encontrada." });
            }
            return Ok(incident);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedad {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar novedad." });
        }
    }

    [HttpGet("check-plate/{plate}")]
    public async Task<IActionResult> CheckPlate(string plate, [FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(plate))
            {
                return BadRequest(new { message = "Debe proporcionar una placa válida para consultar." });
            }

            var result = await _incidentService.CheckPlateAsync(plate, branchId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar placa {Plate}", plate);
            return StatusCode(500, new { message = "Error interno al consultar estado de la placa." });
        }
    }

    [HttpGet("by-plate/{plate}")]
    public async Task<IActionResult> GetByPlate(string plate, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _incidentService.GetByPlateAsync(plate, cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedades para la placa {Plate}", plate);
            return StatusCode(500, new { message = "Error interno al consultar historial de la placa." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveVehicleIncidentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.PlateNumber) || string.IsNullOrWhiteSpace(dto.IncidentType) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new { message = "Placa, tipo de novedad y descripción son obligatorios." });
            }

            var created = await _incidentService.CreateAsync(dto, cancellationToken);

            var title = dto.IsBlocked ? "⛔ Vehículo Bloqueado" : "Novedad de Vehículo Registrada";
            var msg = dto.IsBlocked 
                ? $"Se ha bloqueado el ingreso de la placa '{dto.PlateNumber}' ({dto.IncidentType})."
                : $"Se registró una novedad para la placa '{dto.PlateNumber}'.";

            if (dto.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId.Value, title, msg, "IncidentsChanged", cancellationToken);
            }
            else
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("IncidentsChanged", title, msg, cancellationToken);
            }

            return CreatedAtAction(nameof(GetById), new { id = created.IncidentId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear novedad para placa {Plate}", dto.PlateNumber);
            return StatusCode(500, new { message = "Error interno al registrar novedad." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveVehicleIncidentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.PlateNumber) || string.IsNullOrWhiteSpace(dto.IncidentType) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return BadRequest(new { message = "Placa, tipo de novedad y descripción son obligatorios." });
            }

            var updated = await _incidentService.UpdateAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = "Novedad no encontrada para actualizar." });
            }

            var title = dto.IsBlocked ? "⛔ Estado de Bloqueo Actualizado" : "Novedad de Vehículo Modificada";
            var msg = $"Se actualizaron los datos de la novedad para la placa '{dto.PlateNumber}'.";

            if (dto.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId.Value, title, msg, "IncidentsChanged", cancellationToken);
            }
            else
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("IncidentsChanged", title, msg, cancellationToken);
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar novedad {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar novedad." });
        }
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveIncidentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _incidentService.ResolveAsync(id, dto, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "Novedad no encontrada o no pudo ser resuelta." });
            }

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("IncidentsChanged", "Novedad Resuelta", "Se resolvió una novedad / levantó bloqueo de placa.", cancellationToken);

            return Ok(new { message = "Novedad resuelta y bloqueo levantado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver novedad {Id}", id);
            return StatusCode(500, new { message = "Error interno al resolver novedad." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _incidentService.DeleteAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "Novedad no encontrada o no pudo ser eliminada." });
            }

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("IncidentsChanged", "Novedad Eliminada", "Se eliminó un registro de novedad.", cancellationToken);

            return Ok(new { message = "Novedad eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar novedad {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar novedad." });
        }
    }
}
