using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.UserRoles;

namespace ParkingApi.Domain.Interfaces.Services.UserRoles;

public interface IUserRoleService
{
    Task<IEnumerable<GetUserRoleDto>> GetUserRoles(int? companyId = null, CancellationToken cancellation = default);
    Task<GetUserRoleDto?> GetUserRoleById(int id, CancellationToken cancellation = default);
    Task<GetUserRoleDto> SaveOrEditUserRole(GetUserRoleDto userRole, CancellationToken cancellation = default);
    Task<bool> DeleteUserRole(int id, CancellationToken cancellation = default);
}
