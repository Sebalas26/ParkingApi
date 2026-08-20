using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Models;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IStoreService storeService, ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var stores = await _storeService.GetAllAsync(cancellationToken);
            return Ok(stores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercios");
            return StatusCode(500, new { message = "Error interno al consultar comercios." });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var store = await _storeService.GetByIdAsync(id, cancellationToken);
            if (store == null)
            {
                return NotFound(new { message = "Comercio no encontrado." });
            }
            return Ok(store);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercio {Id}", id);
            return StatusCode(500, new { message = "Error interno al consultar comercio." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Store store, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _storeService.CreateAsync(store, cancellationToken);
            return Ok(created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear comercio");
            return StatusCode(500, new { message = "Error interno al crear comercio." });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Store store, CancellationToken cancellationToken)
    {
        try
        {
            store.StoreId = id;
            var success = await _storeService.UpdateAsync(store, cancellationToken);
            return success ? Ok(store) : NotFound(new { message = "Comercio no encontrado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comercio {Id}", id);
            return StatusCode(500, new { message = "Error interno al actualizar comercio." });
        }
    }
}
