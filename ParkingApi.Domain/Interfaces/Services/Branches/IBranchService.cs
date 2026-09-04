using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Branches;

namespace ParkingApi.Domain.Interfaces.Services.Branches;

public interface IBranchService
{
    Task<IReadOnlyList<BranchDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetActiveAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetBranchesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetBranchesByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);
    Task<BranchDto> CreateAsync(CreateBranchDto dto, CancellationToken cancellationToken = default);
    Task<BranchDto?> UpdateAsync(int branchId, UpdateBranchDto dto, CancellationToken cancellationToken = default);
    Task<bool> AssignUserAsync(AssignUserBranchDto dto, CancellationToken cancellationToken = default);
    Task<bool> UnassignUserAsync(int userId, int branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchPaymentMethodDto>> GetPaymentMethodsAsync(int branchId, CancellationToken cancellationToken = default);
    Task<bool> ConfigurePaymentMethodsAsync(ConfigureBranchPaymentMethodsDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Dtos.Users.GetUsersDto>> GetUsersByBranchIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int branchId, CancellationToken cancellationToken = default);
}
