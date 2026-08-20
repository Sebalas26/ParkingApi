using System;
using System.Collections.Generic;
using ParkingApi.Domain.Dtos.Modules;
using ParkingApi.Domain.Dtos.RoleActions;
using ParkingApi.Domain.Dtos.UserRoles;

namespace ParkingApi.Domain.Dtos.UserRoleModules;

public class GetUserRoleModuleDto
{
    public int Id { get; set; }
    public GetUserRoleDto Role { get; set; } = null!;
    public GetModuleDto Module { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ResponsibleUser { get; set; } = string.Empty;
}

public class SaveUserRoleModuleDto
{
    public List<ActionsRoleDto> Actions { get; set; } = new();
    public bool IsActive { get; set; }
    public int ModulesRoleId { get; set; }
    public int UserRoleId { get; set; }
}
