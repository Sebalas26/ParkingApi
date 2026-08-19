using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Stores;

namespace ParkingApi.Domain.Interfaces.Services.Stores;

public interface IStoreService
{
    Task<IReadOnlyList<StoreDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<StoreDto?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken cancellationToken = default);
    Task<StoreDto?> UpdateAsync(Guid storeId, UpdateStoreDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid storeId, CancellationToken cancellationToken = default);
}
