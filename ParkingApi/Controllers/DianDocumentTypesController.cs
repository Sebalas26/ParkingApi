using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Billing;
using ParkingApi.Domain.Interfaces.Services.Billing;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DianDocumentTypesController : ControllerBase
{
    private readonly IDianDocumentTypeService _service;
    private readonly ILogger<DianDocumentTypesController> _logger;

    public DianDocumentTypesController(
        IDianDocumentTypeService service,
        ILogger<DianDocumentTypesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetAllAsync(cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar tipos de documentos DIAN");
            return StatusCode(500, new { message = "Error al obtener tipos de documentos DIAN." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        try
        {
            var list = await _service.GetActiveAsync(cancellationToken);
            return Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar tipos de documentos DIAN activos");
            return StatusCode(500, new { message = "Error al obtener tipos de documentos DIAN activos." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _service.GetByIdAsync(id, cancellationToken);
            if (item == null)
                return NotFound(new { message = $"Tipo de documento DIAN con ID {id} no encontrado." });

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tipo de documento DIAN {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDianDocumentTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int? responsibleUserId = null;
            var sid = User?.FindFirst(ClaimTypes.Sid)?.Value;
            if (int.TryParse(sid, out int uid)) responsibleUserId = uid;

            var created = await _service.CreateAsync(dto, responsibleUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de documento DIAN");
            return StatusCode(500, new { message = "Error interno al crear el tipo de documento DIAN." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDianDocumentTypeDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int? responsibleUserId = null;
            var sid = User?.FindFirst(ClaimTypes.Sid)?.Value;
            if (int.TryParse(sid, out int uid)) responsibleUserId = uid;

            var updated = await _service.UpdateAsync(id, dto, responsibleUserId, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de documento DIAN {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar el tipo de documento DIAN." });
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
    {
        try
        {
            int? responsibleUserId = null;
            var sid = User?.FindFirst(ClaimTypes.Sid)?.Value;
            if (int.TryParse(sid, out int uid)) responsibleUserId = uid;

            var newStatus = await _service.ToggleStatusAsync(id, responsibleUserId, cancellationToken);
            return Ok(new { isActive = newStatus, message = newStatus ? "Tipo de documento activado" : "Tipo de documento inactivado" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de tipo de documento DIAN {Id}", id);
            return StatusCode(500, new { message = "Error interno al cambiar el estado." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return Ok(new { message = "Tipo de documento DIAN eliminado correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar tipo de documento DIAN {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar el tipo de documento." });
        }
    }
}
