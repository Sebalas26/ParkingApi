using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingApi.Domain.Models;

public class Branch : GeneralEntity
{
    public int CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public int TotalCapacity { get; set; } = 100;
    public string? Notes { get; set; }

    public virtual Company Company { get; set; } = null!;

    public string? LogoBase64 { get; set; }
    public int PaperWidth { get; set; } = 80;
    public decimal DefaultInitialCash { get; set; } = 0m;
    public bool AllowChargeByMinute { get; set; } = true;
    public bool AllowChargeByHour { get; set; } = true;
    public bool AllowChargeByDay { get; set; } = true;
    public bool AllowChargeByNight { get; set; } = false;

    public decimal LostTicketFee { get; set; } = 0m;
    public int? FullDayThresholdMinutes { get; set; } = 180;
    public string? FullDayApplicableDays { get; set; } = "1,2,3,4,5,6,0";
    public System.TimeSpan? FullDayStartTime { get; set; }
    public System.TimeSpan? FullDayEndTime { get; set; }
    public string? FullDayRulesJson { get; set; }
    public string? NightApplicableDays { get; set; } = "1,2,3,4,5,6,0";
    public System.TimeSpan? NightStartTime { get; set; } = new System.TimeSpan(18, 0, 0);
    public System.TimeSpan? NightEndTime { get; set; } = new System.TimeSpan(6, 0, 0);
    public int? NightStayMinMinutes { get; set; } = 240;
    public int EntryGracePeriodMinutes { get; set; } = 0;
    public int ExitGracePeriodMinutes { get; set; } = 0;

    public virtual ICollection<BranchOperatingHour> OperatingHours { get; set; } = new List<BranchOperatingHour>();
    public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    public virtual ICollection<BranchPaymentMethod> BranchPaymentMethods { get; set; } = new List<BranchPaymentMethod>();
    public virtual ICollection<VehicleRate> VehicleRates { get; set; } = new List<VehicleRate>();
    public virtual ICollection<ParkingTicket> ParkingTickets { get; set; } = new List<ParkingTicket>();
    public virtual ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();
    public virtual ICollection<BranchCommercialAgreement> BranchCommercialAgreements { get; set; } = new List<BranchCommercialAgreement>();
}
