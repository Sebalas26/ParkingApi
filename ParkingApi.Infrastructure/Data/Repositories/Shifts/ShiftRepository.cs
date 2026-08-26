using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Shifts;
using ParkingApi.Domain.Models;
using PaymentMethodEnum = ParkingApi.Domain.Common.Enums.PaymentMethod;

namespace ParkingApi.Infrastructure.Data.Repositories.Shifts;

public class ShiftRepository : IShiftRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ShiftRepository> _logger;

    public ShiftRepository(DataContext context, ILogger<ShiftRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkShift?> GetActiveShiftByUserIdAsync(int userId, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.WorkShifts
                .Where(s => s.UserId == userId && s.Status == ShiftStatus.Open);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(s => s.BranchId == branchId.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar turno activo para usuario {UserId} en sede {BranchId}", userId, branchId);
            return null;
        }
    }

    public async Task<WorkShift?> GetActiveShiftAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.WorkShifts
                .Where(s => s.Status == ShiftStatus.Open);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(s => s.BranchId == branchId.Value);
            }

            return await query
                .OrderByDescending(s => s.StartTimeUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar turno activo global en sede {BranchId}", branchId);
            return null;
        }
    }

    public async Task<WorkShift?> GetByIdAsync(Guid shiftId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.WorkShifts
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar turno {ShiftId}", shiftId);
            return null;
        }
    }

    public async Task<IReadOnlyList<WorkShift>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.WorkShifts.AsNoTracking().AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(s => s.BranchId == branchId.Value);
            }

            if (fromDate.HasValue)
            {
                var fromUtc = fromDate.Value.Date;
                query = query.Where(s => s.StartTimeUtc >= fromUtc);
            }

            if (toDate.HasValue)
            {
                var toUtc = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.StartTimeUtc <= toUtc);
            }

            return await query
                .OrderByDescending(s => s.StartTimeUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar historial de turnos en sede {BranchId}", branchId);
            return new List<WorkShift>();
        }
    }

    public async Task<WorkShift> AddAsync(WorkShift shift, CancellationToken cancellationToken = default)
    {
        try
        {
            shift.CreatedAtUtc = DateTime.UtcNow;
            await _context.WorkShifts.AddAsync(shift, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return shift;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear turno");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(WorkShift shift, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.WorkShifts.Update(shift);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar turno {ShiftId}", shift.ShiftId);
            return false;
        }
    }

    public async Task<(decimal cash, decimal card, decimal transfer, decimal discounts, int ticketsCompleted, int ticketsEntered)> CalculateShiftMetricsAsync(
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        int? branchId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var completedQuery = _context.ParkingTickets
                .AsNoTracking()
                .Where(t => t.Status == TicketStatus.Completed && t.ExitTimeUtc >= startTimeUtc && t.ExitTimeUtc <= endTimeUtc);

            var enteredQuery = _context.ParkingTickets
                .AsNoTracking()
                .Where(t => t.EntryTimeUtc >= startTimeUtc && t.EntryTimeUtc <= endTimeUtc);

            if (branchId.HasValue && branchId.Value > 0)
            {
                completedQuery = completedQuery.Where(t => t.BranchId == branchId.Value);
                enteredQuery = enteredQuery.Where(t => t.BranchId == branchId.Value);
            }

            var completedTickets = await completedQuery.ToListAsync(cancellationToken);
            var enteredTicketsCount = await enteredQuery.CountAsync(cancellationToken);

            decimal cash = 0m;
            decimal card = 0m;
            decimal transfer = 0m;
            decimal discounts = completedTickets.Sum(t => t.DiscountAmount);

            foreach (var t in completedTickets)
            {
                if (!t.PaymentMethod.HasValue || t.PaymentMethod == PaymentMethodEnum.Cash)
                {
                    cash += t.NetAmount;
                }
                else if (t.PaymentMethod == PaymentMethodEnum.DebitCard || t.PaymentMethod == PaymentMethodEnum.CreditCard)
                {
                    card += t.NetAmount;
                }
                else if (t.PaymentMethod == PaymentMethodEnum.Transfer)
                {
                    transfer += t.NetAmount;
                }
                else
                {
                    cash += t.NetAmount;
                }
            }

            return (cash, card, transfer, discounts, completedTickets.Count, enteredTicketsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular métricas de turno en sede {BranchId}", branchId);
            return (0m, 0m, 0m, 0m, 0, 0);
        }
    }
}
