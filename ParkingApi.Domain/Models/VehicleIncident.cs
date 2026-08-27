using System;

namespace ParkingApi.Domain.Models;

public class VehicleIncident
{
    public Guid IncidentId { get; set; } = Guid.NewGuid();
    public int? CompanyId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public bool IsBlocked { get; set; } = false;
    public bool IsGlobal { get; set; } = false;
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Status { get; set; } = "Activa";
    public string? ResolvedNotes { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual System.Collections.Generic.ICollection<VehicleIncidentBranch> IncidentBranches { get; set; } = new System.Collections.Generic.List<VehicleIncidentBranch>();
}
