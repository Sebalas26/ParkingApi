using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Tickets;

public sealed class ParkingTicketRepository : IParkingTicketRepository
=======
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Tickets;

public class ParkingTicketRepository : IParkingTicketRepository
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
{
    private readonly DataContext _context;
    private readonly ILogger<ParkingTicketRepository> _logger;

    public ParkingTicketRepository(DataContext context, ILogger<ParkingTicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

<<<<<<< HEAD
    public async Task<ParkingTicket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
=======
    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    {
        try
        {
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
<<<<<<< HEAD
                    .ThenInclude(d => d.Store)
                .Include(t => t.Discounts)
                    .ThenInclude(d => d.Agreement)
                .FirstOrDefaultAsync(t => t.TicketId == ticketId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar tiquete por ID: {TicketId}", ticketId);
=======
                .FirstOrDefaultAsync(t => t.TicketId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete {Id}", Constants.TicketError, id);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return null;
        }
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        try
        {
<<<<<<< HEAD
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar tiquete por cÃ³digo: {TicketNumber}", ticketNumber);
=======
            var normalized = ticketNumber.Trim();
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                .FirstOrDefaultAsync(t => t.TicketNumber == normalized, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete por número {Number}", Constants.TicketError, ticketNumber);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return null;
        }
    }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalized = plateNumber.Trim().ToUpperInvariant();
            return await _context.ParkingTickets
<<<<<<< HEAD
                .FirstOrDefaultAsync(t => t.PlateNumber == normalized && t.Status == TicketStatus.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar vehÃ­culo activo por placa: {PlateNumber}", plateNumber);
=======
                .Include(t => t.Discounts)
                .FirstOrDefaultAsync(t => t.Status == TicketStatus.Active && t.PlateNumber.ToUpper() == normalized, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete activo por placa {Plate}", Constants.TicketError, plateNumber);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
<<<<<<< HEAD
                .Where(t => t.Status == TicketStatus.Active)
                .OrderByDescending(t => t.EntryTimeUtc)
                .AsNoTracking()
=======
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Where(t => t.Status == TicketStatus.Active)
                .OrderByDescending(t => t.EntryTimeUtc)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al listar tiquetes activos.");
=======
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes activos", Constants.TicketError);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ParkingTickets
<<<<<<< HEAD
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc >= today)
                .AsNoTracking()
=======
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today)
                .OrderByDescending(t => t.ExitTimeUtc)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al listar transacciones completadas del dÃ­a.");
=======
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes completados de hoy", Constants.TicketError);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return new List<ParkingTicket>();
        }
    }

<<<<<<< HEAD
    public async Task<IReadOnlyList<ParkingTicket>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Where(t => t.EntryTimeUtc >= start && t.EntryTimeUtc <= end)
                .OrderByDescending(t => t.EntryTimeUtc)
                .AsNoTracking()
=======
    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = date.Date;
            return await _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Where(t => t.EntryTimeUtc.Date == targetDate || (t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == targetDate))
                .OrderByDescending(t => t.EntryTimeUtc)
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al consultar tiquetes por rango de fecha.");
=======
            _logger.LogError(ex, "{Error}: Error al consultar historial de tiquetes para {Date}", Constants.TicketError, date);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return new List<ParkingTicket>();
        }
    }

<<<<<<< HEAD
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
=======
    public async Task<IReadOnlyList<ParkingTicket>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .OrderByDescending(t => t.EntryTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar todos los tiquetes", Constants.TicketError);
            return new List<ParkingTicket>();
        }
    }

    public async Task<ParkingTicket> AddAsync(ParkingTicket ticket, CancellationToken cancellationToken = default)
    {
        try
        {
            ticket.CreatedAtUtc = DateTime.UtcNow;
            await _context.ParkingTickets.AddAsync(ticket, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al guardar nuevo tiquete", Constants.TicketError);
            throw;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
            _logger.LogError(ex, "Error al actualizar tiquete: {TicketId}", ticket.TicketId);
            return false;
        }
=======
            _logger.LogError(ex, "{Error}: Error al actualizar tiquete {Id}", Constants.TicketError, ticket.TicketId);
            return false;
        }
    }

    public async Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .AsNoTracking()
                .CountAsync(t => t.Status == TicketStatus.Active, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al contar tiquetes activos", Constants.TicketError);
            return 0;
        }
    }

    public async Task<int> CountTodayCompletedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ParkingTickets
                .AsNoTracking()
                .CountAsync(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al contar tiquetes completados de hoy", Constants.TicketError);
            return 0;
        }
    }

    public async Task<decimal> GetTodayRevenueAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            return await _context.ParkingTickets
                .AsNoTracking()
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today)
                .SumAsync(t => t.NetAmount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al calcular ingresos de hoy", Constants.TicketError);
            return 0m;
        }
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    }
}
