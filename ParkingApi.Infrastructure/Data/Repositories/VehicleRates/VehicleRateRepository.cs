using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.VehicleRates;

public class VehicleRateRepository : IVehicleRateRepository
{
    private readonly DataContext _context;
    private readonly ILogger<VehicleRateRepository> _logger;

    public VehicleRateRepository(DataContext context, ILogger<VehicleRateRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleRate>> GetAllAsync(int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.VehicleRates.AsNoTracking();
            if (companyId.HasValue && companyId.Value > 0)
            {
                query = query.Where(r => r.CompanyId == companyId.Value || r.CompanyId == null);
            }
            return await query
                .OrderBy(r => r.VehicleType)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tarifas", Constants.RateError);
            return new List<VehicleRate>();
        }
    }

    public async Task<VehicleRate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.VehicleRates
                .FirstOrDefaultAsync(r => r.RateId == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tarifa por Id {Id}", Constants.RateError, id);
            return null;
        }
    }

    public Task<VehicleRate?> GetByTypeAsync(VehicleType type, CancellationToken cancellationToken = default)
        => GetByTypeAsync(type, null, null, cancellationToken);

    public async Task<VehicleRate?> GetByTypeAsync(VehicleType type, int? branchId, int? companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.VehicleRates
                .AsNoTracking()
                .Where(r => r.VehicleType == type && r.IsActive);

            if (branchId.HasValue && branchId.Value > 0)
            {
                var branchRate = await query
                    .Where(r => r.BranchId == branchId.Value && r.HourRate > 0)
                    .FirstOrDefaultAsync(cancellationToken);
                if (branchRate != null) return branchRate;
            }

            if (companyId.HasValue && companyId.Value > 0)
            {
                var companyRate = await query
                    .Where(r => (r.CompanyId == companyId.Value || r.BranchId == null) && r.HourRate > 0)
                    .FirstOrDefaultAsync(cancellationToken);
                if (companyRate != null) return companyRate;
            }

            return await query
                .Where(r => r.HourRate > 0)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tarifa por tipo {Type} para sede {BranchId}", Constants.RateError, type, branchId);
            return null;
        }
    }

    public async Task<VehicleRate> AddAsync(VehicleRate rate, CancellationToken cancellationToken = default)
    {
        try
        {
            rate.CreatedAtUtc = DateTime.UtcNow;
            await _context.VehicleRates.AddAsync(rate, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return rate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al guardar tarifa", Constants.RateError);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(VehicleRate rate, CancellationToken cancellationToken = default)
    {
        try
        {
            rate.UpdatedAtUtc = DateTime.UtcNow;
            _context.VehicleRates.Update(rate);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar tarifa {Id}", Constants.RateError, rate.RateId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _context.VehicleRates.FirstOrDefaultAsync(r => r.RateId == id, cancellationToken);
            if (rate == null)
            {
                return false;
            }

            _context.VehicleRates.Remove(rate);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al eliminar tarifa {Id}", Constants.RateError, id);
            return false;
        }
    }
}
