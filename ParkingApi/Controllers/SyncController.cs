using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Services.Sync;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    [HttpGet("bootstrap")]
    public async Task<IActionResult> GetBootstrap([FromQuery] int? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _syncService.GetBootstrapDataAsync(branchId, cancellationToken);
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener datos de bootstrap de sincronización para sede {BranchId}", branchId);
            return StatusCode(500, new { message = "Error interno al sincronizar datos." });
        }
    }
}
