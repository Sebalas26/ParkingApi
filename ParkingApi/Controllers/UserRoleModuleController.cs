using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services.UserRoleModules;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserRoleModuleController : ControllerBase
{
    private readonly ILogger<UserRoleModuleController> _logger;
    private readonly IUserRoleModuleService _userRoleModuleService;

    public UserRoleModuleController(ILogger<UserRoleModuleController> logger, IUserRoleModuleService userRoleModuleService)
    {
        _logger = logger;
        _userRoleModuleService = userRoleModuleService;
    }

    [HttpGet("GetUserRoleModule")]
    [HttpGet]
    public async Task<IActionResult> GetUserRoleModule(CancellationToken cancellation)
    {
        try
        {
            var result = await _userRoleModuleService.GetUserRoleModules(cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los módulos de rol de usuario");
            return StatusCode(500, new { message = "Error interno al consultar módulos de rol de usuario." });
        }
    }

    [HttpGet("GetUserRoleModule/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserRoleModuleById(int id, CancellationToken cancellation)
    {
        try
        {
            var result = await _userRoleModuleService.GetUserRoleModuleById(id, cancellation);
            if (result == null)
            {
                return NotFound(new { message = "Módulo de rol no encontrado." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el módulo de rol de usuario por id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar módulo de rol." });
        }
    }

    [HttpPost("SaveOrEditUserRoleModule")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditUserRoleModule([FromBody] SaveUserRoleModuleDto saveUserRoleModule, CancellationToken cancellation)
    {
        try
        {
            var result = await _userRoleModuleService.SaveOrEditUserRoleModule(saveUserRoleModule, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el módulo de rol de usuario");
            return StatusCode(500, new { message = "Error interno al guardar módulo de rol de usuario." });
        }
    }
}
