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

    public PaymentMethodController(
        ILogger<PaymentMethodController> logger, 
        IPaymentMethodService paymentMethodService,
        ParkingApi.Domain.Interfaces.Services.Realtime.IRealtimeNotificationService realtimeNotifier)
    {
        _logger = logger;
        _paymentMethodService = paymentMethodService;
        _realtimeNotifier = realtimeNotifier;
    }

    [HttpGet("GetPaymentMethods")]
    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods([FromQuery] int? companyId, CancellationToken cancellation)
    {
        try
        {
            if (!companyId.HasValue || companyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    companyId = cid;
                }
            }

            var methods = await _paymentMethodService.GetAllAsync(companyId, cancellation);
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
            if (!companyId.HasValue || companyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    companyId = cid;
                }
            }

            var methods = await _paymentMethodService.GetAllActiveAsync(companyId, cancellation);
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
            if (!value.CompanyId.HasValue || value.CompanyId.Value <= 0)
            {
                var companyClaim = User.FindFirst("company_id")?.Value;
                if (int.TryParse(companyClaim, out int cid))
                {
                    value.CompanyId = cid;
                }
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
}
