using System;

namespace ParkingApi.Domain.Models;

public class RoleAction : GeneralEntity
{
    public int RoleId { get; set; }
    public int ActionId { get; set; }

    public virtual Action ActionIdNavigation { get; set; } = null!;
    public virtual UserRole RoleIdNavigation { get; set; } = null!;
}
