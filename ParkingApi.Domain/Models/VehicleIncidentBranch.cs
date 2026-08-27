using System;

namespace ParkingApi.Domain.Models;

public class VehicleIncidentBranch
{
    public Guid IncidentId { get; set; }
    public int BranchId { get; set; }

    public virtual VehicleIncident VehicleIncident { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
