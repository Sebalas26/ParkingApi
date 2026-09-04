using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Billing;
using ParkingApi.Domain.Interfaces.Services.Billing;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResolutionsController : ControllerBase
{
    private readonly IBillingResolutionService _resolutionService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;
    private readonly ILogger<ResolutionsController> _logger;

    public ResolutionsController(
        IBillingResolutionService resolutionService, 
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser,
        ILogger<ResolutionsController> logger)
    {
        _resolutionService = resolutionService;
        _realtimeNotifier = realtimeNotifier;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId, [FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var list = await _resolutionService.GetAllAsync(branchId, effectiveCompanyId, cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resoluciones de facturación");
            return StatusCode(500, new { message = "Error interno al consultar resoluciones." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] int? branchId, [FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var list = await _resolutionService.GetActiveAsync(branchId, effectiveCompanyId, cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resoluciones activas");
            return StatusCode(500, new { message = "Error interno al consultar resoluciones activas." });
        }
    }

    [HttpGet("by-branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(int branchId, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _resolutionService.GetActiveAsync(branchId, _currentUser.CompanyId, cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resoluciones para la sede {BranchId}", branchId);
            return StatusCode(500, new { message = "Error interno al consultar resoluciones por sede." });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var resolution = await _resolutionService.GetByIdAsync(id, cancellationToken);
            if (resolution == null)
            {
                return NotFound(new { message = "Resolución no encontrada." });
            }
            return Ok(resolution);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resolución {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar resolución." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveBillingResolutionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Prefix) || string.IsNullOrWhiteSpace(dto.ResolutionNumber))
            {
                return BadRequest(new { message = "Nombre, prefijo y número de resolución son obligatorios." });
            }

            if (dto.FromNumber > dto.ToNumber)
            {
                return BadRequest(new { message = "El rango inicial ('Desde') no puede ser mayor al rango final ('Hasta')." });
            }

            if (!_currentUser.IsSuperAdmin)
            {
                dto.CompanyId = _currentUser.CompanyId;
            }
            else if (!dto.CompanyId.HasValue || dto.CompanyId <= 0)
            {
                dto.CompanyId = _currentUser.CompanyId;
            }

            var created = await _resolutionService.CreateAsync(dto, cancellationToken);

            var title = "Resolución de Facturación Creada";
            var msg = $"Se registró la resolución '{dto.Name}' (Prefijo {dto.Prefix}).";
            if (dto.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId.Value, title, msg, "ResolutionsChanged", cancellationToken);
            }

            return CreatedAtAction(nameof(GetById), new { id = created.ResolutionId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear resolución de facturación");
            return StatusCode(500, new { message = "Error interno al registrar resolución." });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveBillingResolutionDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Prefix) || string.IsNullOrWhiteSpace(dto.ResolutionNumber))
            {
                return BadRequest(new { message = "Nombre, prefijo y número de resolución son obligatorios." });
            }

            if (dto.FromNumber > dto.ToNumber)
            {
                return BadRequest(new { message = "El rango inicial ('Desde') no puede ser mayor al rango final ('Hasta')." });
            }

            var updated = await _resolutionService.UpdateAsync(id, dto, cancellationToken);
            if (updated == null)
            {
                return NotFound(new { message = "Resolución no encontrada para actualizar." });
            }

            var title = "Resolución de Facturación Actualizada";
            var msg = $"Se actualizaron los parámetros de la resolución '{dto.Name}'.";
            if (dto.BranchId.HasValue)
            {
                _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId.Value, title, msg, "ResolutionsChanged", cancellationToken);
            }

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar resolución {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar resolución." });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _resolutionService.GetByIdAsync(id, cancellationToken);
            var success = await _resolutionService.DeactivateAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = "Resolución no encontrada o no pudo desactivarse." });
            }

            if (existing?.BranchId.HasValue == true)
            {
                _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(existing.BranchId.Value, "Resolución Inactivada", "Una resolución de facturación de la sede fue inactivada.", "ResolutionsChanged", cancellationToken);
            }

            return Ok(new { message = "Resolución desactivada exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar resolución {Id}", id);
            return StatusCode(500, new { message = "Error interno al desactivar resolución." });
        }
    }
}
