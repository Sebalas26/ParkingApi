using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Tickets;

public interface IParkingTicketRepository
{
<<<<<<< HEAD
    Task<ParkingTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
=======
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default);
<<<<<<< HEAD
    Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
=======
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ParkingTicket> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<int> CountTodayCompletedAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
}
