using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Services.Auth;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthDto auth, CancellationToken cancellation)
    {
        try
        {
            var result = await _authService.Login(auth, cancellation);
            if (result == null)
            {
                return Unauthorized(new { message = "Credenciales inválidas. Por favor, inténtalo de nuevo." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el proceso de Authenticate");
            return StatusCode(500, new { message = "Error interno del servidor al procesar la autenticación." });
        }
    }

    [AllowAnonymous]
    [HttpPost("login-mobile")]
    public async Task<IActionResult> LoginMobile([FromBody] LoginMobileDto credentials, CancellationToken cancellation)
    {
        try
        {
            var result = await _authService.LoginAsync(credentials, cancellation);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el login móvil");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> LoginStandard([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginStandardAsync(dto, cancellationToken);
            if (!response.Success)
            {
                return Unauthorized(new { message = response.ErrorMessage });
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoginStandard");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellation)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto?.Email))
            {
                return BadRequest(new { message = "Por favor ingresa un correo electrónico válido." });
            }

            var success = await _authService.GeneratePasswordResetTokenAsync(dto.Email.Trim(), cancellation);
            if (success)
            {
                return Ok(new { message = "Se ha generado la solicitud para restablecer tu contraseña." });
            }
            return BadRequest(new { message = "No se pudo procesar la solicitud de recuperación." });
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en forgot-password para {Email}", dto?.Email);
            return StatusCode(500, new { message = "Ocurrió un error al procesar la solicitud. Inténtalo de nuevo más tarde." });
        }
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellation)
    {
        try
        {
            var success = await _authService.ResetPasswordAsync(dto, cancellation);
            if (success)
            {
                return Ok(new { message = "Contraseña restablecida exitosamente." });
            }
            return BadRequest(new { message = "El token es inválido o ha expirado." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en reset-password");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellation)
    {
        try
        {
            var sidClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (string.IsNullOrEmpty(sidClaim) || !int.TryParse(sidClaim, out int userId))
            {
                return Unauthorized(new { message = "Usuario no identificado." });
            }

            var success = await _authService.ChangePasswordAsync(userId, dto, cancellation);
            if (success)
            {
                return Ok(new { message = "Contraseña cambiada exitosamente." });
            }
            return BadRequest(new { message = "No se pudo cambiar la contraseña." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en change-password");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [Authorize]
    [HttpGet("validate-session")]
    public IActionResult ValidateSession()
    {
        var sidClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        return Ok(new
        {
            valid = true,
            userId = sidClaim,
            username = nameClaim,
            validatedAtUtc = DateTime.UtcNow
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellation)
    {
        try
        {
            var sidClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (!string.IsNullOrEmpty(sidClaim) && int.TryParse(sidClaim, out int userId))
            {
                await _authService.LogoutAsync(userId, cancellation);
            }
            return Ok(new { message = "Sesión cerrada correctamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en proceso de logout");
            return StatusCode(500, new { message = "Error al cerrar sesión" });
        }
    }
}
