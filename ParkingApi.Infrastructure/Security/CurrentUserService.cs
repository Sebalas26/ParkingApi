using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Sid)?.Value;
}
