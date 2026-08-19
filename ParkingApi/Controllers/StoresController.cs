using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Stores;
using ParkingApi.Domain.Interfaces.Services.Stores;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var stores = await _storeService.GetAllAsync(cancellationToken);
        return Ok(stores);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var store = await _storeService.GetByIdAsync(id, cancellationToken);
        if (store == null) return NotFound(new { message = "Comercio no encontrado." });
        return Ok(store);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreDto dto, CancellationToken cancellationToken)
    {
        var store = await _storeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = store.StoreId }, store);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreDto dto, CancellationToken cancellationToken)
    {
        var store = await _storeService.UpdateAsync(id, dto, cancellationToken);
        if (store == null) return NotFound(new { message = "Comercio no encontrado." });
        return Ok(store);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await _storeService.DeleteAsync(id, cancellationToken);
        return success ? Ok(new { message = "Comercio desactivado exitosamente." }) : NotFound();
    }
}
