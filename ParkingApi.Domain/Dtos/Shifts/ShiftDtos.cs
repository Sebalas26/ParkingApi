using System;
using ParkingApi.Domain.Common.Enums;

namespace ParkingApi.Domain.Dtos.Shifts;

public class OpenShiftRequestDto
{
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
    public int? UserId { get; set; }
    public string? CashRegisterName { get; set; }
    public decimal BaseAmount { get; set; } = 0m;
    public string? Notes { get; set; }
}

public class CloseShiftRequestDto
{
    public Guid ShiftId { get; set; }
    public decimal ActualCashCounted { get; set; }
    public string? Notes { get; set; }
}

public class ShiftSummaryDto
{
    public Guid ShiftId { get; set; }
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
    public int UserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = "Caja Principal";
    public DateTime StartTimeUtc { get; set; }
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
    public ShiftStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class WorkShiftDto
{
    public Guid ShiftId { get; set; }
    public int? BranchId { get; set; }
    public int? CompanyId { get; set; }
    public int UserId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = "Caja Principal";
    public DateTime StartTimeUtc { get; set; }
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
    public ShiftStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
}
