using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class Action : GeneralEntity
{
    public int ModuleId { get; set; }
    public int OperationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public virtual Module ModuleIdNavigation { get; set; } = null!;
    public virtual Operation OperationIdNavigation { get; set; } = null!;
    public virtual ICollection<RoleAction> RoleActions { get; set; } = new List<RoleAction>();
}
