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
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetAllAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<ParkingTicket> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<int> CountTodayCompletedAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<int> CountTodayTotalAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTicketsByRangeAsync(DateTime fromUtc, DateTime toUtc, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTodayRevenueAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default);
}
