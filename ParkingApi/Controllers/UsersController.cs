using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.Users;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService;
    private readonly IRealtimeNotificationService _realtimeNotifier;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;

    public UsersController(
        ILogger<UsersController> logger,
        IUserService userService,
        IRealtimeNotificationService realtimeNotifier,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser)
    {
        _logger = logger;
        _userService = userService;
        _realtimeNotifier = realtimeNotifier;
        _currentUser = currentUser;
    }

    [HttpGet("GetUsers")]
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int? companyId, [FromQuery] int? branchId, CancellationToken cancellation)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var users = await _userService.GetUsers(effectiveCompanyId, branchId, cancellation);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, new { message = "Error interno al consultar usuarios." });
        }
    }

    [HttpGet("GetUser/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellation)
    {
        try
        {
            var user = await _userService.GetUserById(id, cancellation);
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            if (!_currentUser.IsSuperAdmin && user.CompanyId.HasValue && !_currentUser.CanAccessCompany(user.CompanyId.Value))
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar usuario." });
        }
    }

    [HttpPost("SaveOrEditUsers")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditUsers([FromBody] GetUsersDto getUsersDto, CancellationToken cancellation)
    {
        try
        {
            if (!_currentUser.IsSuperAdmin)
            {
                getUsersDto.CompanyId = _currentUser.CompanyId;
            }
            else if (!getUsersDto.CompanyId.HasValue || getUsersDto.CompanyId.Value <= 0)
            {
                getUsersDto.CompanyId = _currentUser.CompanyId;
            }

            var result = await _userService.CreateOrEditUser(getUsersDto, cancellation);
            if (result == null)
            {
                return BadRequest(new { message = "No se pudo guardar o editar el usuario." });
            }

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync(
                "UsersChanged",
                "Usuarios Actualizados",
                $"Se guardaron los datos del usuario '{getUsersDto.Username}'.",
                cancellation);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar usuario");
            return StatusCode(500, new { message = "Error interno al guardar usuario." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellation)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id, cancellation);
            if (success)
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync(
                    "UsersChanged",
                    "Usuario Eliminado",
                    $"Se eliminó el usuario con ID {id}.",
                    cancellation);
                return Ok(new { message = "Usuario eliminado correctamente de la base de datos." });
            }
            return NotFound(new { message = "Usuario no encontrado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar usuario." });
        }
    }
}
