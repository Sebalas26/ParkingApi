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

    public async Task<VehicleRate> CreateRateAsync(VehicleRate rate, CancellationToken cancellationToken = default)
    {
        try
        {
            if (rate.RateId == Guid.Empty)
            {
                rate.RateId = Guid.NewGuid();
            }
            rate.CreatedAtUtc = DateTime.UtcNow;
            rate.IsActive = true;
            return await _rateRepository.AddAsync(rate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al crear tarifa", Constants.RateError);
            throw new Exception("Error interno al registrar la tarifa.");
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

    public async Task<VehicleRate> UpdateRateAsync(VehicleRate input, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _rateRepository.GetByIdAsync(input.RateId, cancellationToken);
            if (rate == null)
            {
                throw new KeyNotFoundException("Tarifa no encontrada.");
            }

            rate.BranchId = input.BranchId;
            rate.VehicleType = input.VehicleType;
            rate.DisplayName = input.DisplayName;
            rate.HourRate = input.HourRate;
            rate.MinuteRate = input.MinuteRate;
            rate.FullDayRate = input.FullDayRate;
            rate.GracePeriodMinutes = input.GracePeriodMinutes;
            rate.IconKey = input.IconKey ?? rate.IconKey;
            rate.IsActive = input.IsActive;
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
            _logger.LogError(ex, "{Error}: Error al actualizar tarifa {RateId}", Constants.RateError, input.RateId);
            throw new Exception("Error interno al actualizar la tarifa.");
        }
    }

    public async Task<bool> DeleteRateAsync(Guid rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _rateRepository.DeleteAsync(rateId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al eliminar tarifa {RateId}", Constants.RateError, rateId);
            return false;
        }
    }
}
