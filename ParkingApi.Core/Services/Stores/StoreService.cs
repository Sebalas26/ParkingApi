using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Stores;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly ILogger<StoreService> _logger;

    public StoreService(IStoreRepository storeRepository, ILogger<StoreService> logger)
    {
        _storeRepository = storeRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storeRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar comercios", Constants.StoreError);
            return new List<Store>();
        }
    }

    public async Task<Store?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storeRepository.GetByIdAsync(storeId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar comercio {StoreId}", Constants.StoreError, storeId);
            return null;
        }
    }

    public async Task<Store> CreateAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storeRepository.AddAsync(store, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al crear comercio", Constants.StoreError);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _storeRepository.UpdateAsync(store, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar comercio {StoreId}", Constants.StoreError, store.StoreId);
            return false;
        }
    }
}
