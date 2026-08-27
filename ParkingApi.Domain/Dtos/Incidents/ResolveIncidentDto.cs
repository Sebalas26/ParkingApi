using System;

namespace ParkingApi.Domain.Dtos.Incidents;

public class ResolveIncidentDto
{
    public string ResolvedNotes { get; set; } = string.Empty;
}
