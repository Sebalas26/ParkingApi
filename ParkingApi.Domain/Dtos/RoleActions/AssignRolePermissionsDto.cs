using System.Collections.Generic;

namespace ParkingApi.Domain.Dtos.RoleActions;

public class AssignRolePermissionsDto
{
    public int RoleId { get; set; }
    public List<int> ActionIds { get; set; } = new();
}
