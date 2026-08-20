using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Discounts;

public interface ITicketDiscountRepository
{
    Task<IReadOnlyList<TicketDiscount>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<TicketDiscount> AddAsync(TicketDiscount discount, CancellationToken cancellationToken = default);
}
