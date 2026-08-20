using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.UserRoleModules;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.UserRoleModules;

public class UserRoleModuleRepository : IUserRoleModuleRepository
{
    private readonly DataContext _context;
    private readonly ILogger<UserRoleModuleRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public UserRoleModuleRepository(DataContext context, ILogger<UserRoleModuleRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetUserRoleModuleDto>> GetUserRoleModules(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRoleModule
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Include(x => x.ModuleIdNavigation)
                .Select(x => new GetUserRoleModuleDto
                {
                    Id = x.Id,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Role = new GetUserRoleDto
                    {
                        IdUserRol = x.UserRoleIdNavigation.Id,
                        RoleName = x.UserRoleIdNavigation.Role,
                        IsActive = x.UserRoleIdNavigation.IsActive
                    },
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    }
                })
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener UserRoleModules");
            return Enumerable.Empty<GetUserRoleModuleDto>();
        }
    }

    public async Task<GetUserRoleModuleDto?> GetUserRoleModuleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRoleModule
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Include(x => x.ModuleIdNavigation)
                .Where(x => x.Id == id)
                .Select(x => new GetUserRoleModuleDto
                {
                    Id = x.Id,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Role = new GetUserRoleDto
                    {
                        IdUserRol = x.UserRoleIdNavigation.Id,
                        RoleName = x.UserRoleIdNavigation.Role,
                        IsActive = x.UserRoleIdNavigation.IsActive
                    },
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    }
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener UserRoleModule por Id");
            return null;
        }
    }

    public async Task<bool> SaveUserRoleModule(UserRoleModule userRoleModule, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                userRoleModule.ResponsibleUserId = uid;
            }
            userRoleModule.CreatedAt = DateTime.UtcNow;
            await _context.UserRoleModule.AddAsync(userRoleModule, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar UserRoleModule");
            return false;
        }
    }

    public async Task<bool> UpdateUserRoleModule(UserRoleModule userRoleModule, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.UserRoleModule.FirstOrDefaultAsync(urm => urm.Id == userRoleModule.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.IsActive = userRoleModule.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar UserRoleModule");
            return false;
        }
    }

    public async Task<bool> ValidateExistUserRoleModule(int userRoleId, int moduleId, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRoleModule
                .AsNoTracking()
                .AnyAsync(x => x.UserRoleId == userRoleId && x.ModulesRoleId == moduleId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar existencia de UserRoleModule");
            return false;
        }
    }

    public async Task<GetUserRoleModuleDto?> GetuserRoleModulesCreate(int userRoleId, int moduleId, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.UserRoleModule
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Include(x => x.ModuleIdNavigation)
                .Where(x => x.UserRoleId == userRoleId && x.ModulesRoleId == moduleId)
                .Select(x => new GetUserRoleModuleDto
                {
                    Id = x.Id,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Role = new GetUserRoleDto
                    {
                        IdUserRol = x.UserRoleIdNavigation.Id,
                        RoleName = x.UserRoleIdNavigation.Role,
                        IsActive = x.UserRoleIdNavigation.IsActive
                    },
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    }
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar UserRoleModule creado");
            return null;
        }
    }
}
