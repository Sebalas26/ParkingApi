using System;

namespace ParkingApi.Domain.Dtos.Notifications;

public class PushSubscriptionDto
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? UserAgent { get; set; }
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
}

public class UserNotificationPreferenceDto
{
    public bool NotifyAppUpdates { get; set; } = true;
    public bool NotifyShiftOpen { get; set; } = false;
    public bool NotifyShiftClose { get; set; } = true;
    public bool NotifyCashDiscrepancy { get; set; } = true;
    public bool NotifyVehicleIncidents { get; set; } = true;
    public bool NotifyOverdueVehicles { get; set; } = false;
    public bool NotifyCancelledTickets { get; set; } = true;
}

public class SendPushNotificationRequestDto
{
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string NotificationType { get; set; } = "SYSTEM";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Url { get; set; }
}
