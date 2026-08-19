using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Rates;

public interface IVehicleRateRepository : IBaseRepository<VehicleRate>
{
    Task<VehicleRate?> GetByTypeAsync(VehicleType type, CancellationToken cancellationToken = default);
}
