using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Services.Analytics;

namespace ParkingApi.Core.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly ILogger<AnalyticsService> _logger;
    private const int TotalCapacity = 120;

    public AnalyticsService(IParkingTicketRepository ticketRepository, ILogger<AnalyticsService> logger)
    {
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<FinancialSummaryDto> GetDailySummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var todayTickets = await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken);
            var activeCount = await _ticketRepository.CountActiveAsync(cancellationToken);

            var totalRevenue = todayTickets.Sum(t => t.NetAmount);
            var completedCount = todayTickets.Count;
            var avgDuration = completedCount > 0 ? todayTickets.Average(t => t.TotalDurationMinutes) : 0;

            var revenueByType = todayTickets
                .GroupBy(t => t.VehicleType)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NetAmount));

            var countByType = todayTickets
                .GroupBy(t => t.VehicleType)
                .ToDictionary(g => g.Key, g => g.Count());

            var revenueByPayment = todayTickets
                .Where(t => t.PaymentMethod.HasValue)
                .GroupBy(t => t.PaymentMethod!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NetAmount));

            return new FinancialSummaryDto
            {
                TotalRevenueToday = totalRevenue,
                ActiveVehiclesCount = activeCount,
                CompletedTransactionsToday = completedCount,
                AverageDurationMinutes = Math.Round(avgDuration, 1),
                RevenueByVehicleType = revenueByType,
                CountByVehicleType = countByType,
                RevenueByPaymentMethod = revenueByPayment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar resumen financiero diario");
            return new FinancialSummaryDto();
        }
    }

    public async Task<OccupancyStatsDto> GetOccupancyStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeCount = await _ticketRepository.CountActiveAsync(cancellationToken);
            return new OccupancyStatsDto
            {
                TotalCapacity = TotalCapacity,
                OccupiedSpots = activeCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de ocupación");
            return new OccupancyStatsDto { TotalCapacity = TotalCapacity, OccupiedSpots = 0 };
        }
    }
}
