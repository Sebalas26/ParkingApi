using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Models;

public class VehicleRate
{
    public Guid RateId { get; set; } = Guid.NewGuid();
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal HourRate { get; set; }
    public decimal MinuteRate { get; set; }
    public decimal FullDayRate { get; set; }
    public decimal NightRate { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public string IconKey { get; set; } = "IconCar";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
}
