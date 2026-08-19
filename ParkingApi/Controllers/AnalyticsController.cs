using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Interfaces.Services.Analytics;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary(CancellationToken cancellationToken)
    {
        var summary = await _analyticsService.GetDailySummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy(CancellationToken cancellationToken)
    {
        var occupancy = await _analyticsService.GetOccupancyStatsAsync(cancellationToken);
        return Ok(occupancy);
    }
}
