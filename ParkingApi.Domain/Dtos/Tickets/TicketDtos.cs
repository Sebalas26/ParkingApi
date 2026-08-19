using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Tickets;

public class CheckInRequestDto
{
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Notes { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public DateTime? EntryTimeUtc { get; set; }
}

public class CheckOutRequestDto
{
    public Guid TicketId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? AgreementId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? PurchaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
}

public class TicketDto
{
    public Guid TicketId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime EntryTimeUtc { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
    public int TotalDurationMinutes { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal ChangeGiven { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TicketStatus Status { get; set; }
    public string OperatorName { get; set; } = string.Empty;
}
