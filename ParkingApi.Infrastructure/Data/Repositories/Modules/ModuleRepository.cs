using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Interfaces.Repositories.Modules;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Modules;

public class ModuleRepository : IModuleRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ModuleRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public ModuleRepository(DataContext context, ILogger<ModuleRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetModuleDto>> GetModules(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.Module
                .AsNoTracking()
                .Select(x => new GetModuleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ModuleError);
            return Enumerable.Empty<GetModuleDto>();
        }
    }

    public async Task<GetModuleDto?> GetModuleById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.Module
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new GetModuleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ModuleError);
            return null;
        }
    }

    public async Task<GetModuleDto?> GetModuleName(string moduleName, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = moduleName.Trim().ToLower();
            return await _context.Module
                .AsNoTracking()
                .Where(x => x.Name.ToLower() == normalized)
                .Select(x => new GetModuleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.ModuleError);
            return null;
        }
    }

    public async Task<bool> SaveModule(Module module, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                module.ResponsibleUserId = uid;
            }
            module.CreatedAt = DateTime.UtcNow;
            await _context.Module.AddAsync(module, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar módulo");
            return false;
        }
    }

    public async Task<bool> UpdateModule(Module module, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.Module.FirstOrDefaultAsync(m => m.Id == module.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.Name = module.Name;
            existing.IsActive = module.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar módulo");
            return false;
        }
    }
}
