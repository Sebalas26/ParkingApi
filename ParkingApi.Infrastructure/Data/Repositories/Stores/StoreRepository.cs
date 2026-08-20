<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
=======
using System;
using System.Collections.Generic;
using System.Linq;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Stores;

public sealed class StoreRepository : IStoreRepository
=======
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Stores;

public class StoreRepository : IStoreRepository
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
                .Include(s => s.Agreements)
                .AsNoTracking()
=======
                .AsNoTracking()
                .Include(s => s.Agreements)
                .OrderBy(s => s.Name)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al consultar comercios aliados.");
=======
            _logger.LogError(ex, "{Error}: Error al consultar comercios", Constants.StoreError);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return new List<Store>();
        }
    }

<<<<<<< HEAD
    public async Task<Store?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
=======
    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    {
        try
        {
            return await _context.Stores
                .Include(s => s.Agreements)
<<<<<<< HEAD
                .FirstOrDefaultAsync(s => s.StoreId == storeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercio por ID: {StoreId}", storeId);
=======
                .FirstOrDefaultAsync(s => s.StoreId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar comercio {Id}", Constants.StoreError, id);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return null;
        }
    }

<<<<<<< HEAD
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
=======
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
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
            _logger.LogError(ex, "Error al actualizar comercio: {StoreId}", store.StoreId);
=======
            _logger.LogError(ex, "{Error}: Error al actualizar comercio {Id}", Constants.StoreError, store.StoreId);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return false;
        }
    }
}
