using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Rates;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Interfaces.Services.Rates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Rates;

public class VehicleRateService : IVehicleRateService
{
    private readonly IVehicleRateRepository _rateRepository;
    private readonly ILogger<VehicleRateService> _logger;

    public VehicleRateService(IVehicleRateRepository rateRepository, ILogger<VehicleRateService> logger)
    {
        _rateRepository = rateRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleRateDto>> GetAllRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rates = await _rateRepository.GetAllAsync(cancellationToken);
            return rates.Select(r => new VehicleRateDto
            {
                RateId = r.RateId,
                VehicleType = r.VehicleType,
                DisplayName = r.DisplayName,
                HourRate = r.HourRate,
                MinuteRate = r.MinuteRate,
                FullDayRate = r.FullDayRate,
                GracePeriodMinutes = r.GracePeriodMinutes,
                IconKey = r.IconKey,
                IsActive = r.IsActive
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifas vehiculares.");
            return new List<VehicleRateDto>();
        }
    }

    public async Task<VehicleRateDto?> GetByIdAsync(Guid rateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var r = await _rateRepository.GetByIdAsync(rateId, cancellationToken);
            if (r == null) return null;

            return new VehicleRateDto
            {
                RateId = r.RateId,
                VehicleType = r.VehicleType,
                DisplayName = r.DisplayName,
                HourRate = r.HourRate,
                MinuteRate = r.MinuteRate,
                FullDayRate = r.FullDayRate,
                GracePeriodMinutes = r.GracePeriodMinutes,
                IconKey = r.IconKey,
                IsActive = r.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tarifa por ID: {RateId}", rateId);
            return null;
        }
    }

    public async Task<VehicleRateDto> CreateRateAsync(CreateRateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = new VehicleRate
            {
                RateId = Guid.NewGuid(),
                VehicleType = dto.VehicleType,
                DisplayName = dto.DisplayName.Trim(),
                HourRate = dto.HourRate,
                MinuteRate = dto.MinuteRate,
                FullDayRate = dto.FullDayRate,
                GracePeriodMinutes = dto.GracePeriodMinutes,
                IconKey = dto.IconKey,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _rateRepository.AddAsync(rate, cancellationToken);

            return new VehicleRateDto
            {
                RateId = rate.RateId,
                VehicleType = rate.VehicleType,
                DisplayName = rate.DisplayName,
                HourRate = rate.HourRate,
                MinuteRate = rate.MinuteRate,
                FullDayRate = rate.FullDayRate,
                GracePeriodMinutes = rate.GracePeriodMinutes,
                IconKey = rate.IconKey,
                IsActive = rate.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear tarifa vehicular.");
            throw;
        }
    }

    public async Task<VehicleRateDto?> UpdateRateAsync(Guid rateId, UpdateRateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await _rateRepository.GetByIdAsync(rateId, cancellationToken);
            if (rate == null) return null;

            rate.DisplayName = dto.DisplayName.Trim();
            rate.HourRate = dto.HourRate;
            rate.MinuteRate = dto.MinuteRate;
            rate.FullDayRate = dto.FullDayRate;
            rate.GracePeriodMinutes = dto.GracePeriodMinutes;
            rate.IsActive = dto.IsActive;
            rate.UpdatedAtUtc = DateTime.UtcNow;

            await _rateRepository.UpdateAsync(rate, cancellationToken);

            return new VehicleRateDto
            {
                RateId = rate.RateId,
                VehicleType = rate.VehicleType,
                DisplayName = rate.DisplayName,
                HourRate = rate.HourRate,
                MinuteRate = rate.MinuteRate,
                FullDayRate = rate.FullDayRate,
                GracePeriodMinutes = rate.GracePeriodMinutes,
                IconKey = rate.IconKey,
                IsActive = rate.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar tarifa vehicular: {RateId}", rateId);
            return null;
        }
    }
}
