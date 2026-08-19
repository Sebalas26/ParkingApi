using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Tickets;

public sealed class ParkingTicketRepository : IParkingTicketRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ParkingTicketRepository> _logger;

    public ParkingTicketRepository(DataContext context, ILogger<ParkingTicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                    .ThenInclude(d => d.Store)
                .Include(t => t.Discounts)
                    .ThenInclude(d => d.Agreement)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar tiquete por ID: {TicketId}", ticketId);
            return null;
        }
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar tiquete por cÃ³digo: {TicketNumber}", ticketNumber);
            return null;
        }
    }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalized = plateNumber.Trim().ToUpperInvariant();
            return await _context.ParkingTickets
                .FirstOrDefaultAsync(t => t.PlateNumber == normalized && t.Status == TicketStatus.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar vehÃ­culo activo por placa: {PlateNumber}", plateNumber);
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Where(t => t.Status == TicketStatus.Active)
                .OrderByDescending(t => t.EntryTimeUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar tiquetes activos.");
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ParkingTickets
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc >= today)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar transacciones completadas del dÃ­a.");
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Where(t => t.EntryTimeUtc >= start && t.EntryTimeUtc <= end)
                .OrderByDescending(t => t.EntryTimeUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tiquetes por rango de fecha.");
            return new List<ParkingTicket>();
        }
    }

    public async Task<bool> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.ParkingTickets.AddAsync(ticket, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar tiquete: {TicketNumber}", ticket.TicketNumber);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(ParkingTicket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.ParkingTickets.Update(ticket);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tiquete: {TicketId}", ticket.TicketId);
            return false;
        }
    }
}
