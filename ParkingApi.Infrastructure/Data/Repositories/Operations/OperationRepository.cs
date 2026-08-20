using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.Operations;
using ParkingApi.Domain.Interfaces.Repositories.Operations;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Operations;

public class OperationRepository : IOperationRepository
{
    private readonly DataContext _context;
    private readonly ILogger<OperationRepository> _logger;
    private readonly ICurrentUserService _currentUser;

    public OperationRepository(DataContext context, ILogger<OperationRepository> logger, ICurrentUserService currentUser)
    {
        _context = context;
        _logger = logger;
        _currentUser = currentUser;
    }

    public async Task<IEnumerable<GetOperationDto>> GetOperations(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.Operation
                .AsNoTracking()
                .Select(x => new GetOperationDto
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
            _logger.LogError(ex, Constants.OperationError);
            return Enumerable.Empty<GetOperationDto>();
        }
    }

    public async Task<GetOperationDto?> GetOperationsById(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.Operation
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new GetOperationDto
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
            _logger.LogError(ex, Constants.OperationError);
            return null;
        }
    }

    public async Task<GetOperationDto?> GetOperationName(string operationName, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = operationName.Trim().ToLower();
            return await _context.Operation
                .AsNoTracking()
                .Where(x => x.Name.ToLower() == normalized)
                .Select(x => new GetOperationDto
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
            _logger.LogError(ex, Constants.OperationError);
            return null;
        }
    }

    public async Task<bool> SaveOperation(Operation operation, CancellationToken cancellation = default)
    {
        try
        {
            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                operation.ResponsibleUserId = uid;
            }
            operation.CreatedAt = DateTime.UtcNow;
            await _context.Operation.AddAsync(operation, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar operación");
            return false;
        }
    }

    public async Task<bool> UpdateOperation(Operation operation, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.Operation.FirstOrDefaultAsync(o => o.Id == operation.Id, cancellation);
            if (existing == null) return false;

            if (int.TryParse(_currentUser?.UserId, out int uid))
            {
                existing.ResponsibleUserId = uid;
            }
            existing.Name = operation.Name;
            existing.IsActive = operation.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar operación");
            return false;
        }
    }
}
