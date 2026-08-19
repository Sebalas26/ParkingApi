using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Stores;

public sealed class StoreRepository : IStoreRepository
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
                .Include(s => s.Agreements)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercios aliados.");
            return new List<Store>();
        }
    }

    public async Task<Store?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Stores
                .Include(s => s.Agreements)
                .FirstOrDefaultAsync(s => s.StoreId == storeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercio por ID: {StoreId}", storeId);
            return null;
        }
    }

    public async Task<bool> AddAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Stores.AddAsync(store, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar comercio: {Name}", store.Name);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Stores.Update(store);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comercio: {StoreId}", store.StoreId);
            return false;
        }
    }
}
