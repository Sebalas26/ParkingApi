using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Incidents;

namespace ParkingApi.Domain.Interfaces.Services.Incidents;

public interface IVehicleIncidentService
{
    Task<IReadOnlyList<VehicleIncidentDto>> GetAllAsync(int? branchId = null, string? status = null, bool? isBlocked = null, string? search = null, CancellationToken cancellationToken = default);
    Task<VehicleIncidentDto?> GetByIdAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleIncidentDto>> GetByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<PlateCheckResultDto> CheckPlateAsync(string plateNumber, int? branchId = null, CancellationToken cancellationToken = default);
    Task<VehicleIncidentDto> CreateAsync(SaveVehicleIncidentDto dto, CancellationToken cancellationToken = default);
    Task<VehicleIncidentDto?> UpdateAsync(Guid incidentId, SaveVehicleIncidentDto dto, CancellationToken cancellationToken = default);
    Task<bool> ResolveAsync(Guid incidentId, ResolveIncidentDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid incidentId, CancellationToken cancellationToken = default);
}
