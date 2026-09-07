using System;

namespace ParkingApi.Domain.Models;

public class PushSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSentAtUtc { get; set; }

    public virtual User? User { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
}
