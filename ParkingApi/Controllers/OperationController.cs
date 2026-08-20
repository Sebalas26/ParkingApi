using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Interfaces.Services.Operations;

namespace ParkingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OperationController : ControllerBase
{
    private readonly ILogger<OperationController> _logger;
    private readonly IOperationService _operationService;

    public OperationController(ILogger<OperationController> logger, IOperationService operationService)
    {
        _logger = logger;
        _operationService = operationService;
    }

    [HttpGet("GetOperations")]
    [HttpGet]
    public async Task<IActionResult> GetOperations(CancellationToken cancellation)
    {
        try
        {
            var operations = await _operationService.GetOperations(cancellation);
            return Ok(operations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las operaciones");
            return StatusCode(500, new { message = "Error interno al consultar operaciones." });
        }
    }

    [HttpGet("GetOperation/{id}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOperationById(int id, CancellationToken cancellation)
    {
        try
        {
            var operation = await _operationService.GetOperationsById(id, cancellation);
            if (operation == null)
            {
                return NotFound(new { message = "Operación no encontrada." });
            }
            return Ok(operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la operación con id {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar operación." });
        }
    }

    [HttpPost("SaveOrEditOperation")]
    [HttpPost]
    public async Task<IActionResult> SaveOrEditOperation([FromBody] GetOperationDto getOperation, CancellationToken cancellation)
    {
        try
        {
            var result = await _operationService.SaveOrEditOperation(getOperation, cancellation);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar o editar la operación");
            return StatusCode(500, new { message = "Error interno al guardar operación." });
        }
    }
}
