using System;

namespace ParkingApi.Domain.Dtos.Modules;

public class GetModuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateModuleDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
