using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class IdentificationType : GeneralEntity
{
    public string Identification { get; set; } = string.Empty;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
