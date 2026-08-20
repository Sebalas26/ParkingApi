using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Modules;

namespace ParkingApi.Domain.Interfaces.Services.Modules;

public interface IModuleService
{
    Task<IEnumerable<GetModuleDto>> GetModules(CancellationToken cancellation = default);
    Task<GetModuleDto?> GetModuleById(int id, CancellationToken cancellation = default);
    Task<GetModuleDto> SaveOrEditModule(GetModuleDto module, CancellationToken cancellation = default);
}
