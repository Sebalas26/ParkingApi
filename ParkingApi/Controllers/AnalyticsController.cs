using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Services.Analytics;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailySummary(
        [FromQuery] string? period,
        [FromQuery] int? branchId,
        [FromQuery] int? companyId,
        [FromQuery] int offsetMinutes = 300,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var summary = await _analyticsService.GetDailySummaryAsync(period, branchId, effectiveCompanyId, offsetMinutes, cancellationToken);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen diario para sede {BranchId}, empresa {CompanyId}", branchId, companyId);
            return StatusCode(500, new { message = "Error interno al generar resumen diario." });
        }
    }

    [HttpGet("occupancy")]
    public async Task<IActionResult> GetOccupancy([FromQuery] int? branchId, [FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var occupancy = await _analyticsService.GetOccupancyStatsAsync(branchId, effectiveCompanyId, cancellationToken);
            return Ok(occupancy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ocupación para sede {BranchId}, empresa {CompanyId}", branchId, companyId);
            return StatusCode(500, new { message = "Error interno al consultar ocupación." });
        }
    }

    [HttpGet("peak-traffic")]
    public async Task<IActionResult> GetPeakTraffic(
        [FromQuery] string? period,
        [FromQuery] int? branchId,
        [FromQuery] int? companyId,
        [FromQuery] int offsetMinutes = 300,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var report = await _analyticsService.GetPeakTrafficAsync(period, branchId, effectiveCompanyId, offsetMinutes, cancellationToken);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de horas pico de tráfico");
            return StatusCode(500, new { message = "Error interno al calcular horas pico de tráfico." });
        }
    }
}
