using System;

namespace ParkingApi.Domain.Dtos.UserRoles;

public class GetUserRoleDto
{
    public int IdUserRol { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserRoleDto
{
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
