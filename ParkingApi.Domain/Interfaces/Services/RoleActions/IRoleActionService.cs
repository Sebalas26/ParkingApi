using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.RoleActions;

namespace ParkingApi.Domain.Interfaces.Services.RoleActions;

public interface IRoleActionService
{
    Task<List<ActionsRoleDto>> GetActionsByRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<List<string>> GetActionsByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
}
