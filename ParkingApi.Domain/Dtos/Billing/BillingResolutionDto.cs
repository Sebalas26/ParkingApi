using System;

namespace ParkingApi.Domain.Dtos.Billing;

public class BillingResolutionDto
{
    public Guid ResolutionId { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
    public string ResolutionNumber { get; set; } = string.Empty;
    public long FromNumber { get; set; }
    public long ToNumber { get; set; }
    public long CurrentNumber { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string? TechnicalKey { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
