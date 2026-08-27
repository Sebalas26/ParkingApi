using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Incidents;

public interface IVehicleIncidentRepository
{
    Task<IReadOnlyList<VehicleIncident>> GetAllAsync(int? branchId = null, string? status = null, bool? isBlocked = null, string? search = null, CancellationToken cancellationToken = default);
    Task<VehicleIncident?> GetByIdAsync(Guid incidentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleIncident>> GetByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<VehicleIncident?> GetActiveBlockByPlateAsync(string plateNumber, int? branchId = null, CancellationToken cancellationToken = default);
    Task<VehicleIncident> AddAsync(VehicleIncident incident, CancellationToken cancellationToken = default);
    Task<VehicleIncident?> UpdateAsync(VehicleIncident incident, CancellationToken cancellationToken = default);
    Task<bool> ResolveAsync(Guid incidentId, string resolvedNotes, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid incidentId, CancellationToken cancellationToken = default);
}
