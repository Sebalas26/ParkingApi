using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Notifications;
using ParkingApi.Domain.Interfaces.Services.Notifications;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using WebPush;

namespace ParkingApi.Core.Services.Notifications;

public class PushNotificationService : IPushNotificationService
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushNotificationService> _logger;

    private static VapidDetails? _vapidDetails;
    private static readonly object _vapidLock = new();

    public PushNotificationService(
        DataContext context,
        IConfiguration configuration,
        ILogger<PushNotificationService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
        EnsureVapidDetails();
    }

    private void EnsureVapidDetails()
    {
        if (_vapidDetails != null) return;

        lock (_vapidLock)
        {
            if (_vapidDetails != null) return;

            var subject = _configuration["Vapid:Subject"] ?? "mailto:soporte@parkflow.app";
            var publicKey = _configuration["Vapid:PublicKey"];
            var privateKey = _configuration["Vapid:PrivateKey"];

            if (string.IsNullOrWhiteSpace(publicKey) || string.IsNullOrWhiteSpace(privateKey))
            {
                // Generar claves VAPID criptográficas en memoria si no están configuradas
                var generatedKeys = VapidHelper.GenerateVapidKeys();
                publicKey = generatedKeys.PublicKey;
                privateKey = generatedKeys.PrivateKey;
                _logger.LogInformation("VAPID Keys generadas dinámicamente para WebPush. Clave Pública: {PublicKey}", publicKey);
            }

            _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
        }
    }

    public string GetVapidPublicKey()
    {
        EnsureVapidDetails();
        return _vapidDetails!.PublicKey;
    }

    public async Task<bool> SubscribeAsync(PushSubscriptionDto dto, int userId, int companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == dto.Endpoint, cancellationToken);

            if (existing != null)
            {
                existing.UserId = userId;
                existing.CompanyId = companyId;
                existing.BranchId = dto.BranchId;
                existing.P256dh = dto.P256dh;
                existing.Auth = dto.Auth;
                existing.DeviceName = dto.DeviceName;
                existing.UserAgent = dto.UserAgent;
                existing.IsActive = true;
            }
            else
            {
                var newSub = new Domain.Models.PushSubscription
                {
                    UserId = userId,
                    CompanyId = companyId,
                    BranchId = dto.BranchId,
                    Endpoint = dto.Endpoint,
                    P256dh = dto.P256dh,
                    Auth = dto.Auth,
                    DeviceName = dto.DeviceName,
                    UserAgent = dto.UserAgent,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };
                await _context.PushSubscriptions.AddAsync(newSub, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar suscripción push para usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UnsubscribeAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            var sub = await _context.PushSubscriptions
                .FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);

            if (sub != null)
            {
                sub.IsActive = false;
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al dar de baja suscripción push para endpoint {Endpoint}", endpoint);
            return false;
        }
    }

    public async Task<UserNotificationPreferenceDto> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var pref = await _context.UserNotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (pref == null)
        {
            return new UserNotificationPreferenceDto();
        }

        return new UserNotificationPreferenceDto
        {
            NotifyAppUpdates = pref.NotifyAppUpdates,
            NotifyShiftOpen = pref.NotifyShiftOpen,
            NotifyShiftClose = pref.NotifyShiftClose,
            NotifyCashDiscrepancy = pref.NotifyCashDiscrepancy,
            NotifyVehicleIncidents = pref.NotifyVehicleIncidents,
            NotifyOverdueVehicles = pref.NotifyOverdueVehicles,
            NotifyCancelledTickets = pref.NotifyCancelledTickets
        };
    }

    public async Task<bool> UpdateUserPreferencesAsync(int userId, UserNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var pref = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

            if (pref == null)
            {
                pref = new UserNotificationPreference
                {
                    UserId = userId,
                    NotifyAppUpdates = dto.NotifyAppUpdates,
                    NotifyShiftOpen = dto.NotifyShiftOpen,
                    NotifyShiftClose = dto.NotifyShiftClose,
                    NotifyCashDiscrepancy = dto.NotifyCashDiscrepancy,
                    NotifyVehicleIncidents = dto.NotifyVehicleIncidents,
                    NotifyOverdueVehicles = dto.NotifyOverdueVehicles,
                    NotifyCancelledTickets = dto.NotifyCancelledTickets,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                await _context.UserNotificationPreferences.AddAsync(pref, cancellationToken);
            }
            else
            {
                pref.NotifyAppUpdates = dto.NotifyAppUpdates;
                pref.NotifyShiftOpen = dto.NotifyShiftOpen;
                pref.NotifyShiftClose = dto.NotifyShiftClose;
                pref.NotifyCashDiscrepancy = dto.NotifyCashDiscrepancy;
                pref.NotifyVehicleIncidents = dto.NotifyVehicleIncidents;
                pref.NotifyOverdueVehicles = dto.NotifyOverdueVehicles;
                pref.NotifyCancelledTickets = dto.NotifyCancelledTickets;
                pref.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar preferencias de notificación para usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<int> SendPushNotificationAsync(int companyId, int? branchId, string notificationType, string title, string message, string? url = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Validar si la empresa existe
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);

            if (company == null)
            {
                _logger.LogWarning("Notificación push omitida: La empresa {CompanyId} no existe.", companyId);
                return 0;
            }

            var isTestOrSystem = string.Equals(notificationType, "APP_UPDATE", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(notificationType, "SYSTEM", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(notificationType, "TEST", StringComparison.OrdinalIgnoreCase);

            if (!isTestOrSystem && !company.HasPushNotificationsEnabled)
            {
                _logger.LogInformation("Notificación push omitida: La empresa {CompanyId} no tiene habilitadas las notificaciones push de negocio.", companyId);
                return 0;
            }

            // Validar si el tipo de notificación está permitido para la empresa (si no es de prueba o sistema)
            if (!isTestOrSystem && !string.IsNullOrWhiteSpace(company.AllowedPushTypesJson))
            {
                var allowed = JsonSerializer.Deserialize<List<string>>(company.AllowedPushTypesJson);
                if (allowed != null && allowed.Count > 0 && !allowed.Contains(notificationType, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Notificación push omitida: El tipo '{Type}' no está autorizado para la empresa {CompanyId}.", notificationType, companyId);
                    return 0;
                }
            }

            // 2. Obtener suscripciones activas vinculadas a la empresa
            var query = _context.PushSubscriptions
                .Where(s => s.CompanyId == companyId && s.IsActive);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(s => s.BranchId == null || s.BranchId == branchId.Value);
            }

            var subscriptions = await query.ToListAsync(cancellationToken);
            if (subscriptions.Count == 0)
            {
                _logger.LogInformation("Notificación push omitida: No se encontraron suscripciones activas para la empresa {CompanyId} y sede {BranchId}.", companyId, branchId);
                return 0;
            }

            // 3. Obtener preferencias de los usuarios receptores
            var userIds = subscriptions.Select(s => s.UserId).Distinct().ToList();
            var preferences = await _context.UserNotificationPreferences
                .AsNoTracking()
                .Where(p => userIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, cancellationToken);

            var webPushClient = new WebPushClient();
            var payloadObj = new
            {
                notification = new
                {
                    title = title,
                    body = message,
                    icon = "/assets/icons/icon-192x192.png",
                    badge = "/assets/icons/icon-72x72.png",
                    vibrate = new int[] { 100, 50, 100 },
                    data = new
                    {
                        url = url ?? "/",
                        type = notificationType,
                        timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            };
            var payloadJson = JsonSerializer.Serialize(payloadObj);

            int sentCount = 0;
            var subscriptionsToRemove = new List<Domain.Models.PushSubscription>();

            foreach (var sub in subscriptions)
            {
                // Validar si el usuario tiene deshabilitado este tipo de notificación
                if (preferences.TryGetValue(sub.UserId, out var userPref))
                {
                    bool isAllowed = notificationType.ToUpperInvariant() switch
                    {
                        "APP_UPDATE" => userPref.NotifyAppUpdates,
                        "SHIFT_OPEN" => userPref.NotifyShiftOpen,
                        "SHIFT_CLOSE" => userPref.NotifyShiftClose,
                        "CASH_DISCREPANCY" => userPref.NotifyCashDiscrepancy,
                        "VEHICLE_INCIDENT" => userPref.NotifyVehicleIncidents,
                        "OVERDUE_VEHICLE" => userPref.NotifyOverdueVehicles,
                        "CANCELLED_TICKET" => userPref.NotifyCancelledTickets,
                        _ => true
                    };

                    if (!isAllowed) continue;
                }

                try
                {
                    var pushSub = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                    await webPushClient.SendNotificationAsync(pushSub, payloadJson, _vapidDetails);
                    sub.LastSentAtUtc = DateTime.UtcNow;
                    sentCount++;
                }
                catch (WebPushException webEx)
                {
                    _logger.LogWarning("Fallo al enviar notificación push al dispositivo {Endpoint}: {Status}", sub.Endpoint, webEx.StatusCode);
                    if (webEx.StatusCode == System.Net.HttpStatusCode.Gone || webEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        sub.IsActive = false;
                        subscriptionsToRemove.Add(sub);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error no fatal al despachar WebPush al endpoint {Endpoint}", sub.Endpoint);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("WebPush: Enviadas {SentCount} notificaciones push tipo '{Type}' para empresa {CompanyId}", sentCount, notificationType, companyId);
            return sentCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general al despachar notificaciones WebPush para empresa {CompanyId}", companyId);
            return 0;
        }
    }

    public async Task<int> BroadcastVersionNotificationAsync(BroadcastVersionRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureVapidDetails();

            var subscriptions = await _context.PushSubscriptions
                .Where(s => s.IsActive)
                .ToListAsync(cancellationToken);

            if (subscriptions.Count == 0)
            {
                _logger.LogInformation("Difusión de versión omitida: No hay dispositivos activos registrados.");
                return 0;
            }

            var title = string.IsNullOrWhiteSpace(dto.Title)
                ? (string.IsNullOrWhiteSpace(dto.Version) ? "🚀 ¡Nueva Versión de ParkFlow Disponible!" : $"🚀 ¡Nueva Versión de ParkFlow Disponible! ({dto.Version})")
                : dto.Title;

            var message = string.IsNullOrWhiteSpace(dto.Message)
                ? (string.IsNullOrWhiteSpace(dto.Version) ? "Se ha publicado una nueva actualización en el sistema. Toca aquí para actualizar." : $"Se ha publicado la versión {dto.Version} con mejoras en el sistema. Toca aquí para actualizar.")
                : dto.Message;

            var webPushClient = new WebPushClient();
            var payloadObj = new
            {
                notification = new
                {
                    title = title,
                    body = message,
                    icon = "/assets/icons/icon-192x192.png",
                    badge = "/assets/icons/icon-72x72.png",
                    vibrate = new int[] { 100, 50, 100 },
                    data = new
                    {
                        url = dto.Url ?? "/",
                        type = "APP_UPDATE",
                        version = dto.Version,
                        timestamp = DateTime.UtcNow.ToString("o")
                    }
                }
            };
            var payloadJson = JsonSerializer.Serialize(payloadObj);

            int sentCount = 0;
            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSub = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                    await webPushClient.SendNotificationAsync(pushSub, payloadJson, _vapidDetails);
                    sub.LastSentAtUtc = DateTime.UtcNow;
                    sentCount++;
                }
                catch (WebPushException webEx)
                {
                    _logger.LogWarning("Fallo al enviar notificación push de versión al dispositivo {Endpoint}: {Status}", sub.Endpoint, webEx.StatusCode);
                    if (webEx.StatusCode == System.Net.HttpStatusCode.Gone || webEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        sub.IsActive = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error no fatal al despachar WebPush al endpoint {Endpoint}", sub.Endpoint);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("WebPush Difusión: Notificada versión '{Version}' a {SentCount} dispositivos activos (Android e iOS).", dto.Version, sentCount);
            return sentCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general en difusión masiva de nueva versión.");
            return 0;
        }
    }
}
