using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Tickets;

public interface IParkingTicketRepository
{
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(int? branchId = null, int? companyId = null, int offsetMinutes = 300, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(int? branchId, int? companyId, CancellationToken cancellationToken)
        => GetTodayCompletedTicketsAsync(branchId, companyId, 300, cancellationToken);
    Task<IReadOnlyList<ParkingTicket>> GetRecentCompletedTicketsAsync(int? branchId = null, int? companyId = null, int hours = 48, int limit = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetAllAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<ParkingTicket> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<int> CountTodayCompletedAsync(int? branchId = null, int? companyId = null, int offsetMinutes = 300, CancellationToken cancellationToken = default);
    Task<int> CountTodayCompletedAsync(int? branchId, int? companyId, CancellationToken cancellationToken)
        => CountTodayCompletedAsync(branchId, companyId, 300, cancellationToken);
    Task<int> CountTodayTotalAsync(int? branchId = null, int? companyId = null, int offsetMinutes = 300, CancellationToken cancellationToken = default);
    Task<int> CountTodayTotalAsync(int? branchId, int? companyId, CancellationToken cancellationToken)
        => CountTodayTotalAsync(branchId, companyId, 300, cancellationToken);
    Task<IReadOnlyList<ParkingTicket>> GetTicketsByRangeAsync(DateTime fromUtc, DateTime toUtc, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTodayRevenueAsync(int? branchId = null, int? companyId = null, int offsetMinutes = 300, CancellationToken cancellationToken = default);
    Task<decimal> GetTodayRevenueAsync(int? branchId, int? companyId, CancellationToken cancellationToken)
        => GetTodayRevenueAsync(branchId, companyId, 300, cancellationToken);
}
