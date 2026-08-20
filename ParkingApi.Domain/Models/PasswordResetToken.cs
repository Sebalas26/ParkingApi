using System;

namespace ParkingApi.Domain.Models;

public class PasswordResetToken : GeneralEntity
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;

    public virtual User User { get; set; } = null!;
}
