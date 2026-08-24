using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class ParkingLot : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMainImage { get; set; } = false;

    public virtual ICollection<UserParking> UserParkings { get; set; } = new List<UserParking>();
}
