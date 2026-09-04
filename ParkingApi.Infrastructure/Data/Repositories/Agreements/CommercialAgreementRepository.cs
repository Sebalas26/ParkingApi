using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Agreements;

public class CommercialAgreementRepository : ICommercialAgreementRepository
{
    private readonly DataContext _context;
    private readonly ILogger<CommercialAgreementRepository> _logger;

    public CommercialAgreementRepository(DataContext context, ILogger<CommercialAgreementRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CommercialAgreements.AsNoTracking();
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(a => a.Store != null && (a.Store.CompanyId == companyId.Value || a.Store.CompanyId == null));
            }
            return await query
                .Include(a => a.Store)
                .OrderBy(a => a.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar convenios", Constants.AgreementError);
            return new List<CommercialAgreement>();
        }
    }

    public async Task<CommercialAgreement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.CommercialAgreements
                .Include(a => a.Store)
                .FirstOrDefaultAsync(a => a.AgreementId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar convenio {Id}", Constants.AgreementError, id);
            return null;
        }
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetByStoreIdAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.CommercialAgreements
                .AsNoTracking()
                .Where(a => a.StoreId == storeId && a.IsActive)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar convenios del comercio {StoreId}", Constants.AgreementError, storeId);
            return new List<CommercialAgreement>();
        }
    }

    public async Task<CommercialAgreement> AddAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            agreement.CreatedAtUtc = DateTime.UtcNow;
            await _context.CommercialAgreements.AddAsync(agreement, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return agreement;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al guardar convenio", Constants.AgreementError);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.CommercialAgreements.FirstOrDefaultAsync(a => a.AgreementId == agreement.AgreementId, cancellationToken);
            if (existing == null) return false;

            existing.StoreId = agreement.StoreId;
            existing.Name = agreement.Name;
            existing.MinPurchaseAmount = agreement.MinPurchaseAmount;
            existing.DiscountPercentage = agreement.DiscountPercentage;
            existing.DiscountFixedAmount = agreement.DiscountFixedAmount;
            existing.MaxHoursApplicable = agreement.MaxHoursApplicable;
            existing.MaxMinutesApplicable = agreement.MaxMinutesApplicable;
            existing.IsActive = agreement.IsActive;
            existing.ImageUrl = agreement.ImageUrl;

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar convenio {Id}", Constants.AgreementError, agreement.AgreementId);
            return false;
        }
    }
}
