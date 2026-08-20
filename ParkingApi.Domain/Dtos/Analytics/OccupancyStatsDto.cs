using System;
using System.Collections.Generic;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Analytics;

public class OccupancyStatsDto
{
    public int TotalCapacity { get; set; } = 120;
    public int OccupiedSpots { get; set; }
    public int AvailableSpots => Math.Max(0, TotalCapacity - OccupiedSpots);
    public double OccupancyRate => TotalCapacity > 0 ? Math.Round((OccupiedSpots * 100.0) / TotalCapacity, 1) : 0.0;
    public Dictionary<VehicleType, int> OccupancyByType { get; set; } = new();
}
