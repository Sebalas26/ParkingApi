using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.UserRoles;

public interface IUserRoleRepository
{
    Task<IEnumerable<GetUserRoleDto>> GetUserRoles(int? companyId = null, CancellationToken cancellation = default);
    Task<GetUserRoleDto?> GetUserRoleById(int id, CancellationToken cancellation = default);
    Task<GetUserRoleDto?> GetUserRoleName(string nameRol, int? companyId = null, CancellationToken cancellation = default);
    Task<bool> SaveUserRole(UserRole userRole, CancellationToken cancellation = default);
    Task<bool> UpdateUserRole(UserRole userRole, CancellationToken cancellation = default);
    Task<bool> DeleteUserRole(int id, CancellationToken cancellation = default);
}
