using System;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Dtos.Operations;

namespace ParkingApi.Domain.Dtos.Actions;

public class GetActionsDto
{
    public int Id { get; set; }
    public GetModuleDto Module { get; set; } = null!;
    public GetOperationDto Operation { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ResponsibleUser { get; set; } = string.Empty;
}
