using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Filters;

public class PermissionFilter : IAsyncActionFilter
{
    private readonly DataContext _context;

    public PermissionFilter(DataContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var attr = ctx.ActionDescriptor.EndpointMetadata
            .OfType<RequirePermissionAttribute>()
            .FirstOrDefault();

        if (attr == null)
        {
            await next();
            return;
        }

        var roleClaim = ctx.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
        if (string.IsNullOrEmpty(roleClaim))
        {
            ctx.Result = new ForbidResult();
            return;
        }

        if (roleClaim.Equals("Administrador", System.StringComparison.OrdinalIgnoreCase) ||
            roleClaim.Equals("Admin", System.StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var hasPermission = await _context.RolePermissions
            .Include(rp => rp.Role)
            .AnyAsync(rp => rp.Role.Name == roleClaim
                         && rp.PermissionSlug == attr.Slug
                         && rp.CanView);

        if (!hasPermission)
        {
            ctx.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
