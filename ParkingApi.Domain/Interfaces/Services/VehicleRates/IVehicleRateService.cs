using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.VehicleRates;

public interface IVehicleRateService
{
    Task<IReadOnlyList<VehicleRate>> GetAllRatesAsync(CancellationToken cancellationToken = default);
    Task<VehicleRate?> GetByIdAsync(Guid rateId, CancellationToken cancellationToken = default);
    Task<VehicleRate> UpdateRateAsync(Guid rateId, decimal hourRate, decimal minuteRate, decimal fullDayRate, int graceMinutes, CancellationToken cancellationToken = default);
}
