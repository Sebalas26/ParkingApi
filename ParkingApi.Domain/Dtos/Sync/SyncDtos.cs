using System;
using System.Collections.Generic;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Dtos.Sync;

public class BootstrapSyncDto
{
    public DateTime ServerTimeUtc { get; set; } = DateTime.UtcNow;
    public int TotalCapacity { get; set; } = 120;
    public List<User> Users { get; set; } = new();
    public List<VehicleRate> Rates { get; set; } = new();
    public List<Store> Stores { get; set; } = new();
    public List<CommercialAgreement> Agreements { get; set; } = new();
    public List<ParkingTicket> ActiveTickets { get; set; } = new();
    public List<ParkingTicket> RecentTickets { get; set; } = new();
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
