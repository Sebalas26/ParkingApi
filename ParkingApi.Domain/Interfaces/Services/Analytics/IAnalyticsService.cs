using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Analytics;

namespace ParkingApi.Domain.Interfaces.Services.Analytics;

public interface IAnalyticsService
{
    Task<FinancialSummaryDto> GetDailySummaryAsync(CancellationToken cancellationToken = default);
    Task<OccupancyStatsDto> GetOccupancyStatsAsync(CancellationToken cancellationToken = default);
}
