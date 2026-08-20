using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.MonthlySubscriptions;

public class MonthlySubscriptionService : IMonthlySubscriptionService
{
    private readonly IMonthlySubscriptionRepository _repository;

    public MonthlySubscriptionService(IMonthlySubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MonthlySubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<MonthlySubscriptionDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetActiveAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<MonthlySubscriptionDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(subscriptionId, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<MonthlySubscriptionDto?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetActiveByPlateAsync(plateNumber, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<MonthlySubscriptionDto> CreateAsync(CreateMonthlySubscriptionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new MonthlySubscription
        {
            SubscriptionId = Guid.NewGuid(),
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
