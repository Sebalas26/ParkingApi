using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Tickets;

public class CheckInRequestDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Notes { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime? EntryTimeUtc { get; set; }
}
