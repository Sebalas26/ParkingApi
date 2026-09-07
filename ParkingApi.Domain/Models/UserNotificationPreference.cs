using System;

namespace ParkingApi.Domain.Models;

public class UserNotificationPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool NotifyAppUpdates { get; set; } = true;
    public bool NotifyShiftOpen { get; set; } = false;
    public bool NotifyShiftClose { get; set; } = true;
    public bool NotifyCashDiscrepancy { get; set; } = true;
    public bool NotifyVehicleIncidents { get; set; } = true;
    public bool NotifyOverdueVehicles { get; set; } = false;
    public bool NotifyCancelledTickets { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}
