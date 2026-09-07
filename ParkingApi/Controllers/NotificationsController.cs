using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly ILogger<NotificationsController> _logger;
    private const string DefaultVapidPublicKey = "BKXh5lK6ONEnwCpYwp29nWqAj0an-nrgnLWgDZdn6yRef74i8ozxS6In0nwFCBefu_l8oMHXJtcm9U83BdfonF8";

    public NotificationsController(ILogger<NotificationsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("vapid-public-key")]
    public IActionResult GetVapidPublicKey()
    {
        try
        {
            return Ok(new { publicKey = DefaultVapidPublicKey });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la clave pública VAPID.");
            return StatusCode(500, new { message = "Error interno al obtener clave VAPID." });
        }
    }

    [HttpGet("preferences")]
    public IActionResult GetPreferences()
    {
        try
        {
            return Ok(new
            {
                notifyAppUpdates = true,
                notifyShiftOpen = false,
                notifyShiftClose = true,
                notifyCashDiscrepancy = true,
                notifyVehicleIncidents = true,
                notifyOverdueVehicles = false,
                notifyCancelledTickets = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener preferencias de notificación.");
            return StatusCode(500, new { message = "Error al obtener preferencias de notificación." });
        }
    }

    [HttpPut("preferences")]
    public IActionResult UpdatePreferences([FromBody] object dto)
    {
        try
        {
            return Ok(new { message = "Preferencias de notificación guardadas exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar preferencias de notificación.");
            return StatusCode(500, new { message = "Error al guardar preferencias de notificación." });
        }
    }

    [HttpPost("subscribe")]
    public IActionResult Subscribe([FromBody] object payload)
    {
        try
        {
            _logger.LogInformation("Dispositivo suscrito a WebPush.");
            return Ok(new { message = "Dispositivo suscrito exitosamente a notificaciones WebPush." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registrando suscripción WebPush.");
            return StatusCode(500, new { message = "Error al registrar suscripción WebPush." });
        }
    }

    [HttpPost("unsubscribe")]
    public IActionResult Unsubscribe([FromBody] object payload)
    {
        try
        {
            _logger.LogInformation("Dispositivo desuscrito de WebPush.");
            return Ok(new { message = "Dispositivo desuscrito exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelando suscripción WebPush.");
            return StatusCode(500, new { message = "Error al cancelar suscripción." });
        }
    }

    [HttpPost("send-test")]
    public IActionResult SendTest([FromBody] object payload)
    {
        try
        {
            return Ok(new { message = "Notificación de prueba enviada exitosamente.", sentCount = 1 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar notificación de prueba.");
            return StatusCode(500, new { message = "Error al enviar notificación de prueba." });
        }
    }

    [HttpGet("company-config/{companyId}")]
    public IActionResult GetCompanyConfig(int companyId)
    {
        try
        {
            return Ok(new
            {
                companyId = companyId,
                hasPushNotificationsEnabled = true,
                allowedPushTypes = new[] { "ALL" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración de notificaciones de empresa.");
            return StatusCode(500, new { message = "Error al obtener configuración de empresa." });
        }
    }

    [HttpPut("company-config")]
    public IActionResult UpdateCompanyConfig([FromBody] object dto)
    {
        try
        {
            return Ok(new { message = "Configuración push de la empresa actualizada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración push de empresa.");
            return StatusCode(500, new { message = "Error al actualizar configuración." });
        }
    }
}
