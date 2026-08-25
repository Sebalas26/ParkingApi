using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Dtos.Branches;

public class BranchDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; } = 100;
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBranchDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; } = 100;
    public string? Notes { get; set; }
}

public class UpdateBranchDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class AssignUserBranchDto
{
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public bool IsDefault { get; set; }
}

public class BranchPaymentMethodDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public string? PaymentMethodIcon { get; set; }
    public bool RequiresCashTender { get; set; }
    public bool IsActive { get; set; }
}

public class ConfigureBranchPaymentMethodsDto
{
    public int BranchId { get; set; }
    public List<int> PaymentMethodIds { get; set; } = new();
}
