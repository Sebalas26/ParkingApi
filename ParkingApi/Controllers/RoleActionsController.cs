using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.RoleActions;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleActionsController : ControllerBase
{
    private readonly ILogger<RoleActionsController> _logger;
    private readonly IRoleActionService _roleActionService;
    private readonly IRealtimeNotificationService _realtimeNotifier;

    public RoleActionsController(
        ILogger<RoleActionsController> logger,
        IRoleActionService roleActionService,
        IRealtimeNotificationService realtimeNotifier)
    {
        _logger = logger;
        _roleActionService = roleActionService;
        _realtimeNotifier = realtimeNotifier;
    }

    [HttpGet("GetRoleActions/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleActions(int id, CancellationToken cancellation)
    {
        try
        {
            var actions = await _roleActionService.GetActionsByRoleIdAsync(id, cancellation);
            return Ok(actions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando las acciones del rol {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar las acciones del rol." });
        }
    }

    [HttpGet("PermissionRole/{id}")]
    public async Task<IActionResult> PermissionRole(int id, CancellationToken cancellation)
    {
        try
        {
            var permissions = await _roleActionService.GetActionsByRoleAsync(id, cancellation);
            return Ok(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando los permisos del rol {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar permisos del rol." });
        }
    }

    [HttpPost("AssignRolePermissions")]
    public async Task<IActionResult> AssignRolePermissions([FromBody] AssignRolePermissionsDto dto, CancellationToken cancellation)
    {
        try
        {
            var result = await _roleActionService.AssignRolePermissionsAsync(dto.RoleId, dto.ActionIds, cancellation);
            if (result)
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync(
                    "PermissionsChanged",
                    "Permisos Actualizados",
                    $"Se actualizaron los permisos asignados al rol con ID {dto.RoleId}.",
                    cancellation);
            }
            return Ok(new { success = result, message = result ? "Permisos asignados correctamente." : "Error al asignar permisos." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asignando permisos al rol {RoleId}", dto.RoleId);
            return StatusCode(500, new { message = "Error interno al asignar permisos al rol." });
        }
    }
}
