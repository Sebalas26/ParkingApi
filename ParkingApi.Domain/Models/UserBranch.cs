namespace ParkingApi.Domain.Models;

public class UserBranch : GeneralEntity
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool IsDefault { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
}
