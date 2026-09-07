using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Notifications;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Notifications;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IPushNotificationService _pushService;
    private readonly ICurrentUserService _currentUser;
    private readonly DataContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        IPushNotificationService pushService,
        ICurrentUserService currentUser,
        DataContext context,
        ILogger<NotificationsController> logger)
    {
        _pushService = pushService;
        _currentUser = currentUser;
        _context = context;
        _logger = logger;
    }

    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    public IActionResult GetVapidPublicKey()
    {
        var key = _pushService.GetVapidPublicKey();
        return Ok(new { publicKey = key });
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint) || string.IsNullOrWhiteSpace(dto.P256dh) || string.IsNullOrWhiteSpace(dto.Auth))
        {
            return BadRequest(new { message = "Los datos de la suscripción WebPush (endpoint, p256dh, auth) son requeridos." });
        }

        var userId = _currentUser.ParsedUserId ?? 0;
        var companyId = _currentUser.CompanyId ?? 0;

        if (userId <= 0 || companyId <= 0)
        {
            return Unauthorized(new { message = "Sesión no válida para vincular dispositivo WebPush." });
        }

        var success = await _pushService.SubscribeAsync(dto, userId, companyId, cancellationToken);
        if (success)
        {
            return Ok(new { message = "Dispositivo suscrito a notificaciones push exitosamente." });
        }

        return StatusCode(500, new { message = "No se pudo registrar la suscripción push." });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint))
        {
            return BadRequest(new { message = "El endpoint es obligatorio para cancelar la suscripción." });
        }

        var success = await _pushService.UnsubscribeAsync(dto.Endpoint, cancellationToken);
        return Ok(new { message = "Suscripción cancelada correctamente." });
    }

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = _currentUser.ParsedUserId ?? 0;
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var prefs = await _pushService.GetUserPreferencesAsync(userId, cancellationToken);
        return Ok(prefs);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UserNotificationPreferenceDto dto, CancellationToken cancellationToken)
    {
        var userId = _currentUser.ParsedUserId ?? 0;
        if (userId <= 0)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var success = await _pushService.UpdateUserPreferencesAsync(userId, dto, cancellationToken);
        if (success)
        {
            return Ok(new { message = "Preferencias de notificación actualizadas exitosamente." });
        }

        return StatusCode(500, new { message = "Error al actualizar preferencias de notificación." });
    }

    [HttpPost("send-test")]
    public async Task<IActionResult> SendTest([FromBody] SendPushNotificationRequestDto dto, CancellationToken cancellationToken)
    {
        var companyId = dto.CompanyId ?? _currentUser.CompanyId ?? 0;
        if (companyId <= 0)
        {
            return BadRequest(new { message = "CompanyId requerido para enviar notificación push." });
        }

        var sentCount = await _pushService.SendPushNotificationAsync(
            companyId,
            dto.BranchId,
            string.IsNullOrWhiteSpace(dto.NotificationType) ? "APP_UPDATE" : dto.NotificationType,
            string.IsNullOrWhiteSpace(dto.Title) ? "🔔 Notificación de Prueba ParkFlow" : dto.Title,
            string.IsNullOrWhiteSpace(dto.Message) ? "Las notificaciones push WebPush VAPID están funcionando correctamente en tu dispositivo." : dto.Message,
            dto.Url,
            cancellationToken);

        return Ok(new { message = $"Notificación enviada a {sentCount} dispositivos.", sentCount });
    }

    [HttpGet("company-config/{companyId:int}")]
    public async Task<IActionResult> GetCompanyPushConfig(int companyId, CancellationToken cancellationToken)
    {
        var company = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company == null) return NotFound(new { message = "Empresa no encontrada." });

        List<string> allowedTypes = new();
        if (!string.IsNullOrWhiteSpace(company.AllowedPushTypesJson))
        {
            try
            {
                allowedTypes = JsonSerializer.Deserialize<List<string>>(company.AllowedPushTypesJson) ?? new();
            }
            catch { }
        }

        return Ok(new
        {
            companyId = company.Id,
            hasPushNotificationsEnabled = company.HasPushNotificationsEnabled,
            allowedPushTypes = allowedTypes
        });
    }

    [HttpPut("company-config")]
    [Authorize(Roles = "SuperAdmin,Administrador")]
    public async Task<IActionResult> UpdateCompanyPushConfig([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        if (!body.TryGetProperty("companyId", out var compProp) || !compProp.TryGetInt32(out int companyId))
        {
            return BadRequest(new { message = "companyId requerido." });
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company == null) return NotFound(new { message = "Empresa no encontrada." });

        if (body.TryGetProperty("hasPushNotificationsEnabled", out var enabledProp))
        {
            company.HasPushNotificationsEnabled = enabledProp.GetBoolean();
        }

        if (body.TryGetProperty("allowedPushTypes", out var typesProp))
        {
            company.AllowedPushTypesJson = typesProp.GetRawText();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Configuración de notificaciones push de la empresa actualizada correctamente." });
    }
}
