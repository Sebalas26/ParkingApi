using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Stores;

public interface IStoreRepository
{
    Task<IReadOnlyList<Store>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Store> AddAsync(Store store, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
