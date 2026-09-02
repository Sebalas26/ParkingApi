using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Tickets;

public class ParkingTicketRepository : IParkingTicketRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ParkingTicketRepository> _logger;

    public ParkingTicketRepository(DataContext context, ILogger<ParkingTicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                    .ThenInclude(d => d.Store)
                .Include(t => t.Discounts)
                    .ThenInclude(d => d.Agreement)
                .FirstOrDefaultAsync(t => t.TicketId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete {Id}", Constants.TicketError, id);
            return null;
        }
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalized = ticketNumber.Trim();
            return await _context.ParkingTickets
                .Include(t => t.Discounts)
                .FirstOrDefaultAsync(t => t.TicketNumber == normalized, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete por número {Number}", Constants.TicketError, ticketNumber);
            return null;
        }
    }

    public async Task<ParkingTicket?> GetActiveByPlateAsync(string plateNumber, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalized = plateNumber.Trim().ToUpperInvariant();
            var query = _context.ParkingTickets
                .Include(t => t.Discounts)
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Active && t.PlateNumber.ToUpper() == normalized);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.Branch != null && t.Branch.CompanyId == companyId.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete activo por placa {Plate}", Constants.TicketError, plateNumber);
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Active);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query
                .OrderByDescending(t => t.EntryTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes activos", Constants.TicketError);
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTodayCompletedTicketsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query
                .OrderByDescending(t => t.ExitTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes completados de hoy", Constants.TicketError);
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var targetDate = date.Date;
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Include(t => t.Branch)
                .Where(t => t.EntryTimeUtc.Date == targetDate || (t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == targetDate));

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query
                .OrderByDescending(t => t.EntryTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar historial de tiquetes para {Date}", Constants.TicketError, date);
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetAllAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Discounts)
                .Include(t => t.Branch)
                .AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query
                .OrderByDescending(t => t.EntryTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar todos los tiquetes", Constants.TicketError);
            return new List<ParkingTicket>();
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetTicketsByRangeAsync(DateTime fromUtc, DateTime toUtc, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Where(t => t.EntryTimeUtc >= fromUtc && t.EntryTimeUtc <= toUtc);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.Branch != null && t.Branch.CompanyId == companyId.Value);
            }

            return await query
                .OrderBy(t => t.EntryTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes por rango {From} - {To}", Constants.TicketError, fromUtc, toUtc);
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
            _logger.LogError(ex, "{Error}: Error al actualizar tiquete {Id}", Constants.TicketError, ticket.TicketId);
            return false;
        }
    }

    public async Task<int> CountActiveAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Active);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al contar tiquetes activos", Constants.TicketError);
            return 0;
        }
    }

    public async Task<int> CountTodayCompletedAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al contar tiquetes completados de hoy", Constants.TicketError);
            return 0;
        }
    }

    public async Task<int> CountTodayTotalAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Branch)
                .Where(t => t.EntryTimeUtc.Date == today);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al contar total de tiquetes de hoy", Constants.TicketError);
            return 0;
        }
    }

    public async Task<decimal> GetTodayRevenueAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var query = _context.ParkingTickets
                .AsNoTracking()
                .Include(t => t.Branch)
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == today);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(t => t.BranchId == branchId.Value);
            }
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(t => t.CompanyId == companyId.Value || (t.Branch != null && t.Branch.CompanyId == companyId.Value));
            }

            return await query.SumAsync(t => t.NetAmount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al calcular ingresos de hoy", Constants.TicketError);
            return 0m;
        }
    }
}
