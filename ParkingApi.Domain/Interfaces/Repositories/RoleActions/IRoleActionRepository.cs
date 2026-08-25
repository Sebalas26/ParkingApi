using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.RoleActions;

public interface IRoleActionRepository
{
    Task<List<ActionsRoleDto>> GetActionsByRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<List<string>> GetActionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
    Task<List<ValidateRolActionDto>> ValidateActionRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> SaveRoleAction(RoleAction roleAction, CancellationToken cancellationToken = default);
    Task<bool> ActiveOrInactiveRoleAction(RoleAction roleAction, CancellationToken cancellationToken = default);
    Task<bool> ValidateActionActive(int actionId, CancellationToken cancellationToken = default);
    Task<bool> AssignRolePermissionsAsync(int roleId, List<int> actionIds, CancellationToken cancellationToken = default);
}
