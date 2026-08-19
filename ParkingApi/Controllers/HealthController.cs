using System;
using Microsoft.AspNetCore.Mvc;

namespace ParkingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ParkFlow Central REST API",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
