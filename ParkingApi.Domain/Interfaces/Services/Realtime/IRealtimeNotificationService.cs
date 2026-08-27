using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Realtime;

namespace ParkingApi.Domain.Interfaces.Services.Realtime;

public interface IRealtimeNotificationService
{
    Task NotifyBranchConfigChangedAsync(int branchId, string title, string message, string eventType = "BranchConfigChanged", CancellationToken cancellationToken = default);
    Task NotifyGlobalConfigChangedAsync(string eventType, string title, string message, CancellationToken cancellationToken = default);
    Task NotifyCustomAsync(ConfigNotificationDto notification, CancellationToken cancellationToken = default);
}
