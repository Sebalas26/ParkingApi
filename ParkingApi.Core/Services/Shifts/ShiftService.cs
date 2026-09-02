using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Shifts;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Shifts;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Shifts;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Shifts;

public class ShiftService : IShiftService
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(
        IShiftRepository shiftRepository,
        IBranchRepository branchRepository,
        ICurrentUserService currentUser,
        ILogger<ShiftService> logger)
    {
        _shiftRepository = shiftRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<WorkShiftDto?> OpenShiftAsync(int userId, string operatorName, OpenShiftRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!dto.BranchId.HasValue || dto.BranchId.Value <= 0)
            {
                throw new InvalidOperationException("La sede (BranchId) es obligatoria para la apertura del turno de caja.");
            }

            // Resolver CompanyId mediante cascada estricta (DTO -> Claim JWT -> Sede relacional)
            int? resolvedCompanyId = dto.CompanyId.HasValue && dto.CompanyId.Value > 0 ? dto.CompanyId.Value : null;

            if (!resolvedCompanyId.HasValue && _currentUser != null)
            {
                resolvedCompanyId = _currentUser.GetEffectiveCompanyId(dto.CompanyId);
            }

            if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
            {
                var branch = await _branchRepository.GetByIdAsync(dto.BranchId.Value, cancellationToken);
                if (branch != null && branch.CompanyId > 0)
                {
                    resolvedCompanyId = branch.CompanyId;
                }
            }

            if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
            {
                throw new InvalidOperationException("La empresa (CompanyId) es obligatoria para la apertura del turno de caja.");
            }

            var activeShift = await _shiftRepository.GetActiveShiftByUserIdAsync(userId, dto.BranchId, cancellationToken);
            if (activeShift != null)
            {
                return MapToDto(activeShift);
            }

            var newShift = new WorkShift
            {
                ShiftId = Guid.NewGuid(),
                CompanyId = resolvedCompanyId.Value,
                BranchId = dto.BranchId.Value,
                UserId = userId,
                OperatorName = string.IsNullOrWhiteSpace(operatorName) ? "Operador General" : operatorName,
                StartTimeUtc = DateTime.UtcNow,
                BaseAmount = dto.BaseAmount,
                Status = ShiftStatus.Open,
                Notes = dto.Notes,
                CreatedAtUtc = DateTime.UtcNow
            };

            var created = await _shiftRepository.AddAsync(newShift, cancellationToken);
            return MapToDto(created);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir turno para usuario {UserId} en sede {BranchId}", userId, dto.BranchId);
            return null;
        }
    }

    public async Task<WorkShiftDto?> GetActiveShiftAsync(int? userId, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            WorkShift? shift;
            if (userId.HasValue && userId.Value > 0)
            {
                shift = await _shiftRepository.GetActiveShiftByUserIdAsync(userId.Value, branchId, cancellationToken);
            }
            else
            {
                shift = await _shiftRepository.GetActiveShiftAsync(branchId, cancellationToken);
            }

            return shift != null ? MapToDto(shift) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener turno activo");
            return null;
        }
    }

    public async Task<ShiftSummaryDto?> GetShiftSummaryAsync(Guid shiftId, CancellationToken cancellationToken = default)
    {
        try
        {
            var shift = await _shiftRepository.GetByIdAsync(shiftId, cancellationToken);
            if (shift == null) return null;

            var endTime = shift.EndTimeUtc ?? DateTime.UtcNow;
            var (cash, card, transfer, discounts, ticketsCompleted, ticketsEntered) =
                await _shiftRepository.CalculateShiftMetricsAsync(shift.StartTimeUtc, endTime, shift.BranchId, cancellationToken);

            var expectedCash = shift.BaseAmount + cash;
            var difference = shift.ActualCashCounted - expectedCash;

            return new ShiftSummaryDto
            {
                ShiftId = shift.ShiftId,
                BranchId = shift.BranchId,
                UserId = shift.UserId,
                OperatorName = shift.OperatorName,
                StartTimeUtc = shift.StartTimeUtc,
                EndTimeUtc = shift.EndTimeUtc,
                BaseAmount = shift.BaseAmount,
                TotalCashCollected = cash,
                TotalCardCollected = card,
                TotalTransferCollected = transfer,
                TotalDiscounts = discounts,
                ExpectedCash = expectedCash,
                ActualCashCounted = shift.ActualCashCounted,
                CashDifference = difference,
                TotalTicketsProcessed = ticketsCompleted,
                TotalVehiclesEntered = ticketsEntered,
                Status = shift.Status,
                Notes = shift.Notes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular resumen de turno {ShiftId}", shiftId);
            return null;
        }
    }

    public async Task<WorkShiftDto?> CloseShiftAsync(int userId, CloseShiftRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var shift = await _shiftRepository.GetByIdAsync(dto.ShiftId, cancellationToken);
            if (shift == null || shift.Status == ShiftStatus.Closed)
            {
                return null;
            }

            var endTime = DateTime.UtcNow;
            var (cash, card, transfer, discounts, ticketsCompleted, ticketsEntered) =
                await _shiftRepository.CalculateShiftMetricsAsync(shift.StartTimeUtc, endTime, shift.BranchId, cancellationToken);

            var expectedCash = shift.BaseAmount + cash;
            var difference = dto.ActualCashCounted - expectedCash;

            shift.EndTimeUtc = endTime;
            shift.ClosedAtUtc = endTime;
            shift.TotalCashCollected = cash;
            shift.TotalCardCollected = card;
            shift.TotalTransferCollected = transfer;
            shift.TotalDiscounts = discounts;
            shift.ExpectedCash = expectedCash;
            shift.ActualCashCounted = dto.ActualCashCounted;
            shift.CashDifference = difference;
            shift.TotalTicketsProcessed = ticketsCompleted;
            shift.TotalVehiclesEntered = ticketsEntered;
            shift.Status = ShiftStatus.Closed;
            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                shift.Notes = dto.Notes;
            }

            await _shiftRepository.UpdateAsync(shift, cancellationToken);
            return MapToDto(shift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar turno {ShiftId}", dto.ShiftId);
            return null;
        }
    }

    public async Task<IReadOnlyList<WorkShiftDto>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _shiftRepository.GetHistoryAsync(fromDate, toDate, branchId, cancellationToken);
            return history.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar historial de turnos");
            return new List<WorkShiftDto>();
        }
    }

    private static WorkShiftDto MapToDto(WorkShift s)
    {
        return new WorkShiftDto
        {
            ShiftId = s.ShiftId,
            CompanyId = s.CompanyId,
            BranchId = s.BranchId,
            UserId = s.UserId,
            OperatorName = s.OperatorName,
            StartTimeUtc = s.StartTimeUtc,
            EndTimeUtc = s.EndTimeUtc,
            BaseAmount = s.BaseAmount,
            TotalCashCollected = s.TotalCashCollected,
            TotalCardCollected = s.TotalCardCollected,
            TotalTransferCollected = s.TotalTransferCollected,
            TotalDiscounts = s.TotalDiscounts,
            ExpectedCash = s.ExpectedCash,
            ActualCashCounted = s.ActualCashCounted,
            CashDifference = s.CashDifference,
            TotalTicketsProcessed = s.TotalTicketsProcessed,
            TotalVehiclesEntered = s.TotalVehiclesEntered,
            Status = s.Status,
            Notes = s.Notes,
            CreatedAtUtc = s.CreatedAtUtc,
            ClosedAtUtc = s.ClosedAtUtc
        };
    }
}
