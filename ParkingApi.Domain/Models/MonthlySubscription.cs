using System;
using PaymentMethodEnum = ParkingApi.Domain.Common.Enums.PaymentMethod;
using VehicleTypeEnum = ParkingApi.Domain.Common.Enums.VehicleType;

namespace ParkingApi.Domain.Models;

public class MonthlySubscription : GeneralEntity
{
    public Guid SubscriptionId { get; set; } = Guid.NewGuid();
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleTypeEnum VehicleType { get; set; } = VehicleTypeEnum.Car;
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public decimal MonthlyFee { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Cash;
    public string? Notes { get; set; }

    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
}
