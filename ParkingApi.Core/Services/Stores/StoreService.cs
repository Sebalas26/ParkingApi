using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Stores;
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

    public async Task<IReadOnlyList<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stores = await _storeRepository.GetAllAsync(cancellationToken);
            return stores.Select(s => new StoreDto
            {
                StoreId = s.StoreId,
                Name = s.Name,
                TaxId = s.TaxId,
                PhoneNumber = s.PhoneNumber,
                ContactName = s.ContactName,
                IsActive = s.IsActive,
                AgreementsCount = s.Agreements?.Count ?? 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercios.");
            return new List<StoreDto>();
        }
    }

    public async Task<StoreDto?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var s = await _storeRepository.GetByIdAsync(storeId, cancellationToken);
            if (s == null) return null;

            return new StoreDto
            {
                StoreId = s.StoreId,
                Name = s.Name,
                TaxId = s.TaxId,
                PhoneNumber = s.PhoneNumber,
                ContactName = s.ContactName,
                IsActive = s.IsActive,
                AgreementsCount = s.Agreements?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar comercio por ID: {StoreId}", storeId);
            return null;
        }
    }

    public async Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = new Store
            {
                StoreId = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                TaxId = dto.TaxId.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                ContactName = dto.ContactName?.Trim(),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _storeRepository.AddAsync(store, cancellationToken);

            return new StoreDto
            {
                StoreId = store.StoreId,
                Name = store.Name,
                TaxId = store.TaxId,
                PhoneNumber = store.PhoneNumber,
                ContactName = store.ContactName,
                IsActive = store.IsActive,
                AgreementsCount = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar comercio: {Name}", dto.Name);
            throw;
        }
    }

    public async Task<StoreDto?> UpdateAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _storeRepository.GetByIdAsync(storeId, cancellationToken);
            if (store == null) return null;

            store.Name = dto.Name.Trim();
            store.TaxId = dto.TaxId.Trim();
            store.PhoneNumber = dto.PhoneNumber?.Trim();
            store.ContactName = dto.ContactName?.Trim();
            store.IsActive = dto.IsActive;

            await _storeRepository.UpdateAsync(store, cancellationToken);

            return new StoreDto
            {
                StoreId = store.StoreId,
                Name = store.Name,
                TaxId = store.TaxId,
                PhoneNumber = store.PhoneNumber,
                ContactName = store.ContactName,
                IsActive = store.IsActive,
                AgreementsCount = store.Agreements?.Count ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar comercio: {StoreId}", storeId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _storeRepository.GetByIdAsync(storeId, cancellationToken);
            if (store == null) return false;

            store.IsActive = false;
            return await _storeRepository.UpdateAsync(store, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar comercio: {StoreId}", storeId);
            return false;
        }
    }
}
