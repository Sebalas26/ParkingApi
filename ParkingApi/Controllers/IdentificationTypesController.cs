using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services.IdentificationTypes;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class IdentificationTypesController : ControllerBase
{
    private readonly ILogger<IdentificationTypesController> _logger;
    private readonly IIdentificationTypeService _service;

    public IdentificationTypesController(ILogger<IdentificationTypesController> logger, IIdentificationTypeService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("GetIdentificationTypes")]
    [HttpGet]
    public async Task<IActionResult> GetIdentificationTypes(CancellationToken cancellation)
    {
        try
        {
            var types = await _service.GetAllAsync(cancellation);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de identificación");
            return StatusCode(500, new { message = "Error interno al consultar tipos de identificación." });
        }
    }

    [HttpGet("GetIdentificationTypesActive")]
    public async Task<IActionResult> GetIdentificationTypesActive(CancellationToken cancellation)
    {
        try
        {
            var types = await _service.GetAllActiveAsync(cancellation);
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de identificación activos");
            return StatusCode(500, new { message = "Error interno al consultar tipos de identificación activos." });
        }
    }

    [HttpGet("GetIdentificationType/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetIdentificationTypeById(int id, CancellationToken cancellation)
    {
        try
        {
            var type = await _service.GetByIdAsync(id, cancellation);
            if (type == null)
            {
                return NotFound(new { message = "Tipo de identificación no encontrado." });
            }
            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipo de identificación con id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar tipo de identificación." });
        }
    }

    [HttpPost("SaveOrEditIdentificationTypes")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditIdentificationTypes([FromBody] GetIdentificationTypeDto saveData, CancellationToken cancellation)
    {
        try
        {
            var result = await _service.CreateOrEditIdentificationType(saveData, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar tipo de identificación");
            return StatusCode(500, new { message = "Error interno al guardar tipo de identificación." });
        }
    }
}
