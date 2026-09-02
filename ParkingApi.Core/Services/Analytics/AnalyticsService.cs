using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Services.Analytics;

namespace ParkingApi.Core.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IConfiguration _configuration;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IParkingTicketRepository ticketRepository,
        IBranchRepository branchRepository,
        IConfiguration configuration,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser,
        ILogger<AnalyticsService> logger)
    {
        _ticketRepository = ticketRepository;
        _branchRepository = branchRepository;
        _configuration = configuration;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<FinancialSummaryDto> GetDailySummaryAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveCompanyId = companyId ?? _currentUser.CompanyId;
            var todayTickets = await _ticketRepository.GetTodayCompletedTicketsAsync(branchId, effectiveCompanyId, cancellationToken);
            var activeCount = await _ticketRepository.CountActiveAsync(branchId, effectiveCompanyId, cancellationToken);

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
            _logger.LogError(ex, "Error al generar resumen financiero diario para sede {BranchId}, empresa {CompanyId}", branchId, companyId);
            return new FinancialSummaryDto();
        }
    }

    public async Task<OccupancyStatsDto> GetOccupancyStatsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveCompanyId = companyId ?? _currentUser.CompanyId;

            int totalCapacity = 100;
            if (branchId.HasValue && branchId.Value > 0)
            {
                var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
                totalCapacity = branch?.TotalCapacity > 0 ? branch.TotalCapacity : 100;
            }
            else
            {
                var activeBranches = await _branchRepository.GetActiveAsync(effectiveCompanyId, cancellationToken);
                totalCapacity = activeBranches.Sum(b => b.TotalCapacity);
                if (totalCapacity <= 0)
                {
                    totalCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;
                }
            }

            var activeCount = await _ticketRepository.CountActiveAsync(branchId, effectiveCompanyId, cancellationToken);
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(branchId, effectiveCompanyId, cancellationToken);

            var occupancyByType = activeTickets
                .GroupBy(t => t.VehicleType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new OccupancyStatsDto
            {
                TotalCapacity = totalCapacity,
                OccupiedSpots = activeCount,
                OccupancyByType = occupancyByType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de ocupación para sede {BranchId}, empresa {CompanyId}", branchId, companyId);
            var fallbackCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;
            return new OccupancyStatsDto { TotalCapacity = fallbackCapacity, OccupiedSpots = 0 };
        }
    }

    public async Task<PeakTrafficReportDto> GetPeakTrafficAsync(
        string? period,
        int? branchId,
        int? companyId,
        int offsetMinutes = 300,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPeriod = string.IsNullOrWhiteSpace(period) ? "today" : period.ToLowerInvariant();
            var nowUtc = DateTime.UtcNow;
            var clientNow = nowUtc.AddMinutes(-offsetMinutes);

            DateTime fromLocal;
            DateTime toLocal;

            switch (normalizedPeriod)
            {
                case "yesterday":
                    var yesterday = clientNow.Date.AddDays(-1);
                    fromLocal = yesterday;
                    toLocal = yesterday.AddDays(1).AddTicks(-1);
                    break;
                case "month":
                    fromLocal = new DateTime(clientNow.Year, clientNow.Month, 1);
                    toLocal = fromLocal.AddMonths(1).AddTicks(-1);
                    break;
                case "today":
                default:
                    normalizedPeriod = "today";
                    fromLocal = clientNow.Date;
                    toLocal = clientNow.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            var fromUtc = fromLocal.AddMinutes(offsetMinutes);
            var toUtc = toLocal.AddMinutes(offsetMinutes);

            var tickets = await _ticketRepository.GetTicketsByRangeAsync(fromUtc, toUtc, branchId, companyId, cancellationToken);

            var countsByHour = new Dictionary<int, int>();
            for (int h = 0; h < 24; h++)
            {
                countsByHour[h] = 0;
            }

            foreach (var ticket in tickets)
            {
                var localEntry = ticket.EntryTimeUtc.AddMinutes(-offsetMinutes);
                var hour = localEntry.Hour;
                if (hour >= 0 && hour < 24)
                {
                    countsByHour[hour]++;
                }
            }

            var hourlyData = new List<HourlyTrafficDto>();
            int peakHour = 0;
            int maxCount = 0;

            for (int h = 0; h < 24; h++)
            {
                var count = countsByHour[h];
                if (count > maxCount)
                {
                    maxCount = count;
                    peakHour = h;
                }

                hourlyData.Add(new HourlyTrafficDto
                {
                    Hour = h,
                    HourLabel = $"{h:D2}:00",
                    EntriesCount = count
                });
            }

            var totalEntries = tickets.Count;
            var activeHours = hourlyData.Count(d => d.EntriesCount > 0);
            var avgPerHour = activeHours > 0 ? Math.Round((double)totalEntries / activeHours, 1) : 0.0;

            string FormatHourRange(int h)
            {
                var start = DateTime.Today.AddHours(h).ToString("hh:mm tt");
                var end = DateTime.Today.AddHours((h + 1) % 24).ToString("hh:mm tt");
                return $"{start} - {end}";
            }

            return new PeakTrafficReportDto
            {
                Period = normalizedPeriod,
                StartDateUtc = fromUtc,
                EndDateUtc = toUtc,
                TotalEntries = totalEntries,
                PeakHour = peakHour,
                PeakHourLabel = totalEntries > 0 ? FormatHourRange(peakHour) : "Sin ingresos",
                PeakEntriesCount = maxCount,
                AveragePerHour = avgPerHour,
                HourlyData = hourlyData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular horas pico de tráfico");
            return new PeakTrafficReportDto();
        }
    }
}
