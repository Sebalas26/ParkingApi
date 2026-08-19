using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Agreements;

public sealed class AgreementRepository : IAgreementRepository
{
    private readonly DataContext _context;
    private readonly ILogger<AgreementRepository> _logger;

    public AgreementRepository(DataContext context, ILogger<AgreementRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.CommercialAgreements
                .Include(a => a.Store)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar convenios comerciales.");
            return new List<CommercialAgreement>();
        }
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.CommercialAgreements
                .Where(a => a.StoreId == storeId && a.IsActive)
                .Include(a => a.Store)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar convenios por comercio: {StoreId}", storeId);
            return new List<CommercialAgreement>();
        }
    }

    public async Task<CommercialAgreement?> GetByIdAsync(Guid agreementId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.CommercialAgreements
                .Include(a => a.Store)
                .FirstOrDefaultAsync(a => a.AgreementId == agreementId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar convenio por ID: {AgreementId}", agreementId);
            return null;
        }
    }

    public async Task<bool> AddAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.CommercialAgreements.AddAsync(agreement, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear convenio comercial.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.CommercialAgreements.Update(agreement);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar convenio comercial: {AgreementId}", agreement.AgreementId);
            return false;
        }
    }
}
