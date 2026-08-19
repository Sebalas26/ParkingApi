using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Services.Sync;

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

    [HttpPost("batch")]
    public async Task<IActionResult> ProcessBatch([FromBody] PendingSyncBatchDto batch, CancellationToken cancellationToken)
    {
        var result = await _syncService.ProcessPendingBatchAsync(batch, cancellationToken);
        return Ok(result);
    }
}
