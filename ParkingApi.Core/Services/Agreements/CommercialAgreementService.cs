using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Services.Agreements;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Agreements;

public class CommercialAgreementService : ICommercialAgreementService
{
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly ILogger<CommercialAgreementService> _logger;

    public CommercialAgreementService(ICommercialAgreementRepository agreementRepository, ILogger<CommercialAgreementService> logger)
    {
        _agreementRepository = agreementRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _agreementRepository.GetAllAsync(companyId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar convenios comerciales", Constants.AgreementError);
            return new List<CommercialAgreement>();
        }
    }

    public async Task<CommercialAgreement?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _agreementRepository.GetByIdAsync(agreementId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar convenio comercial {AgreementId}", Constants.AgreementError, agreementId);
            return null;
        }
    }

    public async Task<CommercialAgreement> CreateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _agreementRepository.AddAsync(agreement, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al crear convenio comercial", Constants.AgreementError);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _agreementRepository.UpdateAsync(agreement, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar convenio comercial {AgreementId}", Constants.AgreementError, agreement.AgreementId);
            return false;
        }
    }
}
