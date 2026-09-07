using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Notifications;

namespace ParkingApi.Domain.Interfaces.Services.Notifications;

public interface IPushNotificationService
{
    string GetVapidPublicKey();
    Task<bool> SubscribeAsync(PushSubscriptionDto dto, int userId, int companyId, CancellationToken cancellationToken = default);
    Task<bool> UnsubscribeAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<UserNotificationPreferenceDto> GetUserPreferencesAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserPreferencesAsync(int userId, UserNotificationPreferenceDto dto, CancellationToken cancellationToken = default);
    Task<int> SendPushNotificationAsync(int companyId, int? branchId, string notificationType, string title, string message, string? url = null, CancellationToken cancellationToken = default);
    Task<int> BroadcastVersionNotificationAsync(BroadcastVersionRequestDto dto, CancellationToken cancellationToken = default);
}
