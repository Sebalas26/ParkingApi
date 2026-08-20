using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Discounts;

public class TicketDiscountRepository : ITicketDiscountRepository
{
    private readonly DataContext _context;
    private readonly ILogger<TicketDiscountRepository> _logger;

    public TicketDiscountRepository(DataContext context, ILogger<TicketDiscountRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TicketDiscount>> GetByTicketIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.TicketDiscounts
                .AsNoTracking()
                .Include(d => d.Store)
                .Include(d => d.Agreement)
                .Where(d => d.TicketId == ticketId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar descuentos del tiquete {TicketId}", ticketId);
            return new List<TicketDiscount>();
        }
    }

    public async Task<TicketDiscount> AddAsync(TicketDiscount discount, CancellationToken cancellationToken = default)
    {
        try
        {
            discount.ValidatedAtUtc = DateTime.UtcNow;
            await _context.TicketDiscounts.AddAsync(discount, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return discount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar descuento en tiquete");
            throw;
        }
    }
}
