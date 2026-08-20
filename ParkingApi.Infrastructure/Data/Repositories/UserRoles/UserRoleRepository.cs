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

    public async Task<IEnumerable<GetUserRoleDto>> GetUserRoles(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRole
                .AsNoTracking()
                .Select(x => new GetUserRoleDto
                {
                    IdUserRol = x.Id,
                    RoleName = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.RoleName)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.UserRoleError);
            return Enumerable.Empty<GetUserRoleDto>();
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
}
