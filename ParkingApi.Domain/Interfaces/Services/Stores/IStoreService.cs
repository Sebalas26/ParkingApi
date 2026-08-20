using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.Stores;

public interface IStoreService
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Store?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<Store> CreateAsync(Store store, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
