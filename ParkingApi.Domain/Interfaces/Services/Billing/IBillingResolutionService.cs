using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Billing;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.Billing;

public interface IBillingResolutionService
{
    Task<IReadOnlyList<BillingResolutionDto>> GetAllAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BillingResolutionDto>> GetActiveAsync(int? branchId = null, CancellationToken cancellationToken = default);
    Task<BillingResolutionDto?> GetByIdAsync(Guid resolutionId, CancellationToken cancellationToken = default);
    Task<BillingResolutionDto> CreateAsync(SaveBillingResolutionDto dto, CancellationToken cancellationToken = default);
    Task<BillingResolutionDto?> UpdateAsync(Guid resolutionId, SaveBillingResolutionDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid resolutionId, CancellationToken cancellationToken = default);
}
