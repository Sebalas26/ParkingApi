using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services.VehicleRates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.VehicleRates;

public class VehicleRateService : IVehicleRateService
{
    private readonly IVehicleRateRepository _rateRepository;
    private readonly ILogger<VehicleRateService> _logger;

    public VehicleRateService(IVehicleRateRepository rateRepository, ILogger<VehicleRateService> logger)
    {
        _rateRepository = rateRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleRate>> GetAllRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _rateRepository.GetAllAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar todas las tarifas", Constants.RateError);
            return new List<VehicleRate>();
        }
    }

    public async Task<VehicleRate?> GetByIdAsync(Guid rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _rateRepository.GetByIdAsync(rateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tarifa {RateId}", Constants.RateError, rateId);
            return null;
        }
    }

    public async Task<VehicleRate> UpdateRateAsync(Guid rateId, decimal hourRate, decimal minuteRate, decimal fullDayRate, int graceMinutes, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _rateRepository.GetByIdAsync(rateId, cancellationToken);
            if (rate == null)
            {
                throw new KeyNotFoundException("Tarifa no encontrada.");
            }

            rate.HourRate = hourRate;
            rate.MinuteRate = minuteRate;
            rate.FullDayRate = fullDayRate;
            rate.GracePeriodMinutes = graceMinutes;
            rate.UpdatedAtUtc = DateTime.UtcNow;

            await _rateRepository.UpdateAsync(rate, cancellationToken);
            return rate;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al actualizar tarifa {RateId}", Constants.RateError, rateId);
            throw new Exception("Error interno al actualizar la tarifa.");
        }
    }
}
