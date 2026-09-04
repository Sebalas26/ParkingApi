using System.Collections.Generic;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Analytics;

public class FinancialSummaryDto
{
    public decimal TotalRevenueToday { get; set; }
    public int ActiveVehiclesCount { get; set; }
    public int CompletedTransactionsToday { get; set; }
    public double AverageDurationMinutes { get; set; }
    public decimal TotalRevenue => TotalRevenueToday;
    public int ActiveTickets => ActiveVehiclesCount;
    public int CompletedTickets => CompletedTransactionsToday;
    public int TotalTickets => ActiveVehiclesCount + CompletedTransactionsToday;
    public Dictionary<VehicleType, decimal> RevenueByVehicleType { get; set; } = new();
    public Dictionary<VehicleType, int> CountByVehicleType { get; set; } = new();
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
    public Dictionary<string, int> CountByPaymentMethod { get; set; } = new();
    public Dictionary<string, int> CountByResolution { get; set; } = new();
    public Dictionary<string, decimal> RevenueByResolution { get; set; } = new();
}
