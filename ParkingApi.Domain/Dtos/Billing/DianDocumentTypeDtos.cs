using System;

namespace ParkingApi.Domain.Dtos.Billing;

public class DianDocumentTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DefaultPrefix { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresTechnicalKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CreateDianDocumentTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DefaultPrefix { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresTechnicalKey { get; set; } = false;
}

public class UpdateDianDocumentTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DefaultPrefix { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresTechnicalKey { get; set; }
    public bool IsActive { get; set; }
}
