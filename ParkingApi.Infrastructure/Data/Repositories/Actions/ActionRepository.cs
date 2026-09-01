using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.Actions;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Interfaces.Repositories.Actions;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;
using ActionModel = ParkingApi.Domain.Models.Action;

namespace ParkingApi.Infrastructure.Data.Repositories.Actions;

public class ActionRepository : IActionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ActionRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public ActionRepository(DataContext context, ILogger<ActionRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetActionsDto>> GetActions(CancellationToken cancellation = default)
    {
        try
        {
            var query = _context.Action
                .AsNoTracking()
                .Include(x => x.ModuleIdNavigation)
                .Include(x => x.OperationIdNavigation)
                .AsQueryable();

            if (_currentUser != null && !_currentUser.IsSuperAdmin)
            {
                query = query.Where(x => x.ModuleId != 16 && !x.Slug.StartsWith("companies."));
            }

            return await query
                .Select(x => new GetActionsDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    },
                    Operation = new GetOperationDto
                    {
                        Id = x.OperationIdNavigation.Id,
                        Name = x.OperationIdNavigation.Name,
                        IsActive = x.OperationIdNavigation.IsActive
                    }
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ActionError);
            return Enumerable.Empty<GetActionsDto>();
        }
    }

    public async Task<IEnumerable<GetActionsDto>> GetActionsActive(CancellationToken cancellation = default)
    {
        try
        {
            var query = _context.Action
                .AsNoTracking()
                .Include(x => x.ModuleIdNavigation)
                .Include(x => x.OperationIdNavigation)
                .Where(x => x.IsActive);

            if (_currentUser != null && !_currentUser.IsSuperAdmin)
            {
                query = query.Where(x => x.ModuleId != 16 && !x.Slug.StartsWith("companies."));
            }

            return await query
                .Select(x => new GetActionsDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    },
                    Operation = new GetOperationDto
                    {
                        Id = x.OperationIdNavigation.Id,
                        Name = x.OperationIdNavigation.Name,
                        IsActive = x.OperationIdNavigation.IsActive
                    }
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ActionError);
            return Enumerable.Empty<GetActionsDto>();
        }
    }

    public async Task<GetActionsDto?> GetActionsById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.Action
                .AsNoTracking()
                .Include(x => x.ModuleIdNavigation)
                .Include(x => x.OperationIdNavigation)
                .Where(x => x.Id == id)
                .Select(x => new GetActionsDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    },
                    Operation = new GetOperationDto
                    {
                        Id = x.OperationIdNavigation.Id,
                        Name = x.OperationIdNavigation.Name,
                        IsActive = x.OperationIdNavigation.IsActive
                    }
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ActionError);
            return null;
        }
    }

    public async Task<bool> SaveActions(ActionModel action, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                action.ResponsibleUserId = uid;
            }
            action.CreatedAt = DateTime.UtcNow;
            await _context.Action.AddAsync(action, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar acción");
            return false;
        }
    }

    public async Task<bool> UpdateActions(ActionModel action, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.Action.FirstOrDefaultAsync(a => a.Id == action.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.Name = action.Name;
            existing.Slug = action.Slug;
            existing.ModuleId = action.ModuleId;
            existing.OperationId = action.OperationId;
            existing.IsActive = action.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar acción");
            return false;
        }
    }

    public async Task<bool> GetActionByExist(string name, int idModule, int idOperation, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = name.Trim().ToLower();
            return await _context.Action
                .AsNoTracking()
                .AnyAsync(x => x.Name.ToLower() == normalized && x.ModuleId == idModule && x.OperationId == idOperation, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ActionError);
            return false;
        }
    }

    public async Task<GetActionsDto?> GetActionByName(string name, int idModule, int idOperation, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = name.Trim().ToLower();
            return await _context.Action
                .AsNoTracking()
                .Include(x => x.ModuleIdNavigation)
                .Include(x => x.OperationIdNavigation)
                .Where(x => x.Name.ToLower() == normalized && x.ModuleId == idModule && x.OperationId == idOperation)
                .Select(x => new GetActionsDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    Module = new GetModuleDto
                    {
                        Id = x.ModuleIdNavigation.Id,
                        Name = x.ModuleIdNavigation.Name,
                        IsActive = x.ModuleIdNavigation.IsActive
                    },
                    Operation = new GetOperationDto
                    {
                        Id = x.OperationIdNavigation.Id,
                        Name = x.OperationIdNavigation.Name,
                        IsActive = x.OperationIdNavigation.IsActive
                    }
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ActionError);
            return null;
        }
    }
}
