using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.ParkingLots;
using ParkingApi.Domain.Interfaces.Services.ParkingLots;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ParkingLotsController : ControllerBase
{
    private readonly ILogger<ParkingLotsController> _logger;
    private readonly IParkingLotService _parkingLotService;

    public ParkingLotsController(ILogger<ParkingLotsController> logger, IParkingLotService parkingLotService)
    {
        _logger = logger;
        _parkingLotService = parkingLotService;
    }

    [HttpGet("GetParkingLots")]
    [HttpGet]
    public async Task<IActionResult> GetParkingLots(CancellationToken cancellation)
    {
        try
        {
            var result = await _parkingLotService.GetParkingLotsAsync(cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar parqueaderos");
            return StatusCode(500, new { message = "Error interno al consultar parqueaderos." });
        }
    }

    [HttpGet("GetParkingLot/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetParkingLotById(int id, CancellationToken cancellation)
    {
        try
        {
            var result = await _parkingLotService.GetParkingLotByIdAsync(id, cancellation);
            if (result == null)
            {
                return NotFound(new { message = "Parqueadero no encontrado." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar parqueadero {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar el parqueadero." });
        }
    }

    [HttpPost("SaveOrEdit")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEdit([FromBody] SaveParkingLotDto dto, CancellationToken cancellation)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "El nombre del parqueadero es obligatorio." });
            }

            var result = await _parkingLotService.SaveOrEditParkingLotAsync(dto, cancellation);
            if (result == null)
            {
                return BadRequest(new { message = "No se pudo guardar o editar el parqueadero." });
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar parqueadero");
            return StatusCode(500, new { message = "Error interno al procesar parqueadero." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellation)
    {
        try
        {
            var success = await _parkingLotService.DeactivateParkingLotAsync(id, cancellation);
            return success ? Ok(new { message = "Parqueadero desactivado correctamente." }) : NotFound(new { message = "Parqueadero no encontrado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar parqueadero {Id}", id);
            return StatusCode(500, new { message = "Error interno al desactivar parqueadero." });
        }
    }
}
