using System;
using System.Collections.Generic;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Models;

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
