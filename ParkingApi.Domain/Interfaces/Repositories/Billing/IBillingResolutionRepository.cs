using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Billing;

public interface IBillingResolutionRepository
{
    Task<IReadOnlyList<BillingResolution>> GetAllAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillingResolution>> GetActiveAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<BillingResolution?> GetByIdAsync(Guid resolutionId, CancellationToken cancellationToken = default);
    Task<BillingResolution?> GetByPrefixAndNumberAsync(string prefix, string resolutionNumber, CancellationToken cancellationToken = default);
    Task<BillingResolution> AddAsync(BillingResolution resolution, CancellationToken cancellationToken = default);
    Task<BillingResolution?> UpdateAsync(BillingResolution resolution, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid resolutionId, CancellationToken cancellationToken = default);
}
