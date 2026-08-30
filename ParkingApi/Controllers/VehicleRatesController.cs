using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Services.VehicleRates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleRatesController : ControllerBase
{
    private readonly IVehicleRateService _rateService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ILogger<VehicleRatesController> _logger;

    public VehicleRatesController(
        IVehicleRateService rateService, 
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ILogger<VehicleRatesController> logger)
    {
        _rateService = rateService;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        try
        {
            if (!companyId.HasValue || companyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    companyId = cid;
                }
            }

            var rates = await _rateService.GetAllRatesAsync(companyId, cancellationToken);
            return Ok(rates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifas");
            return StatusCode(500, new { message = "Error interno al consultar tarifas." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var rate = await _rateService.GetByIdAsync(id, cancellationToken);
            if (rate == null)
            {
                return NotFound(new { message = "Tarifa no encontrada." });
            }
            return Ok(rate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifa {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar tarifa." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleRate rate, CancellationToken cancellationToken)
    {
        try
        {
            if (!rate.CompanyId.HasValue || rate.CompanyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    rate.CompanyId = cid;
                }
            }

            var created = await _rateService.CreateRateAsync(rate, cancellationToken);
            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("RatesChanged", "Tarifa de Vehículos Creada", "Se ha agregado una nueva tarifa al sistema.", cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.RateId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tarifa");
            return StatusCode(500, new { message = "Error interno al crear tarifa." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRate rate, CancellationToken cancellationToken)
    {
        try
        {
            rate.RateId = id;
            var updated = await _rateService.UpdateRateAsync(rate, cancellationToken);

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("RatesChanged", "Tarifas Actualizadas", "Se han modificado las tarifas y minutos de gracia en la plataforma.", cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Tarifa no encontrada." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarifa {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar tarifa." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _rateService.DeleteRateAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { message = "Tarifa no encontrada o no se pudo eliminar." });
            }

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("RatesChanged", "Tarifa Eliminada", "Se ha eliminado una tarifa vehicular del sistema.", cancellationToken);
            return Ok(new { success = true, message = "Tarifa eliminada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tarifa {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar tarifa." });
        }
    }
}
