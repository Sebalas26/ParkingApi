using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Branches;
using ParkingApi.Domain.Interfaces.Services.Branches;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(
        IBranchService branchService, 
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser,
        ILogger<BranchesController> logger)
    {
        _branchService = branchService;
        _realtimeNotifier = realtimeNotifier;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);

        if (effectiveCompanyId.HasValue && effectiveCompanyId.Value > 0)
        {
            var companyBranches = await _branchService.GetBranchesByCompanyIdAsync(effectiveCompanyId.Value, cancellationToken);
            return Ok(companyBranches);
        }

        if (!_currentUser.IsSuperAdmin)
        {
            return Ok(new List<BranchDto>());
        }

        var branches = await _branchService.GetAllAsync(cancellationToken);
        return Ok(branches);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive([FromQuery] int? companyId, CancellationToken cancellationToken)
    {
        var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
        var branches = await _branchService.GetActiveAsync(effectiveCompanyId, cancellationToken);
        return Ok(branches);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetByIdAsync(id, cancellationToken);
        if (branch == null) return NotFound(new { message = $"Sede con Id {id} no encontrada." });
        return Ok(branch);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId, CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetBranchesByUserIdAsync(userId, cancellationToken);
        return Ok(branches);
    }

    [HttpGet("company/{companyId:int}")]
    public async Task<IActionResult> GetByCompany(int companyId, CancellationToken cancellationToken)
    {
        var branches = await _branchService.GetBranchesByCompanyIdAsync(companyId, cancellationToken);
        return Ok(branches);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto?.Name))
        {
            return BadRequest(new { message = "El nombre de la sede es obligatorio." });
        }

        try
        {
            if (!dto.CompanyId.HasValue || dto.CompanyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    dto.CompanyId = cid;
                }
            }

            if (!dto.CompanyId.HasValue || dto.CompanyId.Value <= 0)
            {
                return BadRequest(new { message = "Se requiere identificar la empresa (CompanyId) para registrar la sede." });
            }

            var created = await _branchService.CreateAsync(dto, cancellationToken);
            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("BranchCreated", "Nueva Sede Creada", $"Se ha registrado la sede '{created.Name}'.", cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validación al crear sede.");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sede.");
            var innerMsg = ex.InnerException?.Message;
            var detailedMsg = !string.IsNullOrWhiteSpace(innerMsg) ? innerMsg : ex.Message;
            return StatusCode(500, new { message = $"Error al crear la sede: {detailedMsg}" });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBranchDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _branchService.UpdateAsync(id, dto, cancellationToken);
            if (updated == null) return NotFound(new { message = $"Sede con Id {id} no encontrada." });

            _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(id, "Sede Actualizada", $"Se actualizaron los datos y configuración de la sede '{updated.Name}'.", "BranchConfigChanged", cancellationToken);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sede.");
            return StatusCode(500, new { message = "Error interno al actualizar la sede." });
        }
    }

    [HttpPost("assign-user")]
    public async Task<IActionResult> AssignUser([FromBody] AssignUserBranchDto dto, CancellationToken cancellationToken)
    {
        var success = await _branchService.AssignUserAsync(dto, cancellationToken);
        if (success) return Ok(new { message = "Usuario asignado a la sede correctamente." });
        return BadRequest(new { message = "No se pudo asignar el usuario a la sede." });
    }

    [HttpPost("unassign-user")]
    public async Task<IActionResult> UnassignUser([FromBody] AssignUserBranchDto dto, CancellationToken cancellationToken)
    {
        var success = await _branchService.UnassignUserAsync(dto.UserId, dto.BranchId, cancellationToken);
        if (success) return Ok(new { message = "Usuario desasignado de la sede correctamente." });
        return BadRequest(new { message = "No se pudo desasignar el usuario de la sede." });
    }

    [HttpGet("{id:int}/payment-methods")]
    public async Task<IActionResult> GetPaymentMethods(int id, CancellationToken cancellationToken)
    {
        var methods = await _branchService.GetPaymentMethodsAsync(id, cancellationToken);
        return Ok(methods);
    }

    [HttpPost("configure-payment-methods")]
    public async Task<IActionResult> ConfigurePaymentMethods([FromBody] ConfigureBranchPaymentMethodsDto dto, CancellationToken cancellationToken)
    {
        var success = await _branchService.ConfigurePaymentMethodsAsync(dto, cancellationToken);
        if (success)
        {
            _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId, "Medios de Pago Actualizados", "Se actualizaron los medios de pago disponibles para la sede.", "PaymentMethodsChanged", cancellationToken);
            return Ok(new { message = "Medios de pago configurados correctamente para la sede." });
        }
        return BadRequest(new { message = "No se pudieron configurar los medios de pago para la sede." });
    }

    [HttpGet("{id:int}/agreements")]
    public async Task<IActionResult> GetAgreements(int id, CancellationToken cancellationToken)
    {
        var agreements = await _branchService.GetAgreementsAsync(id, cancellationToken);
        return Ok(agreements);
    }

    [HttpPost("configure-agreements")]
    public async Task<IActionResult> ConfigureAgreements([FromBody] ConfigureBranchAgreementsDto dto, CancellationToken cancellationToken)
    {
        var success = await _branchService.ConfigureAgreementsAsync(dto, cancellationToken);
        if (success)
        {
            _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId, "Convenios Actualizados", "Se actualizaron los convenios comerciales habilitados para la sede.", "AgreementsChanged", cancellationToken);
            return Ok(new { message = "Convenios comerciales configurados correctamente para la sede." });
        }
        return BadRequest(new { message = "No se pudieron configurar los convenios para la sede." });
    }

    [HttpGet("{id:int}/resolutions")]
    public async Task<IActionResult> GetResolutions(int id, CancellationToken cancellationToken)
    {
        var resolutions = await _branchService.GetResolutionsAsync(id, cancellationToken);
        return Ok(resolutions);
    }

    [HttpPost("configure-resolutions")]
    public async Task<IActionResult> ConfigureResolutions([FromBody] ConfigureBranchResolutionsDto dto, CancellationToken cancellationToken)
    {
        var success = await _branchService.ConfigureResolutionsAsync(dto, cancellationToken);
        if (success)
        {
            _ = _realtimeNotifier.NotifyBranchConfigChangedAsync(dto.BranchId, "Resoluciones Actualizadas", "Se actualizaron las resoluciones de facturación asignadas a la sede.", "ResolutionsChanged", cancellationToken);
            return Ok(new { message = "Resoluciones de facturación configuradas correctamente para la sede." });
        }
        return BadRequest(new { message = "No se pudieron configurar las resoluciones para la sede." });
    }

    [HttpGet("{id:int}/users")]
    public async Task<IActionResult> GetBranchUsers(int id, CancellationToken cancellationToken)
    {
        var users = await _branchService.GetUsersByBranchIdAsync(id, cancellationToken);
        return Ok(users);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await _branchService.GetByIdAsync(id, cancellationToken);
            if (branch == null) return NotFound(new { message = $"Sede con Id {id} no encontrada." });

            var success = await _branchService.DeleteAsync(id, cancellationToken);
            if (success)
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("BranchDeleted", "Sede Eliminada", $"Se ha eliminado la sede '{branch.Name}' de la empresa.", cancellationToken);
                return Ok(new { message = "Sede eliminada exitosamente." });
            }
            return BadRequest(new { message = "No se pudo eliminar la sede." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la sede con Id {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar la sede." });
        }
    }
}
