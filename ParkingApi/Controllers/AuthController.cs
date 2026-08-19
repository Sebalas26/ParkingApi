using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        if (!response.Success)
        {
            return Unauthorized(new { message = response.ErrorMessage });
        }
        return Ok(response);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (string.IsNullOrEmpty(sid) || !Guid.TryParse(sid, out var userId))
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var success = await _authService.ChangePasswordAsync(userId, dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "Contraseña actual incorrecta." });
        }
        return Ok(new { message = "Contraseña cambiada exitosamente." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (!string.IsNullOrEmpty(sid) && Guid.TryParse(sid, out var userId))
        {
            await _authService.LogoutAsync(userId, cancellationToken);
        }
        return Ok(new { message = "Sesión cerrada correctamente." });
    }
}
