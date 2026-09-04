using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.PaymentMethods;
using ParkingApi.Domain.Interfaces.Services.PaymentMethods;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentMethodController : ControllerBase
{
    private readonly ILogger<PaymentMethodController> _logger;
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService _realtimeNotifier;
    private readonly ParkingApi.Domain.Interfaces.Services.ICurrentUserService _currentUser;

    public PaymentMethodController(
        ILogger<PaymentMethodController> logger, 
        IPaymentMethodService paymentMethodService,
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier,
        ParkingApi.Domain.Interfaces.Services.ICurrentUserService currentUser)
    {
        _logger = logger;
        _paymentMethodService = paymentMethodService;
        _realtimeNotifier = realtimeNotifier;
        _currentUser = currentUser;
    }

    [HttpGet("GetPaymentMethods")]
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] int? companyId, CancellationToken cancellation)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var methods = await _paymentMethodService.GetAllAsync(effectiveCompanyId, cancellation);
            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los métodos de pago");
            return StatusCode(500, new { message = "Error interno al consultar métodos de pago." });
        }
    }

    [HttpGet("GetPaymentMethodsActive")]
    public async Task<IActionResult> GetPaymentMethodsActive([FromQuery] int? companyId, CancellationToken cancellation)
    {
        try
        {
            var effectiveCompanyId = _currentUser.GetEffectiveCompanyId(companyId);
            var methods = await _paymentMethodService.GetAllActiveAsync(effectiveCompanyId, cancellation);
            return Ok(methods);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los métodos de pago activos");
            return StatusCode(500, new { message = "Error interno al consultar métodos de pago activos." });
        }
    }

    [HttpGet("GetPaymentMethod/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentMethodById(int id, CancellationToken cancellation)
    {
        try
        {
            var method = await _paymentMethodService.GetByIdAsync(id, cancellation);
            if (method == null)
            {
                return NotFound(new { message = "Método de pago no encontrado." });
            }
            return Ok(method);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el método de pago con ID {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar método de pago." });
        }
    }

    [HttpPost("CreateOrEditPaymentMethod")]
    [HttpPost]
    public async Task<IActionResult> CreateOrEditPaymentMethod([FromBody] GetPaymentMethodDto value, CancellationToken cancellation)
    {
        try
        {
            if (!_currentUser.IsSuperAdmin)
            {
                value.CompanyId = _currentUser.CompanyId;
            }
            else if (!value.CompanyId.HasValue || value.CompanyId <= 0)
            {
                value.CompanyId = _currentUser.CompanyId;
            }

            var result = await _paymentMethodService.CreateOrEditPaymentMethod(value, cancellation);
            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("PaymentMethodsChanged", "Medio de Pago Modificado", $"Se actualizó el catálogo de medios de pago ('{value.Name}').", cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear o editar el método de pago");
            return StatusCode(500, new { message = "Error interno al guardar método de pago." });
        }
    }

    [HttpDelete("DeletePaymentMethod/{id}")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePaymentMethod(int id, CancellationToken cancellation)
    {
        try
        {
            var deleted = await _paymentMethodService.DeleteAsync(id, cancellation);
            if (!deleted)
            {
                return NotFound(new { success = false, message = "Método de pago no encontrado o no se pudo eliminar." });
            }

            _ = _realtimeNotifier.NotifyGlobalConfigChangedAsync("PaymentMethodsChanged", "Medio de Pago Eliminado", $"Se eliminó el medio de pago con ID #{id}.", cancellation);
            return Ok(new { success = true, message = "Método de pago eliminado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el método de pago con ID {Id}", id);
            return StatusCode(500, new { success = false, message = "Error interno al eliminar método de pago." });
        }
    }
}
