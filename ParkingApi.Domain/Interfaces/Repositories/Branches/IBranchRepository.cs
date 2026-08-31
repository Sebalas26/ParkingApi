using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Branches;

public interface IBranchRepository
{
    Task<IReadOnlyList<Branch>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> GetActiveAsync(int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> GetBranchesByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);
    Task<Branch?> GetByIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<Branch?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> GetBranchesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Branch> AddAsync(Branch branch, CancellationToken cancellationToken = default);
    Task<Branch> UpdateAsync(Branch branch, CancellationToken cancellationToken = default);
    Task<bool> AssignUserAsync(int userId, int branchId, bool isDefault, CancellationToken cancellationToken = default);
    Task<bool> UnassignUserAsync(int userId, int branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchPaymentMethod>> GetPaymentMethodsByBranchIdAsync(int branchId, CancellationToken cancellationToken = default);
    Task<bool> SetPaymentMethodsAsync(int branchId, IEnumerable<int> paymentMethodIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersByBranchIdAsync(int branchId, CancellationToken cancellationToken = default);
}
