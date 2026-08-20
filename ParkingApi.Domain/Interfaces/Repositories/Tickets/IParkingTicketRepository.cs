using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Tickets;

public interface IParkingTicketRepository
{
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ParkingTicket> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountTodayCompletedAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default);
}
