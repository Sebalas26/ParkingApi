namespace ParkingApi.Domain.Models;

public class BranchPaymentMethod : GeneralEntity
{
    public int BranchId { get; set; }
    public int PaymentMethodId { get; set; }
    public bool RequiresCashTender { get; set; }

    public virtual Branch Branch { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
