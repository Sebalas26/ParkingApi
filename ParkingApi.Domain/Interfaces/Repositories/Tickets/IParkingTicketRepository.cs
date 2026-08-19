using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Tickets;

public interface IParkingTicketRepository : IBaseRepository<ParkingTicket>
{
    Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
