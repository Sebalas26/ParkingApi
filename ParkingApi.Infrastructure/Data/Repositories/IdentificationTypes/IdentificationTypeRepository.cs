using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Repositories.IdentificationTypes;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.IdentificationTypes;

public class IdentificationTypeRepository : IIdentificationTypeRepository
{
    private readonly DataContext _context;
    private readonly ILogger<IdentificationTypeRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public IdentificationTypeRepository(DataContext context, ILogger<IdentificationTypeRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetIdentificationTypeDto>> GetAllAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.IdentificationType
                .AsNoTracking()
                .Select(x => new GetIdentificationTypeDto
                {
                    Id = x.Id,
                    Name = x.Identification,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return Enumerable.Empty<GetIdentificationTypeDto>();
        }
    }

    public async Task<IEnumerable<GetIdentificationTypeDto>> GetAllActiveAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.IdentificationType
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new GetIdentificationTypeDto
                {
                    Id = x.Id,
                    Name = x.Identification,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderBy(x => x.Name)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return Enumerable.Empty<GetIdentificationTypeDto>();
        }
    }

    public async Task<GetIdentificationTypeDto?> GetByIdAsync(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.IdentificationType
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new GetIdentificationTypeDto
                {
                    Id = x.Id,
                    Name = x.Identification,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return null;
        }
    }

    public async Task<GetIdentificationTypeDto?> GetByNameAsync(string name, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = name.Trim().ToLower();
            return await _context.IdentificationType
                .AsNoTracking()
                .Where(x => x.Identification.ToLower() == normalized)
                .Select(x => new GetIdentificationTypeDto
                {
                    Id = x.Id,
                    Name = x.Identification,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return null;
        }
    }

    public async Task<bool> CreateAsync(IdentificationType identificationType, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                identificationType.ResponsibleUserId = uid;
            }
            identificationType.CreatedAt = DateTime.UtcNow;
            await _context.IdentificationType.AddAsync(identificationType, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tipo de identificación");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(IdentificationType identificationType, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.IdentificationType.FirstOrDefaultAsync(i => i.Id == identificationType.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.Identification = identificationType.Identification;
            existing.IsActive = identificationType.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tipo de identificación");
            return false;
        }
    }

    public async Task<bool> ValidateExist(string name, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = name.Trim().ToLower();
            return await _context.IdentificationType
                .AsNoTracking()
                .AnyAsync(x => x.Identification.ToLower() == normalized, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.IdentificationTypeError);
            return false;
        }
    }
}
