using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Plans;
using ParkingApi.Domain.Interfaces.Services.Plans;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;
    private readonly ILogger<PlansController> _logger;

    public PlansController(IPlanService planService, ILogger<PlansController> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var plans = await _planService.GetAllPlansAsync(cancellationToken);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar planes SaaS");
            return StatusCode(500, new { message = "Error al obtener planes SaaS." });
        }
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        try
        {
            var plans = await _planService.GetActivePlansAsync(cancellationToken);
            return Ok(plans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar planes SaaS activos");
            return StatusCode(500, new { message = "Error al obtener planes activos." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var plan = await _planService.GetPlanByIdAsync(id, cancellationToken);
            if (plan == null)
            {
                return NotFound(new { message = $"Plan con ID {id} no encontrado." });
            }
            return Ok(plan);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar plan SaaS {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlanDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "El nombre del plan es requerido." });
            }

            var created = await _planService.CreatePlanAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear plan SaaS {PlanName}", dto.Name);
            return StatusCode(500, new { message = $"Error interno al crear plan: {ex.Message}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePlanDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _planService.UpdatePlanAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar plan SaaS {Id}", id);
            return StatusCode(500, new { message = $"Error interno al actualizar plan: {ex.Message}" });
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _planService.TogglePlanStatusAsync(id, cancellationToken);
            if (!result) return NotFound(new { message = $"Plan con ID {id} no encontrado." });
            return Ok(new { message = "Estado del plan actualizado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del plan SaaS {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar estado del plan." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _planService.DeletePlanAsync(id, cancellationToken);
            if (!result) return NotFound(new { message = $"Plan con ID {id} no encontrado." });
            return Ok(new { message = "Plan eliminado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar plan SaaS {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar plan." });
        }
    }
}
