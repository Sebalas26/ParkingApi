using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Services.Analytics;

namespace ParkingApi.Core.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IParkingTicketRepository ticketRepository,
        IConfiguration configuration,
        ILogger<AnalyticsService> logger)
    {
        _ticketRepository = ticketRepository;
        _configuration = configuration;
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

            var countByPayment = todayTickets
                .Where(t => t.PaymentMethod.HasValue)
                .GroupBy(t => ((int)t.PaymentMethod!.Value).ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var countByResolution = todayTickets
                .Where(t => !string.IsNullOrWhiteSpace(t.ResolutionName) || t.ResolutionId.HasValue)
                .GroupBy(t => !string.IsNullOrWhiteSpace(t.ResolutionName) ? t.ResolutionName! : t.ResolutionId.ToString()!)
                .ToDictionary(g => g.Key, g => g.Count());

            var revenueByResolution = todayTickets
                .Where(t => !string.IsNullOrWhiteSpace(t.ResolutionName) || t.ResolutionId.HasValue)
                .GroupBy(t => !string.IsNullOrWhiteSpace(t.ResolutionName) ? t.ResolutionName! : t.ResolutionId.ToString()!)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.NetAmount));

            return new FinancialSummaryDto
            {
                TotalRevenueToday = totalRevenue,
                ActiveVehiclesCount = activeCount,
                CompletedTransactionsToday = completedCount,
                AverageDurationMinutes = Math.Round(avgDuration, 1),
                RevenueByVehicleType = revenueByType,
                CountByVehicleType = countByType,
                RevenueByPaymentMethod = revenueByPayment,
                CountByPaymentMethod = countByPayment,
                CountByResolution = countByResolution,
                RevenueByResolution = revenueByResolution
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
        var totalCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;
        try
        {
            var activeCount = await _ticketRepository.CountActiveAsync(cancellationToken);
            return new OccupancyStatsDto
            {
                TotalCapacity = totalCapacity,
                OccupiedSpots = activeCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de ocupación");
            return new OccupancyStatsDto { TotalCapacity = totalCapacity, OccupiedSpots = 0 };
        }
    }
}
