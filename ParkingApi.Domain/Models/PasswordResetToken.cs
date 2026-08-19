using System;

namespace ParkingApi.Domain.Models;

public class PasswordResetToken
{
    public Guid ResetTokenId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpirationDateUtc { get; set; }
    public bool IsUsed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
