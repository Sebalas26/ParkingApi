using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Billing;

public class BillingResolutionRepository : IBillingResolutionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<BillingResolutionRepository> _logger;

    public BillingResolutionRepository(DataContext context, ILogger<BillingResolutionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BillingResolution>> GetAllAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.BillingResolutions
                .AsNoTracking()
                .Include(r => r.Branch)
                .AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(r => r.BranchId == branchId || r.BranchId == null);
            }

            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(r => r.CompanyId == companyId.Value || r.CompanyId == null);
            }

            return await query
                .OrderByDescending(r => r.IsActive)
                .ThenBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resoluciones de facturación");
            return new List<BillingResolution>();
        }
    }

    public async Task<IReadOnlyList<BillingResolution>> GetActiveAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.BillingResolutions
                .AsNoTracking()
                .Include(r => r.Branch)
                .Where(r => r.IsActive);

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(r => r.BranchId == branchId || r.BranchId == null);
            }

            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(r => r.CompanyId == companyId.Value || r.CompanyId == null);
            }

            return await query
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resoluciones activas");
            return new List<BillingResolution>();
        }
    }

    public async Task<BillingResolution?> GetByIdAsync(Guid resolutionId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.BillingResolutions
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.ResolutionId == resolutionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resolución {Id}", resolutionId);
            return null;
        }
    }

    public async Task<BillingResolution?> GetByPrefixAndNumberAsync(string prefix, string resolutionNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.BillingResolutions
                .Include(r => r.Branch)
                .FirstOrDefaultAsync(r => r.Prefix == prefix && r.ResolutionNumber == resolutionNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar resolución por prefijo {Prefix} y número {Number}", prefix, resolutionNumber);
            return null;
        }
    }

    public async Task<BillingResolution> AddAsync(BillingResolution resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            resolution.CreatedAtUtc = DateTime.UtcNow;
            _context.BillingResolutions.Add(resolution);
            await _context.SaveChangesAsync(cancellationToken);
            return resolution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar resolución {Name}", resolution.Name);
            throw;
        }
    }

    public async Task<BillingResolution?> UpdateAsync(BillingResolution resolution, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.BillingResolutions.FindAsync(new object[] { resolution.ResolutionId }, cancellationToken);
            if (existing == null) return null;

            existing.BranchId = resolution.BranchId;
            existing.Name = resolution.Name;
            existing.DocumentType = resolution.DocumentType;
            existing.Prefix = resolution.Prefix;
            existing.ResolutionNumber = resolution.ResolutionNumber;
            existing.FromNumber = resolution.FromNumber;
            existing.ToNumber = resolution.ToNumber;
            existing.CurrentNumber = resolution.CurrentNumber;
            existing.ValidFrom = resolution.ValidFrom;
            existing.ValidTo = resolution.ValidTo;
            existing.TechnicalKey = resolution.TechnicalKey;
            existing.IsActive = resolution.IsActive;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar resolución {Id}", resolution.ResolutionId);
            throw;
        }
    }

    public async Task<bool> DeactivateAsync(Guid resolutionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.BillingResolutions.FindAsync(new object[] { resolutionId }, cancellationToken);
            if (existing == null) return false;

            existing.IsActive = false;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar resolución {Id}", resolutionId);
            return false;
        }
    }
}
