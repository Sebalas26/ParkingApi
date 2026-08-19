using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Rates;
using ParkingApi.Domain.Interfaces.Services.Rates;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehicleRatesController : ControllerBase
{
    private readonly IVehicleRateService _rateService;

    public VehicleRatesController(IVehicleRateService rateService)
    {
        _rateService = rateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var rates = await _rateService.GetAllRatesAsync(cancellationToken);
        return Ok(rates);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var rate = await _rateService.GetByIdAsync(id, cancellationToken);
        if (rate == null) return NotFound(new { message = "Tarifa no encontrada." });
        return Ok(rate);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRateDto dto, CancellationToken cancellationToken)
    {
        var rate = await _rateService.CreateRateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = rate.RateId }, rate);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRateDto dto, CancellationToken cancellationToken)
    {
        var rate = await _rateService.UpdateRateAsync(id, dto, cancellationToken);
        if (rate == null) return NotFound(new { message = "Tarifa no encontrada." });
        return Ok(rate);
    }
}
