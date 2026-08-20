using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.RoleActions;

public class RoleActionRepository : IRoleActionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<RoleActionRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public RoleActionRepository(DataContext context, ILogger<RoleActionRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<List<ActionsRoleDto>> GetActionsByRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.RoleAction
                .AsNoTracking()
                .Include(x => x.ActionIdNavigation)
                .Where(ra => ra.RoleId == roleId)
                .Select(ra => new ActionsRoleDto
                {
                    ActionId = ra.ActionId,
                    IsActive = ra.IsActive,
                    ModuleId = ra.ActionIdNavigation.ModuleId,
                    ActionName = ra.ActionIdNavigation.Slug
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetActionsByRoleAsync");
            return new List<ActionsRoleDto>();
        }
    }

    public async Task<List<string>> GetActionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.RoleAction
                .AsNoTracking()
                .Include(x => x.ActionIdNavigation)
                .Where(ra => ra.RoleId == roleId && ra.IsActive)
                .Select(ra => ra.ActionIdNavigation.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetActionsByRoleIdAsync");
            return new List<string>();
        }
    }

    public async Task<List<ValidateRolActionDto>> ValidateActionRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.RoleAction
                .AsNoTracking()
                .Where(ra => ra.RoleId == roleId)
                .Select(ra => new ValidateRolActionDto
                {
                    Id = ra.Id,
                    ActionId = ra.ActionId
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ValidateActionRoleAsync");
            return new List<ValidateRolActionDto>();
        }
    }

    public async Task<bool> SaveRoleAction(RoleAction roleAction, CancellationToken cancellationToken = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                roleAction.ResponsibleUserId = uid;
            }
            roleAction.CreatedAt = DateTime.UtcNow;
            await _context.RoleAction.AddAsync(roleAction, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SaveRoleAction");
            return false;
        }
    }

    public async Task<bool> ActiveOrInactiveRoleAction(RoleAction roleAction, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.RoleAction.FirstOrDefaultAsync(ra => ra.Id == roleAction.Id, cancellationToken);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.IsActive = roleAction.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActiveOrInactiveRoleAction");
            return false;
        }
    }

    public async Task<bool> ValidateActionActive(int actionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.RoleAction
                .AsNoTracking()
                .AnyAsync(ra => ra.ActionId == actionId && ra.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ValidateActionActive");
            return false;
        }
    }
}
