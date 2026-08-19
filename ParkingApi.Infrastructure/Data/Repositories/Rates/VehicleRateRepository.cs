using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Rates;

public sealed class VehicleRateRepository : IVehicleRateRepository
{
    private readonly DataContext _context;
    private readonly ILogger<VehicleRateRepository> _logger;

    public VehicleRateRepository(DataContext context, ILogger<VehicleRateRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleRate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.VehicleRates
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar catÃ¡logo de tarifas.");
            return new List<VehicleRate>();
        }
    }

    public async Task<VehicleRate?> GetByIdAsync(Guid rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.VehicleRates.FirstOrDefaultAsync(r => r.RateId == rateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifa por ID: {RateId}", rateId);
            return null;
        }
    }

    public async Task<VehicleRate?> GetByTypeAsync(VehicleType type, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.VehicleRates.FirstOrDefaultAsync(r => r.VehicleType == type && r.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifa por tipo de vehÃ­culo: {VehicleType}", type);
            return null;
        }
    }

    public async Task<bool> AddAsync(VehicleRate rate, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.VehicleRates.AddAsync(rate, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tarifa vehicular.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(VehicleRate rate, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.VehicleRates.Update(rate);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarifa vehicular: {RateId}", rate.RateId);
            return false;
        }
    }
}
