using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.MonthlySubscriptions;

public class MonthlySubscriptionDto
{
    public Guid SubscriptionId { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime EndDateUtc { get; set; }
    public decimal MonthlyFee { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public bool IsCurrentlyValid => IsActive && EndDateUtc >= DateTime.UtcNow;
}

public class CreateMonthlySubscriptionDto
{
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerDocument { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; } = VehicleType.Car;
    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime EndDateUtc { get; set; } = DateTime.UtcNow.AddMonths(1);
    public decimal MonthlyFee { get; set; }
    public decimal AmountPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? Notes { get; set; }
}

public class RenewSubscriptionDto
{
    public int AdditionalMonths { get; set; } = 1;
    public decimal AmountPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? Notes { get; set; }
}
