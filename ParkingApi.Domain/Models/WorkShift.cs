using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Models;

public class WorkShift
{
    public Guid ShiftId { get; set; } = Guid.NewGuid();
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
    public int UserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = "Caja Principal";
    public DateTime StartTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndTimeUtc { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TotalCashCollected { get; set; }
    public decimal TotalCardCollected { get; set; }
    public decimal TotalTransferCollected { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCashCounted { get; set; }
    public decimal CashDifference { get; set; }
    public int TotalTicketsProcessed { get; set; }
    public int TotalVehiclesEntered { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAtUtc { get; set; }

    public virtual Company? Company { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual User? User { get; set; }
}
