using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Dtos.Branches;

public class BranchDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; } = 100;
    public string? Notes { get; set; }
    public string? LogoBase64 { get; set; }
    public int PaperWidth { get; set; } = 80;
    public decimal DefaultInitialCash { get; set; } = 0m;
    public bool AllowChargeByMinute { get; set; } = true;
    public bool AllowChargeByHour { get; set; } = true;
    public bool AllowChargeByDay { get; set; } = true;
    public bool AllowChargeByNight { get; set; } = false;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBranchDto
{
    public int? CompanyId { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; } = 100;
    public string? Notes { get; set; }
    public string? LogoBase64 { get; set; }
    public int PaperWidth { get; set; } = 80;
    public decimal DefaultInitialCash { get; set; } = 0m;
    public bool AllowChargeByMinute { get; set; } = true;
    public bool AllowChargeByHour { get; set; } = true;
    public bool AllowChargeByDay { get; set; } = true;
    public bool AllowChargeByNight { get; set; } = false;
}

public class UpdateBranchDto
{
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; }
    public string? Notes { get; set; }
    public string? LogoBase64 { get; set; }
    public int PaperWidth { get; set; } = 80;
    public decimal DefaultInitialCash { get; set; } = 0m;
    public bool AllowChargeByMinute { get; set; } = true;
    public bool AllowChargeByHour { get; set; } = true;
    public bool AllowChargeByDay { get; set; } = true;
    public bool AllowChargeByNight { get; set; } = false;
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

public class BranchAgreementDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public Guid AgreementId { get; set; }
    public string AgreementName { get; set; } = string.Empty;
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public int? MaxHoursApplicable { get; set; }
    public int? MaxMinutesApplicable { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class ConfigureBranchAgreementsDto
{
    public int BranchId { get; set; }
    public List<Guid> AgreementIds { get; set; } = new();
}
