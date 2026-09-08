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
    public decimal LostTicketFee { get; set; } = 0m;
    public int FullDayThresholdMinutes { get; set; } = 720;
    public string? FullDayApplicableDays { get; set; }
    public string? FullDayStartTime { get; set; }
    public string? FullDayEndTime { get; set; }
    public string? FullDayRulesJson { get; set; }
    public string? NightApplicableDays { get; set; } = "1,2,3,4,5,6,0";
    public string? NightStartTime { get; set; } = "18:00";
    public string? NightEndTime { get; set; } = "06:00";
    public int NightStayMinMinutes { get; set; } = 360;
    public int EntryGracePeriodMinutes { get; set; } = 0;
    public int ExitGracePeriodMinutes { get; set; } = 0;
    public List<BranchOperatingHourDto> OperatingHours { get; set; } = new();
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
    public decimal LostTicketFee { get; set; } = 0m;
    public int FullDayThresholdMinutes { get; set; } = 720;
    public string? FullDayApplicableDays { get; set; }
    public string? FullDayStartTime { get; set; }
    public string? FullDayEndTime { get; set; }
    public string? FullDayRulesJson { get; set; }
    public string? NightApplicableDays { get; set; } = "1,2,3,4,5,6,0";
    public string? NightStartTime { get; set; } = "18:00";
    public string? NightEndTime { get; set; } = "06:00";
    public int NightStayMinMinutes { get; set; } = 360;
    public int? EntryGracePeriodMinutes { get; set; }
    public int? ExitGracePeriodMinutes { get; set; }
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
    public decimal LostTicketFee { get; set; } = 0m;
    public int FullDayThresholdMinutes { get; set; } = 720;
    public string? FullDayApplicableDays { get; set; }
    public string? FullDayStartTime { get; set; }
    public string? FullDayEndTime { get; set; }
    public string? FullDayRulesJson { get; set; }
    public string? NightApplicableDays { get; set; } = "1,2,3,4,5,6,0";
    public string? NightStartTime { get; set; } = "18:00";
    public string? NightEndTime { get; set; } = "06:00";
    public int NightStayMinMinutes { get; set; } = 360;
    public int? EntryGracePeriodMinutes { get; set; }
    public int? ExitGracePeriodMinutes { get; set; }
    public bool IsActive { get; set; }
}

public class BranchOperatingHourDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsOpen { get; set; } = true;
    public string OpeningTime { get; set; } = "08:00";
    public string ClosingTime { get; set; } = "22:00";
    public int BufferMinutesBefore { get; set; } = 30;
    public int BufferMinutesAfter { get; set; } = 30;
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

public class ConfigureBranchResolutionsDto
{
    public int BranchId { get; set; }
    public List<Guid> ResolutionIds { get; set; } = new();
}

