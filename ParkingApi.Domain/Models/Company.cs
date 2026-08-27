using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingApi.Domain.Models;

public class Company : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string PlanType { get; set; } = "Basic";
    public int MaxBranches { get; set; } = 1;
    public DateTime? SubscriptionExpiresAt { get; set; }

    [NotMapped]
    public string? LogoBase64 { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<VehicleRate> VehicleRates { get; set; } = new List<VehicleRate>();
    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    public virtual ICollection<VehicleIncident> VehicleIncidents { get; set; } = new List<VehicleIncident>();
    public virtual ICollection<MonthlySubscription> MonthlySubscriptions { get; set; } = new List<MonthlySubscription>();
    public virtual ICollection<BillingResolution> BillingResolutions { get; set; } = new List<BillingResolution>();
    public virtual ICollection<ParkingTicket> ParkingTickets { get; set; } = new List<ParkingTicket>();
    public virtual ICollection<WorkShift> WorkShifts { get; set; } = new List<WorkShift>();
}
