using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Models;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgreementsController : ControllerBase
{
    private readonly ICommercialAgreementService _agreementService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ILogger<AgreementsController> _logger;

    public AgreementsController(
        ICommercialAgreementService agreementService, 
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ILogger<AgreementsController> logger)
    {
        _agreementService = agreementService;
        _realtimeNotifier = realtimeNotifier;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var agreements = await _agreementService.GetAllAsync(cancellationToken);
            return Ok(agreements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenios");
            return StatusCode(500, new { message = "Error interno al consultar convenios." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var agreement = await _agreementService.GetByIdAsync(id, cancellationToken);
            if (agreement == null)
            {
                return NotFound(new { message = "Convenio no encontrado." });
            }
            return Ok(agreement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenio {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar convenio." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommercialAgreement agreement, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _agreementService.CreateAsync(agreement, cancellationToken);
            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("AgreementsChanged", "Convenio Comercial Creado", $"Se ha registrado el convenio '{created.Name}'.", cancellationToken);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear convenio");
            return StatusCode(500, new { message = "Error interno al crear convenio." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CommercialAgreement agreement, CancellationToken cancellationToken)
    {
        try
        {
            agreement.AgreementId = id;
            var success = await _agreementService.UpdateAsync(agreement, cancellationToken);
            if (success)
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("AgreementsChanged", "Convenio Comercial Actualizado", $"Se actualizaron los parámetros del convenio '{agreement.Name}'.", cancellationToken);
                return Ok(agreement);
            }
            return NotFound(new { message = "Convenio no encontrado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar convenio {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar convenio." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var agreement = await _agreementService.GetByIdAsync(id, cancellationToken);
            if (agreement == null)
            {
                return NotFound(new { message = "Convenio no encontrado." });
            }
            agreement.IsActive = false;
            var success = await _agreementService.UpdateAsync(agreement, cancellationToken);
            if (success)
            {
                _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("AgreementsChanged", "Convenio Inactivado", $"El convenio '{agreement.Name}' ha sido inactivado.", cancellationToken);
                return Ok(new { message = "Convenio inactivado correctamente." });
            }
            return StatusCode(500, new { message = "Error al inactivar convenio." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inactivar convenio {Id}", id);
            return StatusCode(500, new { message = "Error interno al inactivar convenio." });
        }
    }
}
