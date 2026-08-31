using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Interfaces.Services.UserRoles;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserRoleController : ControllerBase
{
    private readonly ILogger<UserRoleController> _logger;
    private readonly IUserRoleService _userRoleService;

    public UserRoleController(ILogger<UserRoleController> logger, IUserRoleService userRoleService)
    {
        _logger = logger;
        _userRoleService = userRoleService;
    }

    [HttpGet("GetUsersRoles")]
    [HttpGet]
    public async Task<IActionResult> GetUsersRoles([FromQuery] int? companyId, [FromQuery] int? branchId, CancellationToken cancellation)
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

            var roles = await _userRoleService.GetUserRoles(companyId, branchId, cancellation);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los roles de los usuarios");
            return StatusCode(500, new { message = "Error interno al consultar roles." });
        }
    }

    [HttpGet("GetUserRole/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserRoleById(int id, CancellationToken cancellation)
    {
        try
        {
            var role = await _userRoleService.GetUserRoleById(id, cancellation);
            if (role == null)
            {
                return NotFound(new { message = "Rol no encontrado." });
            }
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el rol del usuario con id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar rol." });
        }
    }

    [HttpPost("SaveOrEditUserRole")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditUserRole([FromBody] GetUserRoleDto userRole, CancellationToken cancellation)
    {
        try
        {
            if (!userRole.CompanyId.HasValue || userRole.CompanyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    userRole.CompanyId = cid;
                }
            }

            var result = await _userRoleService.SaveOrEditUserRole(userRole, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el rol del usuario");
            return StatusCode(500, new { message = "Error interno al guardar rol." });
        }
    }

    [HttpDelete("DeleteUserRole/{id}")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserRole(int id, CancellationToken cancellation)
    {
        try
        {
            var result = await _userRoleService.DeleteUserRole(id, cancellation);
            if (!result)
            {
                return NotFound(new { message = "Rol no encontrado o no se pudo eliminar." });
            }
            return Ok(new { success = true, message = "Rol eliminado exitosamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el rol #{Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar el rol." });
        }
    }
}
