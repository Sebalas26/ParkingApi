using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Services.Stores;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Stores;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IReadOnlyList<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default)
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

    public async Task<StoreDto?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default)
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

    public async Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken cancellationToken = default)
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

    public async Task<StoreDto?> UpdateAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default)
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

    public async Task<bool> DeleteAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await _storeRepository.GetByIdAsync(storeId, cancellationToken);
        if (store == null) return false;

        store.IsActive = false;
        await _storeRepository.UpdateAsync(store, cancellationToken);
        return true;
    }
}
