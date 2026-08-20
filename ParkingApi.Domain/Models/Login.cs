using System;

namespace ParkingApi.Domain.Models;

public class Login : GeneralEntity
{
    public int UserId { get; set; }
    public string Message { get; set; } = string.Empty;

    public virtual User User { get; set; } = null!;
}
