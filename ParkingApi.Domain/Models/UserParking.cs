using System;

namespace ParkingApi.Domain.Models;

public class UserParking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ParkingLotId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual ParkingLot ParkingLot { get; set; } = null!;
}
