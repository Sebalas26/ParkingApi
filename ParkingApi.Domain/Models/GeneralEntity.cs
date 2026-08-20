using System;

namespace ParkingApi.Domain.Models;

public class GeneralEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? ResponsibleUserId { get; set; }
    public virtual User? ResponsibleUserIdNavigation { get; set; }
}
