using System;
using System.Collections.Generic;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Dtos.Sync;

public class RoleActionSyncDto
{
    public int RoleId { get; set; }
    public string ActionSlug { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UserRoleSyncDto
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class BootstrapSyncDto
{
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public int TotalCapacity { get; set; } = 120;
    public List<Branch> Branches { get; set; } = new();
    public List<User> Users { get; set; } = new();
    public List<UserRoleSyncDto> UserRoles { get; set; } = new();
    public List<RoleActionSyncDto> RoleActions { get; set; } = new();
    public List<PaymentMethod> PaymentMethods { get; set; } = new();
    public List<BranchPaymentMethod> BranchPaymentMethods { get; set; } = new();
    public List<VehicleRate> Rates { get; set; } = new();
    public List<Store> Stores { get; set; } = new();
    public List<CommercialAgreement> Agreements { get; set; } = new();
    public List<WorkShift> WorkShifts { get; set; } = new();
    public List<MonthlySubscription> MonthlySubscriptions { get; set; } = new();
    public List<ParkingTicket> ActiveTickets { get; set; } = new();
    public List<ParkingTicket> RecentTickets { get; set; } = new();
    public List<VehicleIncident> Incidents { get; set; } = new();
}

public class PendingSyncBatchDto
{
    public List<ParkingTicket> PendingTickets { get; set; } = new();
    public List<TicketDiscount> PendingDiscounts { get; set; } = new();
}

public class SyncResultDto
{
    public bool Success { get; set; }
    public int SyncedTicketsCount { get; set; }
    public int SyncedDiscountsCount { get; set; }
    public string Message { get; set; } = string.Empty;
}

