using System;

namespace ParkingApi.Domain.Models;

public class UserRoleModule : GeneralEntity
{
    public int UserRoleId { get; set; }
    public int ModulesRoleId { get; set; }

    public virtual Module ModuleIdNavigation { get; set; } = null!;
    public virtual UserRole UserRoleIdNavigation { get; set; } = null!;
}
