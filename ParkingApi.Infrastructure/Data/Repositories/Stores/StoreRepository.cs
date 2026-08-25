using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Stores;

public class StoreRepository : IStoreRepository
{
    private readonly DataContext _context;
    private readonly ILogger<StoreRepository> _logger;

    public StoreRepository(DataContext context, ILogger<StoreRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Stores
                .AsNoTracking()
                .Include(s => s.Agreements)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar comercios", Constants.StoreError);
            return new List<Store>();
        }
    }

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Stores
                .Include(s => s.Agreements)
                .FirstOrDefaultAsync(s => s.StoreId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar comercio {Id}", Constants.StoreError, id);
            return null;
        }
    }

    public async Task<Store> AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            store.CreatedAtUtc = DateTime.UtcNow;
            await _context.Stores.AddAsync(store, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return store;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al guardar comercio", Constants.StoreError);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.Stores.FirstOrDefaultAsync(s => s.StoreId == store.StoreId, cancellationToken);
            if (existing == null) return false;

            existing.Name = store.Name;
            existing.TaxId = store.TaxId ?? existing.TaxId;
            existing.PhoneNumber = store.PhoneNumber ?? existing.PhoneNumber;
            existing.IsActive = store.IsActive;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar comercio {Id}", Constants.StoreError, store.StoreId);
            return false;
        }
    }
}
