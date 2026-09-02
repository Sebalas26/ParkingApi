using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Dtos.Analytics;

public class HourlyTrafficDto
{
    public int Hour { get; set; }
    public string HourLabel { get; set; } = string.Empty;
    public int EntriesCount { get; set; }
}

public class PeakTrafficReportDto
{
    public string Period { get; set; } = "today";
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public int TotalEntries { get; set; }
    public int PeakHour { get; set; }
    public string PeakHourLabel { get; set; } = string.Empty;
    public int PeakEntriesCount { get; set; }
    public double AveragePerHour { get; set; }
    public List<HourlyTrafficDto> HourlyData { get; set; } = new();
}
