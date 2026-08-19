using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.Tickets;

public interface IParkingTicketService
{
    Task<ParkingTicket> CheckInAsync(CheckInRequestDto dto, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> CheckOutAsync(CheckOutRequestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default);
}
