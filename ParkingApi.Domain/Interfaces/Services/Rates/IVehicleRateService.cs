using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Rates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.Rates;

public interface IVehicleRateService
{
    Task<IReadOnlyList<VehicleRateDto>> GetAllRatesAsync(CancellationToken cancellationToken = default);
    Task<VehicleRateDto?> GetByIdAsync(Guid rateId, CancellationToken cancellationToken = default);
    Task<VehicleRateDto> CreateRateAsync(CreateRateDto dto, CancellationToken cancellationToken = default);
    Task<VehicleRateDto?> UpdateRateAsync(Guid rateId, UpdateRateDto dto, CancellationToken cancellationToken = default);
}
