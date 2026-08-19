using System;
using System.Collections.Generic;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Models;

public class VehicleRate
{
    public Guid RateId { get; set; } = Guid.NewGuid();
    public VehicleType VehicleType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public decimal HourRate { get; set; }
    public decimal MinuteRate { get; set; }
    public decimal FullDayRate { get; set; }
    public int GracePeriodMinutes { get; set; } = 15;
    public string IconKey { get; set; } = "IconCar";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class Store
{
    public Guid StoreId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<CommercialAgreement> Agreements { get; set; } = new List<CommercialAgreement>();
    public ICollection<TicketDiscount> TicketDiscounts { get; set; } = new List<TicketDiscount>();
}

public class CommercialAgreement
{
    public Guid AgreementId { get; set; } = Guid.NewGuid();
    public Guid StoreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinPurchaseAmount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public int? MaxHoursApplicable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Store Store { get; set; } = null!;
    public ICollection<TicketDiscount> TicketDiscounts { get; set; } = new List<TicketDiscount>();
}

public class ParkingTicket
{
    public Guid TicketId { get; set; } = Guid.NewGuid();
    public string TicketNumber { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime EntryTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExitTimeUtc { get; set; }
    public int TotalDurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public string OperatorName { get; set; } = "Operador General";
    public bool IsSynchronized { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TicketDiscount> Discounts { get; set; } = new List<TicketDiscount>();
}

public class TicketDiscount
{
    public Guid TicketDiscountId { get; set; } = Guid.NewGuid();
    public Guid TicketId { get; set; }
    public Guid StoreId { get; set; }
    public Guid AgreementId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal PurchaseAmount { get; set; }
    public decimal AppliedDiscountAmount { get; set; }
    public DateTime ValidatedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsSynchronized { get; set; } = true;

    public ParkingTicket Ticket { get; set; } = null!;
    public Store Store { get; set; } = null!;
    public CommercialAgreement Agreement { get; set; } = null!;
}
