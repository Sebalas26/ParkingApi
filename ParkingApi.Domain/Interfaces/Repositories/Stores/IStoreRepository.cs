<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Stores;

public interface IStoreRepository
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default);
<<<<<<< HEAD
    Task<Store?> GetByIdAsync(Guid storeId, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(Store store, CancellationToken cancellationToken = default);
=======
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Store> AddAsync(Store store, CancellationToken cancellationToken = default);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    Task<bool> UpdateAsync(Store store, CancellationToken cancellationToken = default);
}
