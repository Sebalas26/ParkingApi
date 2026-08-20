using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.UserRoleModules;

namespace ParkingApi.Domain.Interfaces.Services.UserRoleModules;

public interface IUserRoleModuleService
{
    Task<IEnumerable<GetUserRoleModuleDto>> GetUserRoleModules(CancellationToken cancellation = default);
    Task<GetUserRoleModuleDto?> GetUserRoleModuleById(int id, CancellationToken cancellation = default);
    Task<GetUserRoleModuleDto> SaveOrEditUserRoleModule(SaveUserRoleModuleDto saveUserRoleModule, CancellationToken cancellation = default);
}
