using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Shifts;

namespace ParkingApi.Domain.Interfaces.Services.Shifts;

public interface IShiftService
{
    Task<WorkShiftDto?> OpenShiftAsync(int userId, string operatorName, OpenShiftRequestDto dto, CancellationToken cancellationToken = default);
    Task<WorkShiftDto?> GetActiveShiftAsync(int? userId, int? branchId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkShiftDto>> GetActiveShiftsAsync(int? userId, int? branchId = null, CancellationToken cancellationToken = default);
    Task<ShiftSummaryDto?> GetShiftSummaryAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<WorkShiftDto?> CloseShiftAsync(int userId, CloseShiftRequestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkShiftDto>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate, int? branchId = null, CancellationToken cancellationToken = default);
}
