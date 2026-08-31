using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Interfaces.Services.Companies;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(ICompanyService companyService, ILogger<CompaniesController> logger)
    {
        _companyService = companyService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _companyService.GetAllCompaniesAsync(cancellationToken);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar empresas");
            return StatusCode(500, new { message = "Error al obtener empresas." });
        }
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        try
        {
            var companies = await _companyService.GetActiveCompaniesAsync(cancellationToken);
            return Ok(companies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar empresas activas");
            return StatusCode(500, new { message = "Error al obtener empresas activas." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var company = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
            if (company == null)
            {
                return NotFound(new { message = $"Empresa con ID {id} no encontrada." });
            }
            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Nit))
            {
                return BadRequest(new { message = "El nombre y el NIT de la empresa son requeridos." });
            }

            if (string.IsNullOrWhiteSpace(dto.AdminUsername) || string.IsNullOrWhiteSpace(dto.AdminPassword))
            {
                return BadRequest(new { message = "El usuario y contraseña del administrador inicial son requeridos." });
            }

            int? responsibleUserId = null;
            var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (int.TryParse(sid, out int uid)) responsibleUserId = uid;

            var created = await _companyService.CreateCompanyAsync(dto, responsibleUserId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empresa {CompanyName}", dto.Name);
            var innerMsg = ex.InnerException?.Message;
            var detailedMsg = !string.IsNullOrWhiteSpace(innerMsg) ? innerMsg : ex.Message;
            return StatusCode(500, new { message = $"Error interno al crear empresa: {detailedMsg}" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyDto dto, CancellationToken cancellationToken)
    {
        try
        {
            int? responsibleUserId = null;
            var sid = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (int.TryParse(sid, out int uid)) responsibleUserId = uid;

            var updated = await _companyService.UpdateCompanyAsync(id, dto, responsibleUserId, cancellationToken);
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
            _logger.LogError(ex, "Error al actualizar empresa {Id}", id);
            var innerMsg = ex.InnerException?.Message;
            var detailedMsg = !string.IsNullOrWhiteSpace(innerMsg) ? innerMsg : ex.Message;
            return StatusCode(500, new { message = $"Error interno al actualizar empresa: {detailedMsg}" });
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _companyService.ToggleCompanyStatusAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = $"Empresa con ID {id} no encontrada." });
            }
            return Ok(new { message = "Estado de empresa actualizado exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al alternar estado de empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno al alternar estado." });
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _companyService.DeleteCompanyAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new { message = $"Empresa con ID {id} no encontrada." });
            }
            return Ok(new { message = "Empresa y todos sus datos asociados eliminados exitosamente." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno al eliminar la empresa." });
        }
    }
}
