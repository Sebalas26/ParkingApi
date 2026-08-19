using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Agreements;

public class CommercialAgreementService : IAgreementService
{
    private readonly IAgreementRepository _agreementRepository;
    private readonly ILogger<CommercialAgreementService> _logger;

    public CommercialAgreementService(IAgreementRepository agreementRepository, ILogger<CommercialAgreementService> logger)
    {
        _agreementRepository = agreementRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommercialAgreementDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenios.");
            return new List<CommercialAgreementDto>();
        }
    }

    public async Task<IReadOnlyList<CommercialAgreementDto>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenios por comercio: {StoreId}", storeId);
            return new List<CommercialAgreementDto>();
        }
    }

    public async Task<CommercialAgreementDto?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenio por ID: {AgreementId}", agreementId);
            return null;
        }
    }

    public async Task<CommercialAgreementDto> CreateAsync(CreateAgreementDto dto, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear convenio comercial.");
            throw;
        }
    }

    public async Task<CommercialAgreementDto?> UpdateAsync(Guid agreementId, UpdateAgreementDto dto, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar convenio: {AgreementId}", agreementId);
            return null;
        }
    }

    public async Task<bool> DeleteAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        try
        {
            var agreement = await _agreementRepository.GetByIdAsync(agreementId, cancellationToken);
            if (agreement == null) return false;

            agreement.IsActive = false;
            return await _agreementRepository.UpdateAsync(agreement, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar convenio: {AgreementId}", agreementId);
            return false;
        }
    }
}
