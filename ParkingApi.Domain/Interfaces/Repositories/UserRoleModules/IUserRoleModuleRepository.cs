using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.UserRoleModules;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.UserRoleModules;

public interface IUserRoleModuleRepository
{
    Task<IEnumerable<GetUserRoleModuleDto>> GetUserRoleModules(CancellationToken cancellation = default);
    Task<GetUserRoleModuleDto?> GetUserRoleModuleById(int id, CancellationToken cancellation = default);
    Task<bool> SaveUserRoleModule(UserRoleModule userRoleModule, CancellationToken cancellation = default);
    Task<bool> UpdateUserRoleModule(UserRoleModule userRoleModule, CancellationToken cancellation = default);
    Task<bool> ValidateExistUserRoleModule(int userRoleId, int moduleId, CancellationToken cancellation = default);
    Task<GetUserRoleModuleDto?> GetuserRoleModulesCreate(int userRoleId, int moduleId, CancellationToken cancellation = default);
}
