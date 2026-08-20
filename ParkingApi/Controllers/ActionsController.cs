using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Actions;
using ParkingApi.Domain.Interfaces.Services.Actions;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ActionsController : ControllerBase
{
    private readonly ILogger<ActionsController> _logger;
    private readonly IActionService _actionService;

    public ActionsController(ILogger<ActionsController> logger, IActionService actionService)
    {
        _logger = logger;
        _actionService = actionService;
    }

    [HttpGet("GetActions")]
    [HttpGet]
    public async Task<IActionResult> GetActions(CancellationToken cancellation)
    {
        try
        {
            var actions = await _actionService.GetActions(cancellation);
            return Ok(actions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las acciones");
            return StatusCode(500, new { message = "Error interno al consultar acciones." });
        }
    }

    [HttpGet("GetActionsActive")]
    public async Task<IActionResult> GetActionsActive(CancellationToken cancellation)
    {
        try
        {
            var actions = await _actionService.GetActionsActive(cancellation);
            return Ok(actions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las acciones activas");
            return StatusCode(500, new { message = "Error interno al consultar acciones activas." });
        }
    }

    [HttpGet("GetAction/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetActionById(int id, CancellationToken cancellation)
    {
        try
        {
            var action = await _actionService.GetActionsById(id, cancellation);
            if (action == null)
            {
                return NotFound(new { message = "Acción no encontrada." });
            }
            return Ok(action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la acción por id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar acción." });
        }
    }

    [HttpPost("SaveOrEditAction")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditAction([FromBody] GetActionsDto getAction, CancellationToken cancellation)
    {
        try
        {
            var result = await _actionService.SaveOrEditActions(getAction, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar actions");
            return StatusCode(500, new { message = "Error interno al guardar acción." });
        }
    }
}
