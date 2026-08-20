using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Interfaces.Services.Modules;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ModuleController : ControllerBase
{
    private readonly ILogger<ModuleController> _logger;
    private readonly IModuleService _moduleService;

    public ModuleController(ILogger<ModuleController> logger, IModuleService moduleService)
    {
        _logger = logger;
        _moduleService = moduleService;
    }

    [HttpGet("GetModules")]
    [HttpGet]
    public async Task<IActionResult> GetModules(CancellationToken cancellation)
    {
        try
        {
            var modules = await _moduleService.GetModules(cancellation);
            return Ok(modules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los módulos");
            return StatusCode(500, new { message = "Error interno al consultar módulos." });
        }
    }

    [HttpGet("GetModule/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetModuleById(int id, CancellationToken cancellation)
    {
        try
        {
            var module = await _moduleService.GetModuleById(id, cancellation);
            if (module == null)
            {
                return NotFound(new { message = "Módulo no encontrado." });
            }
            return Ok(module);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el módulo con id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar módulo." });
        }
    }

    [HttpPost("SaveModule")]
    [HttpPost]
    public async Task<IActionResult> SaveModule([FromBody] GetModuleDto value, CancellationToken cancellation)
    {
        try
        {
            var result = await _moduleService.SaveOrEditModule(value, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar el módulo");
            return StatusCode(500, new { message = "Error interno al guardar módulo." });
        }
    }
}
