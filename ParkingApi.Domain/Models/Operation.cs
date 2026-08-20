using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class Operation : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<Action> Actions { get; set; } = new List<Action>();
}
