using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Shifts;

public interface IShiftRepository
{
    Task<WorkShift?> GetActiveShiftByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<WorkShift?> GetActiveShiftAsync(CancellationToken cancellationToken = default);
    Task<WorkShift?> GetByIdAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkShift>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<WorkShift> AddAsync(WorkShift shift, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(WorkShift shift, CancellationToken cancellationToken = default);
    Task<(decimal cash, decimal card, decimal transfer, decimal discounts, int ticketsCompleted, int ticketsEntered)> CalculateShiftMetricsAsync(DateTime startTimeUtc, DateTime endTimeUtc, CancellationToken cancellationToken = default);
}
