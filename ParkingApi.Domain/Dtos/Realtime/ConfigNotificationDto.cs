using System;

namespace ParkingApi.Domain.Dtos.Realtime;

public class ConfigNotificationDto
{
    public string EventType { get; set; } = "ConfigUpdated";
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
    public int? UserId { get; set; }
    public string? SessionToken { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
