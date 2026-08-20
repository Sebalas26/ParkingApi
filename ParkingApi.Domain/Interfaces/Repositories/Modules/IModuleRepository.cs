using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Modules;

public interface IModuleRepository
{
    Task<IEnumerable<GetModuleDto>> GetModules(CancellationToken cancellation = default);
    Task<GetModuleDto?> GetModuleById(int id, CancellationToken cancellation = default);
    Task<GetModuleDto?> GetModuleName(string moduleName, CancellationToken cancellation = default);
    Task<bool> SaveModule(Module module, CancellationToken cancellation = default);
    Task<bool> UpdateModule(Module module, CancellationToken cancellation = default);
}
