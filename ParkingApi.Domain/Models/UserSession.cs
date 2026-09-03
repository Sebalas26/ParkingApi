using System;

namespace ParkingApi.Domain.Models;

public class UserSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public string Jti { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }

    public virtual User? User { get; set; }
}
