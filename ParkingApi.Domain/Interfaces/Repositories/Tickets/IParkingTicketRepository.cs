using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Tickets;

public interface IParkingTicketRepository
{
    Task<ParkingTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
}
