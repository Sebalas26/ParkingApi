using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class UserRole : GeneralEntity
{
    public int? CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;

    public virtual Company? Company { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<RoleAction> RoleActions { get; set; } = new List<RoleAction>();
    public virtual ICollection<UserRoleModule> UserRoleModules { get; set; } = new List<UserRoleModule>();
}
