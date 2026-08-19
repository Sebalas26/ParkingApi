using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Agreements;

public class CommercialAgreementService : IAgreementService
{
    private readonly IAgreementRepository _agreementRepository;

    public CommercialAgreementService(IAgreementRepository agreementRepository)
    {
        _agreementRepository = agreementRepository;
    }

    public async Task<IReadOnlyList<CommercialAgreementDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
        return agreements.Select(a => new CommercialAgreementDto
        {
            AgreementId = a.AgreementId,
            StoreId = a.StoreId,
            StoreName = a.Store?.Name ?? string.Empty,
            Name = a.Name,
            MinPurchaseAmount = a.MinPurchaseAmount,
            DiscountPercentage = a.DiscountPercentage,
            DiscountFixedAmount = a.DiscountFixedAmount,
            MaxHoursApplicable = a.MaxHoursApplicable,
            IsActive = a.IsActive
        }).ToList();
    }

    public async Task<IReadOnlyList<CommercialAgreementDto>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        var agreements = await _agreementRepository.GetByStoreIdAsync(storeId, cancellationToken);
        return agreements.Select(a => new CommercialAgreementDto
        {
            AgreementId = a.AgreementId,
            StoreId = a.StoreId,
            StoreName = a.Store?.Name ?? string.Empty,
            Name = a.Name,
            MinPurchaseAmount = a.MinPurchaseAmount,
            DiscountPercentage = a.DiscountPercentage,
            DiscountFixedAmount = a.DiscountFixedAmount,
            MaxHoursApplicable = a.MaxHoursApplicable,
            IsActive = a.IsActive
        }).ToList();
    }

    public async Task<CommercialAgreementDto?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        var a = await _agreementRepository.GetByIdAsync(agreementId, cancellationToken);
        if (a == null) return null;

        return new CommercialAgreementDto
        {
            AgreementId = a.AgreementId,
            StoreId = a.StoreId,
            StoreName = a.Store?.Name ?? string.Empty,
            Name = a.Name,
            MinPurchaseAmount = a.MinPurchaseAmount,
            DiscountPercentage = a.DiscountPercentage,
            DiscountFixedAmount = a.DiscountFixedAmount,
            MaxHoursApplicable = a.MaxHoursApplicable,
            IsActive = a.IsActive
        };
    }

    public async Task<CommercialAgreementDto> CreateAsync(CreateAgreementDto dto, CancellationToken cancellationToken = default)
    {
        var agreement = new CommercialAgreement
        {
            AgreementId = Guid.NewGuid(),
            StoreId = dto.StoreId,
            Name = dto.Name.Trim(),
            MinPurchaseAmount = dto.MinPurchaseAmount,
            DiscountPercentage = dto.DiscountPercentage,
            DiscountFixedAmount = dto.DiscountFixedAmount,
            MaxHoursApplicable = dto.MaxHoursApplicable,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _agreementRepository.AddAsync(agreement, cancellationToken);

        return new CommercialAgreementDto
        {
            AgreementId = agreement.AgreementId,
            StoreId = agreement.StoreId,
            Name = agreement.Name,
            MinPurchaseAmount = agreement.MinPurchaseAmount,
            DiscountPercentage = agreement.DiscountPercentage,
            DiscountFixedAmount = agreement.DiscountFixedAmount,
            MaxHoursApplicable = agreement.MaxHoursApplicable,
            IsActive = agreement.IsActive
        };
    }

    public async Task<CommercialAgreementDto?> UpdateAsync(Guid agreementId, UpdateAgreementDto dto, CancellationToken cancellationToken = default)
    {
        var agreement = await _agreementRepository.GetByIdAsync(agreementId, cancellationToken);
        if (agreement == null) return null;

        agreement.Name = dto.Name.Trim();
        agreement.MinPurchaseAmount = dto.MinPurchaseAmount;
        agreement.DiscountPercentage = dto.DiscountPercentage;
        agreement.DiscountFixedAmount = dto.DiscountFixedAmount;
        agreement.MaxHoursApplicable = dto.MaxHoursApplicable;
        agreement.IsActive = dto.IsActive;

        await _agreementRepository.UpdateAsync(agreement, cancellationToken);

        return new CommercialAgreementDto
        {
            AgreementId = agreement.AgreementId,
            StoreId = agreement.StoreId,
            Name = agreement.Name,
            MinPurchaseAmount = agreement.MinPurchaseAmount,
            DiscountPercentage = agreement.DiscountPercentage,
            DiscountFixedAmount = agreement.DiscountFixedAmount,
            MaxHoursApplicable = agreement.MaxHoursApplicable,
            IsActive = agreement.IsActive
        };
    }

    public async Task<bool> DeleteAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        var agreement = await _agreementRepository.GetByIdAsync(agreementId, cancellationToken);
        if (agreement == null) return false;

        agreement.IsActive = false;
        await _agreementRepository.UpdateAsync(agreement, cancellationToken);
        return true;
    }
}
