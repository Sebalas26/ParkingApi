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

    public async Task<IEnumerable<GetUserRoleDto>> GetUserRoles(int? companyId = null, CancellationToken cancellation = default)
    {
        try
        {
            if (companyId.HasValue && companyId.Value > 0)
            {
                bool hasCompanyRoles = await _context.UserRole.AnyAsync(x => x.CompanyId == companyId.Value, cancellation);
                if (!hasCompanyRoles)
                {
                    await AutoProvisionCompanyAdminRoleAsync(companyId.Value, cancellation);
                }

                return await _context.UserRole
                    .AsNoTracking()
                    .Where(x => x.CompanyId == companyId.Value)
                    .Select(x => new GetUserRoleDto
                    {
                        IdUserRol = x.Id,
                        CompanyId = x.CompanyId,
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

    private async Task AutoProvisionCompanyAdminRoleAsync(int targetCompanyId, CancellationToken cancellation)
    {
        try
        {
            var companyAdminRole = new UserRole
            {
                CompanyId = targetCompanyId,
                Role = "Administrador",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _context.UserRole.AddAsync(companyAdminRole, cancellation);
            await _context.SaveChangesAsync(cancellation);

            var allowedModuleIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            foreach (var modId in allowedModuleIds)
            {
                _context.UserRoleModule.Add(new UserRoleModule
                {
                    UserRoleId = companyAdminRole.Id,
                    ModulesRoleId = modId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            var allowedActions = await _context.Action
                .Where(a => a.ModuleId != 16 && a.IsActive)
                .Select(a => a.Id)
                .ToListAsync(cancellation);

            foreach (var actionId in allowedActions)
            {
                _context.RoleAction.Add(new RoleAction
                {
                    RoleId = companyAdminRole.Id,
                    ActionId = actionId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al auto-aprovisionar el rol de Administrador para la empresa {CompanyId}", targetCompanyId);
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

    public async Task<GetUserRoleDto?> GetUserRoleName(string nameRol, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = nameRol.Trim().ToLower();
            return await _context.UserRole
                .AsNoTracking()
                .Where(x => x.Role.ToLower() == normalized)
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
