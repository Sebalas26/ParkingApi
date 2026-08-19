using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Core.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private const int TotalCapacity = 120;

    public AnalyticsService(IParkingTicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<FinancialSummaryDto> GetDailySummaryAsync(CancellationToken cancellationToken = default)
    {
        var completed = await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken);
        var active = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);

        var totalRev = completed.Sum(t => t.NetAmount);
        var avgDuration = completed.Count > 0 ? completed.Average(t => t.TotalDurationMinutes) : 0.0;

        var revByType = new Dictionary<VehicleType, decimal>();
        var countByType = new Dictionary<VehicleType, int>();

        foreach (VehicleType type in Enum.GetValues<VehicleType>())
        {
            revByType[type] = completed.Where(t => t.VehicleType == type).Sum(t => t.NetAmount);
            countByType[type] = completed.Count(t => t.VehicleType == type) + active.Count(t => t.VehicleType == type);
        }

        return new FinancialSummaryDto
        {
            TotalRevenueToday = totalRev,
            ActiveVehiclesCount = active.Count,
            CompletedTransactionsToday = completed.Count,
            AverageDurationMinutes = avgDuration,
            RevenueByVehicleType = revByType,
            CountByVehicleType = countByType
        };
    }

    public async Task<OccupancyStatsDto> GetOccupancyStatsAsync(CancellationToken cancellationToken = default)
    {
        var active = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
        return new OccupancyStatsDto
        {
            TotalCapacity = TotalCapacity,
            OccupiedSpots = active.Count
        };
    }
}
