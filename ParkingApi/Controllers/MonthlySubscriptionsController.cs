using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MonthlySubscriptionsController : ControllerBase
{
    private readonly ILogger<MonthlySubscriptionsController> _logger;
    private readonly IMonthlySubscriptionService _subscriptionService;

    public MonthlySubscriptionsController(
        ILogger<MonthlySubscriptionsController> logger,
        IMonthlySubscriptionService subscriptionService)
    {
        _logger = logger;
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await _subscriptionService.GetAllAsync(cancellationToken);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar todas las mensualidades");
            return StatusCode(500, new { message = "Error interno al consultar mensualidades." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await _subscriptionService.GetActiveAsync(cancellationToken);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar mensualidades activas");
            return StatusCode(500, new { message = "Error interno al consultar mensualidades activas." });
        }
    }

    [HttpGet("by-plate/{plate}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByPlate(string plate, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.GetActiveByPlateAsync(plate, cancellationToken);
            if (subscription == null)
            {
                return NotFound(new { message = "No existe mensualidad activa para la placa proporcionada." });
            }
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar mensualidad por placa {Plate}", plate);
            return StatusCode(500, new { message = "Error interno al consultar mensualidad por placa." });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _subscriptionService.GetByIdAsync(id, cancellationToken);
            if (subscription == null)
            {
                return NotFound(new { message = "Mensualidad no encontrada." });
            }
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar mensualidad {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar mensualidad." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMonthlySubscriptionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var created = await _subscriptionService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.SubscriptionId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar mensualidad para placa {Plate}", dto?.PlateNumber);
            return StatusCode(500, new { message = "Error interno al registrar mensualidad." });
        }
    }

    [HttpPost("{id:guid}/renew")]
    public async Task<IActionResult> Renew(Guid id, [FromBody] RenewSubscriptionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var renewed = await _subscriptionService.RenewAsync(id, dto, cancellationToken);
            if (renewed == null)
            {
                return NotFound(new { message = "Mensualidad no encontrada para renovar." });
            }
            return Ok(renewed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar mensualidad {Id}", id);
            return StatusCode(500, new { message = "Error interno al renovar mensualidad." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _subscriptionService.CancelAsync(id, cancellationToken);
            if (!result) return NotFound(new { message = "Mensualidad no encontrada para cancelar." });
            return Ok(new { message = "Mensualidad cancelada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar mensualidad {Id}", id);
            return StatusCode(500, new { message = "Error interno al cancelar mensualidad." });
        }
    }
}
