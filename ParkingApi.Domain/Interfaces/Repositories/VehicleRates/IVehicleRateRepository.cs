using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.VehicleRates;

public interface IVehicleRateRepository
{
    Task<IReadOnlyList<VehicleRate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<VehicleRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VehicleRate?> GetByTypeAsync(VehicleType type, CancellationToken cancellationToken = default);
    Task<VehicleRate> AddAsync(VehicleRate rate, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(VehicleRate rate, CancellationToken cancellationToken = default);
}
