using System;

namespace ParkingApi.Domain.Dtos.Incidents;

public class PlateCheckResultDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public bool HasIncidents { get; set; }
    public bool IsBlocked { get; set; }
    public string? Reason { get; set; }
    public string? IncidentType { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime? ReportedAtUtc { get; set; }
    public Guid? IncidentId { get; set; }
}
