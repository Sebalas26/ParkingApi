using System;

namespace ParkingApi.Domain.Dtos.Incidents;

public class VehicleIncidentDto
{
    public Guid IncidentId { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string Status { get; set; } = "Activa";
    public string? ResolvedNotes { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
