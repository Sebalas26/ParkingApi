using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

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
}

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Store store, CancellationToken cancellationToken)
    {
        var created = await _storeService.CreateAsync(store, cancellationToken);
        return Ok(created);
    }
}

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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CommercialAgreement agreement, CancellationToken cancellationToken)
    {
        var created = await _agreementService.CreateAsync(agreement, cancellationToken);
        return Ok(created);
    }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateUserAsync(dto, cancellationToken);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var success = await _userService.DeactivateUserAsync(id, cancellationToken);
        return success ? Ok(new { message = "Usuario desactivado." }) : NotFound();
    }
}
