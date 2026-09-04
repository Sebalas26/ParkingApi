using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Plans;

public interface IPlanRepository
{
    Task<IReadOnlyList<SaaSPlan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SaaSPlan>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<SaaSPlan?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SaaSPlan> AddAsync(SaaSPlan plan, CancellationToken cancellationToken = default);
    Task<SaaSPlan> UpdateAsync(SaaSPlan plan, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
