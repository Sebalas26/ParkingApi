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
    private readonly ILogger<VehicleRatesController> _logger;

    public VehicleRatesController(IVehicleRateService rateService, ILogger<VehicleRatesController> logger)
    {
        _rateService = rateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var rates = await _rateService.GetAllRatesAsync(cancellationToken);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleRate rate, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _rateService.UpdateRateAsync(
                id,
                rate.HourRate,
                rate.MinuteRate,
                rate.FullDayRate,
                rate.GracePeriodMinutes,
                cancellationToken);

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarifa {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar tarifa." });
        }
    }
}
