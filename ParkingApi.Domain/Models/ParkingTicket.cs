using System;
using System.Collections.Generic;
using ParkingApi.Domain.Common.Enums;
using PaymentMethodEnum = ParkingApi.Domain.Common.Enums.PaymentMethod;

namespace ParkingApi.Domain.Models;

public class ParkingTicket
{
    public Guid TicketId { get; set; } = Guid.NewGuid();
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
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
    public PaymentMethodEnum? PaymentMethod { get; set; }
    /// <summary>ID real del catálogo maestro de medios de pago (tabla PaymentMethods). Tiene prioridad sobre el enum PaymentMethod para analytics.</summary>
    public int? PaymentMethodId { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Active;
    public string OperatorName { get; set; } = "Operador General";
    public bool IsSynchronized { get; set; } = true;
    public Guid? ResolutionId { get; set; }
    public string? ResolutionName { get; set; }
    public string? InvoiceNumber { get; set; }
    public bool IsElectronicInvoice { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<TicketDiscount> Discounts { get; set; } = new List<TicketDiscount>();
}
