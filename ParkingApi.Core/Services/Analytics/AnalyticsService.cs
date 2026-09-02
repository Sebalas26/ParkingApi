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
}
