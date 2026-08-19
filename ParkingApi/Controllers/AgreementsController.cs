using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Agreements;
using ParkingApi.Domain.Interfaces.Services.Agreements;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgreementsController : ControllerBase
{
    private readonly IAgreementService _agreementService;

    public AgreementsController(IAgreementService agreementService)
    {
        _agreementService = agreementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var agreements = await _agreementService.GetAllAsync(cancellationToken);
        return Ok(agreements);
    }

    [HttpGet("by-store/{storeId}")]
    public async Task<IActionResult> GetByStore(Guid storeId, CancellationToken cancellationToken)
    {
        var agreements = await _agreementService.GetByStoreIdAsync(storeId, cancellationToken);
        return Ok(agreements);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await _agreementService.GetByIdAsync(id, cancellationToken);
        if (agreement == null) return NotFound(new { message = "Convenio no encontrado." });
        return Ok(agreement);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAgreementDto dto, CancellationToken cancellationToken)
    {
        var agreement = await _agreementService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = agreement.AgreementId }, agreement);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAgreementDto dto, CancellationToken cancellationToken)
    {
        var agreement = await _agreementService.UpdateAsync(id, dto, cancellationToken);
        if (agreement == null) return NotFound(new { message = "Convenio no encontrado." });
        return Ok(agreement);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var success = await _agreementService.DeleteAsync(id, cancellationToken);
        return success ? Ok(new { message = "Convenio desactivado exitosamente." }) : NotFound();
    }
}
