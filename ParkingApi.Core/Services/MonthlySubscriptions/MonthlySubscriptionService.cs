using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.MonthlySubscriptions;

public class MonthlySubscriptionService : IMonthlySubscriptionService
{
    private readonly IMonthlySubscriptionRepository _repository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserService _currentUser;

    public MonthlySubscriptionService(
        IMonthlySubscriptionRepository repository,
        IBranchRepository branchRepository,
        ICurrentUserService currentUser)
    {
        _repository = repository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MonthlySubscriptionDto>> GetAllAsync(int? companyId = null, int? branchId = null, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(companyId, branchId, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<MonthlySubscriptionDto>> GetActiveAsync(int? companyId = null, int? branchId = null, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetActiveAsync(companyId, branchId, cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<MonthlySubscriptionDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(subscriptionId, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<MonthlySubscriptionDto?> GetActiveByPlateAsync(string plateNumber, int? companyId = null, int? branchId = null, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetActiveByPlateAsync(plateNumber, companyId, branchId, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<MonthlySubscriptionDto> CreateAsync(CreateMonthlySubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.BranchId.HasValue || dto.BranchId.Value <= 0)
        {
            throw new InvalidOperationException("La sede (BranchId) es obligatoria para registrar la mensualidad.");
        }

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
            throw new InvalidOperationException("La empresa (CompanyId) es obligatoria para registrar la mensualidad.");
        }

        var entity = new MonthlySubscription
        {
            SubscriptionId = Guid.NewGuid(),
            CompanyId = resolvedCompanyId.Value,
            BranchId = dto.BranchId.Value,
            CustomerName = dto.CustomerName.Trim(),
            CustomerDocument = dto.CustomerDocument.Trim(),
            CustomerPhone = dto.CustomerPhone.Trim(),
            CustomerEmail = dto.CustomerEmail?.Trim(),
            PlateNumber = dto.PlateNumber.Trim().ToUpperInvariant(),
            VehicleType = dto.VehicleType,
            StartDateUtc = dto.StartDateUtc,
            EndDateUtc = dto.EndDateUtc,
            MonthlyFee = dto.MonthlyFee,
            AmountPaid = dto.AmountPaid,
            PaymentMethod = dto.PaymentMethod,
            IsActive = true,
            Notes = dto.Notes
        };

        var saved = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(saved);
    }

    public async Task<MonthlySubscriptionDto?> RenewAsync(Guid subscriptionId, RenewSubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(subscriptionId, cancellationToken);
        if (entity == null) return null;

        var baseDate = entity.EndDateUtc > DateTime.UtcNow ? entity.EndDateUtc : DateTime.UtcNow;
        entity.EndDateUtc = baseDate.AddMonths(dto.AdditionalMonths > 0 ? dto.AdditionalMonths : 1);
        entity.AmountPaid += dto.AmountPaid;
        entity.PaymentMethod = dto.PaymentMethod;
        entity.IsActive = true;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? dto.Notes
                : $"{entity.Notes} | Renovación: {dto.Notes}";
        }

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return MapToDto(updated);
    }

    public async Task<bool> CancelAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(subscriptionId, cancellationToken);
    }

    private static MonthlySubscriptionDto MapToDto(MonthlySubscription s)
    {
        return new MonthlySubscriptionDto
        {
            SubscriptionId = s.SubscriptionId,
            CompanyId = s.CompanyId,
            BranchId = s.BranchId,
            CustomerName = s.CustomerName,
            CustomerDocument = s.CustomerDocument,
            CustomerPhone = s.CustomerPhone,
            CustomerEmail = s.CustomerEmail,
            PlateNumber = s.PlateNumber,
            VehicleType = s.VehicleType,
            StartDateUtc = s.StartDateUtc,
            EndDateUtc = s.EndDateUtc,
            MonthlyFee = s.MonthlyFee,
            AmountPaid = s.AmountPaid,
            PaymentMethod = s.PaymentMethod,
            IsActive = s.IsActive,
            Notes = s.Notes
        };
    }
}
