using System;

namespace ParkingApi.Domain.Models;

public class DianDocumentType : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DefaultPrefix { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresTechnicalKey { get; set; } = false;
}
