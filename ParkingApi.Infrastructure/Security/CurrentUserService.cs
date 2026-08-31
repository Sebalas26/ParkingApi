using System;
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

    public int? ParsedUserId
    {
        get
        {
            var raw = UserId;
            return int.TryParse(raw, out int id) ? id : null;
        }
    }

    public int? CompanyId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("company_id")?.Value;
            return int.TryParse(claim, out int cid) ? cid : null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return false;

            var claim = user.FindFirst("is_super_admin")?.Value;
            if (bool.TryParse(claim, out bool isSuper) && isSuper)
            {
                return true;
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrWhiteSpace(role) && (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) || role.Equals("Super Administrador", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // If user is authenticated and has no company_id claim and role_id is 1 or role is superadmin
            if (user.Identity?.IsAuthenticated == true && !CompanyId.HasValue)
            {
                var roleIdClaim = user.FindFirst("role_id")?.Value;
                if (roleIdClaim == "1") return true;
            }

            return false;
        }
    }

    public int? RoleId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("role_id")?.Value;
            return int.TryParse(claim, out int rid) ? rid : null;
        }
    }

    public string? RoleName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? GetEffectiveCompanyId(int? requestedCompanyId)
    {
        if (IsSuperAdmin)
        {
            if (requestedCompanyId.HasValue && requestedCompanyId.Value > 0)
            {
                return requestedCompanyId.Value;
            }

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Company-Id", out var headerVal))
            {
                var raw = headerVal.ToString();
                if (int.TryParse(raw, out int headerCid) && headerCid > 0)
                {
                    return headerCid;
                }
            }

            return null;
        }

        // For regular tenant users, always enforce their own CompanyId
        return CompanyId;
    }

    public bool CanAccessCompany(int targetCompanyId)
    {
        if (IsSuperAdmin) return true;
        return CompanyId.HasValue && CompanyId.Value == targetCompanyId;
    }
}

