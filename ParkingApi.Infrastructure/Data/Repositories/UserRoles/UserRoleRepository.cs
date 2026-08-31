using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.UserRoles;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.UserRoles;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly DataContext _context;
    private readonly ILogger<UserRoleRepository> _logger;

    public UserRoleRepository(DataContext context, ILogger<UserRoleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<GetUserRoleDto>> GetUserRoles(int? companyId = null, int? branchId = null, CancellationToken cancellation = default)
    {
        try
        {
            if (companyId.HasValue && companyId.Value > 0)
            {
                await EnsureCompanyAdminRoleAsync(companyId.Value, cancellation);

                var query = _context.UserRole.AsNoTracking().Where(x => x.CompanyId == companyId.Value);
                if (branchId.HasValue && branchId.Value > 0)
                {
                    query = query.Where(x => x.BranchId == branchId.Value || x.BranchId == null);
                }

                return await query
                    .Select(x => new GetUserRoleDto
                    {
                        IdUserRol = x.Id,
                        CompanyId = x.CompanyId,
                        BranchId = x.BranchId,
                        RoleName = x.Role,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .OrderBy(x => x.RoleName)
                    .ToListAsync(cancellation);
            }
            else
            {
                return await _context.UserRole
                    .AsNoTracking()
                    .Where(x => x.CompanyId == null)
                    .Select(x => new GetUserRoleDto
                    {
                        IdUserRol = x.Id,
                        CompanyId = x.CompanyId,
                        BranchId = x.BranchId,
                        RoleName = (x.Id == 1 && (x.Role == "Administrador" || x.Role == "Admin")) ? "Super Administrador" : x.Role,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .OrderBy(x => x.RoleName)
                    .ToListAsync(cancellation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.UserRoleError);
            return Enumerable.Empty<GetUserRoleDto>();
        }
    }

    private async Task EnsureCompanyAdminRoleAsync(int targetCompanyId, CancellationToken cancellation)
    {
        try
        {
            var hasAnyRole = await _context.UserRole
                .AnyAsync(x => x.CompanyId == targetCompanyId, cancellation);

            if (!hasAnyRole)
            {
                var allowedModuleIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                var allowedActions = await _context.Action
                    .Where(a => a.ModuleId != 16 && a.IsActive && !a.Slug.StartsWith("companies."))
                    .Select(a => a.Id)
                    .ToListAsync(cancellation);

                var newRole = new UserRole
                {
                    CompanyId = targetCompanyId,
                    Role = "Administrador",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserRole.AddAsync(newRole, cancellation);
                await _context.SaveChangesAsync(cancellation);

                foreach (var modId in allowedModuleIds)
                {
                    _context.UserRoleModule.Add(new UserRoleModule
                    {
                        UserRoleId = newRole.Id,
                        ModulesRoleId = modId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                foreach (var actionId in allowedActions)
                {
                    _context.RoleAction.Add(new RoleAction
                    {
                        RoleId = newRole.Id,
                        ActionId = actionId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync(cancellation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asegurar rol administrador para la empresa {CompanyId}", targetCompanyId);
        }
    }

    public async Task<GetUserRoleDto?> GetUserRoleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRole
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new GetUserRoleDto
                {
                    IdUserRol = x.Id,
                    RoleName = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.UserRoleError);
            return null;
        }
    }

    public async Task<GetUserRoleDto?> GetUserRoleName(string nameRol, int? companyId = null, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = nameRol.Trim().ToLower();
            return await _context.UserRole
                .AsNoTracking()
                .Where(x => x.Role.ToLower() == normalized && x.CompanyId == companyId)
                .Select(x => new GetUserRoleDto
                {
                    IdUserRol = x.Id,
                    CompanyId = x.CompanyId,
                    RoleName = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.UserRoleError);
            return null;
        }
    }

    public async Task<bool> SaveUserRole(UserRole userRole, CancellationToken cancellation = default)
    {
        try
        {
            userRole.CreatedAt = DateTime.UtcNow;
            await _context.UserRole.AddAsync(userRole, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar rol de usuario");
            return false;
        }
    }

    public async Task<bool> UpdateUserRole(UserRole userRole, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.UserRole.FirstOrDefaultAsync(r => r.Id == userRole.Id, cancellation);
            if (existing == null) return false;

            existing.Role = userRole.Role;
            existing.IsActive = userRole.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol de usuario");
            return false;
        }
    }

    public async Task<bool> DeleteUserRole(int id, CancellationToken cancellation = default)
    {
        try
        {
            var role = await _context.UserRole.FirstOrDefaultAsync(r => r.Id == id, cancellation);
            if (role == null) return false;

            var normalizedRole = (role.Role ?? string.Empty).Trim().ToLower();
            if (role.Id == 1 || normalizedRole == "super administrador" || normalizedRole == "superadmin" || normalizedRole == "super admin")
            {
                throw new InvalidOperationException("No está permitido eliminar el rol Super Administrador del sistema.");
            }

            _context.UserRole.Remove(role);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol de usuario #{Id}", id);
            throw;
        }
    }
}
