using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Tickets;

public class CheckOutRequestDto
{
    public Guid TicketId { get; set; }
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal AmountPaid { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? AgreementId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal? PurchaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
    public Guid? ResolutionId { get; set; }
    public string? ResolutionName { get; set; }
    public string? FiscalInvoiceNumber { get; set; }
}
