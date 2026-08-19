using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Services.Auth;

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
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (string.IsNullOrEmpty(sid) || !Guid.TryParse(sid, out var userId))
        {
            return Unauthorized(new { message = "SesiÃ³n no identificada." });
        }

        var user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user == null) return NotFound(new { message = "Usuario no encontrado." });
        return Ok(user);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
        if (string.IsNullOrEmpty(sid) || !Guid.TryParse(sid, out var userId))
        {
            return Unauthorized(new { message = "SesiÃ³n no vÃ¡lida." });
        }

        var success = await _authService.ChangePasswordAsync(userId, dto, cancellationToken);
        if (!success)
        {
            return BadRequest(new { message = "ContraseÃ±a actual incorrecta." });
        }
        return Ok(new { message = "ContraseÃ±a cambiada exitosamente." });
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
    {
        var success = await _authService.ForgotPasswordAsync(dto, cancellationToken);
        return Ok(new { success, message = "Si el correo estÃ¡ registrado, se enviaron las instrucciones." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var success = await _authService.ResetPasswordAsync(dto, cancellationToken);
        if (!success) return BadRequest(new { message = "Token invÃ¡lido o expirado." });
        return Ok(new { message = "ContraseÃ±a restablecida exitosamente." });
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
        return Ok(new { message = "SesiÃ³n cerrada correctamente." });
    }
}
