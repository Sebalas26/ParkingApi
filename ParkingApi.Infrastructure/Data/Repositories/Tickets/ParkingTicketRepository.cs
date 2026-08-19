using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Base;

namespace ParkingApi.Infrastructure.Data.Repositories.Tickets;

public class ParkingTicketRepository : BaseRepository<ParkingTicket>, IParkingTicketRepository
{
    public ParkingTicketRepository(DataContext context) : base(context) { }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Discounts)
            .FirstOrDefaultAsync(t =>
                t.Status == TicketStatus.Active &&
                t.PlateNumber.ToLower() == plateNumber.ToLower(), cancellationToken);
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Discounts)
            .FirstOrDefaultAsync(t => t.TicketNumber.ToLower() == ticketNumber.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Discounts)
            .Where(t => t.Status == TicketStatus.Active)
            .OrderByDescending(t => t.EntryTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Include(t => t.Discounts)
            .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today)
            .OrderByDescending(t => t.ExitTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Discounts)
            .Where(t => t.EntryTimeUtc >= startDate && t.EntryTimeUtc <= endDate)
            .OrderByDescending(t => t.EntryTimeUtc)
            .ToListAsync(cancellationToken);
    }
}
