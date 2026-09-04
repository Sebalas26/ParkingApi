using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Plans;

namespace ParkingApi.Domain.Interfaces.Services.Plans;

public interface IPlanService
{
    Task<IReadOnlyList<PlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default);
    Task<PlanDto?> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PlanDto> CreatePlanAsync(CreatePlanDto dto, CancellationToken cancellationToken = default);
    Task<PlanDto> UpdatePlanAsync(int id, UpdatePlanDto dto, CancellationToken cancellationToken = default);
    Task<bool> TogglePlanStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeletePlanAsync(int id, CancellationToken cancellationToken = default);
}
