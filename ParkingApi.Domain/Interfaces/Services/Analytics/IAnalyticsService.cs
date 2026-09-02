using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Analytics;

namespace ParkingApi.Domain.Interfaces.Services.Analytics;

public interface IAnalyticsService
{
    Task<FinancialSummaryDto> GetDailySummaryAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<OccupancyStatsDto> GetOccupancyStatsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
}
