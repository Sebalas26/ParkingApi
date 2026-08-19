using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Rates;

public class VehicleRateDto
{
    public Guid RateId { get; set; }
    public VehicleType VehicleType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal HourRate { get; set; }
    public decimal MinuteRate { get; set; }
    public decimal FullDayRate { get; set; }
    public int GracePeriodMinutes { get; set; }
    public string IconKey { get; set; } = "IconCar";
    public bool IsActive { get; set; }
}

public class CreateRateDto
{
    public VehicleType VehicleType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal HourRate { get; set; }
    public decimal MinuteRate { get; set; }
    public decimal FullDayRate { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public string IconKey { get; set; } = "IconCar";
}

public class UpdateRateDto
{
    public string DisplayName { get; set; } = string.Empty;
    public decimal HourRate { get; set; }
    public decimal MinuteRate { get; set; }
    public decimal FullDayRate { get; set; }
    public int GracePeriodMinutes { get; set; }
    public bool IsActive { get; set; } = true;
}
