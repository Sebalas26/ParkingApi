using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;

    public SyncController(ISyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpGet("bootstrap")]
    public async Task<IActionResult> GetBootstrap(CancellationToken cancellationToken)
    {
        var data = await _syncService.GetBootstrapDataAsync(cancellationToken);
        return Ok(data);
    }
}

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

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        return Ok(new { status = "Healthy", timestamp = System.DateTime.UtcNow });
    }
}
