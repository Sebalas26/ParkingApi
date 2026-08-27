using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Realtime;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Hubs;

namespace ParkingApi.Services.Realtime;

public class RealtimeNotificationService : IRealtimeNotificationService
{
    private readonly IHubContext<ParkingHub> _hubContext;
    private readonly ILogger<RealtimeNotificationService> _logger;

    public RealtimeNotificationService(
        IHubContext<ParkingHub> hubContext,
        ILogger<RealtimeNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyBranchConfigChangedAsync(
        int branchId, 
        string title, 
        string message, 
        string eventType = "BranchConfigChanged", 
        CancellationToken cancellationToken = default)
    {
        var notification = new ConfigNotificationDto
        {
            EventType = eventType,
            BranchId = branchId,
            Title = title,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };

        await NotifyCustomAsync(notification, cancellationToken);
    }

    public async Task NotifyGlobalConfigChangedAsync(
        string eventType, 
        string title, 
        string message, 
        CancellationToken cancellationToken = default)
    {
        var notification = new ConfigNotificationDto
        {
            EventType = eventType,
            BranchId = null,
            Title = title,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };

        await NotifyCustomAsync(notification, cancellationToken);
    }

    public async Task NotifyCustomAsync(ConfigNotificationDto notification, CancellationToken cancellationToken = default)
    {
        try
        {
            if (notification.BranchId.HasValue)
            {
                var groupName = $"Branch_{notification.BranchId.Value}";
                _logger.LogInformation("[SignalR] Emitiendo notificación a grupo '{GroupName}': {Title} - {Message}", groupName, notification.Title, notification.Message);
                await _hubContext.Clients.Group(groupName).SendAsync("OnConfigUpdateRequired", notification, cancellationToken);
            }
            else
            {
                _logger.LogInformation("[SignalR] Emitiendo notificación global a todos los clientes: {Title} - {Message}", notification.Title, notification.Message);
                await _hubContext.Clients.All.SendAsync("OnConfigUpdateRequired", notification, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error al emitir notificación en tiempo real.");
        }
    }
}
