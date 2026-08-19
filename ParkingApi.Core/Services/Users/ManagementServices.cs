using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => new UserDto
        {
            UserId = u.UserId,
            Username = u.Username,
            FullName = u.FullName,
            Email = u.Email,
            RoleName = u.Role?.Name ?? "Operador",
            RoleId = u.RoleId,
            IsActive = u.IsActive
        }).ToList();
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = dto.Username.Trim().ToLowerInvariant(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email?.Trim(),
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            RoleId = dto.RoleId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            RoleId = user.RoleId,
            IsActive = user.IsActive
        };
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }
}

public class VehicleRateService : IVehicleRateService
{
    private readonly IVehicleRateRepository _rateRepository;

    public VehicleRateService(IVehicleRateRepository rateRepository)
    {
        _rateRepository = rateRepository;
    }

    public async Task<IReadOnlyList<VehicleRate>> GetAllRatesAsync(CancellationToken cancellationToken = default)
    {
        return await _rateRepository.GetAllAsync(cancellationToken);
    }

    public async Task<VehicleRate> UpdateRateAsync(Guid rateId, decimal hourRate, decimal minuteRate, decimal fullDayRate, int graceMinutes, CancellationToken cancellationToken = default)
    {
        var rate = await _rateRepository.GetByIdAsync(rateId, cancellationToken);
        if (rate == null) throw new KeyNotFoundException("Tarifa no encontrada.");

        rate.HourRate = hourRate;
        rate.MinuteRate = minuteRate;
        rate.FullDayRate = fullDayRate;
        rate.GracePeriodMinutes = graceMinutes;
        rate.UpdatedAtUtc = DateTime.UtcNow;

        await _rateRepository.UpdateAsync(rate, cancellationToken);
        return rate;
    }
}

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _storeRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Store> CreateAsync(Store store, CancellationToken cancellationToken = default)
    {
        return await _storeRepository.AddAsync(store, cancellationToken);
    }

    public async Task UpdateAsync(Store store, CancellationToken cancellationToken = default)
    {
        await _storeRepository.UpdateAsync(store, cancellationToken);
    }
}

public class CommercialAgreementService : IAgreementService
{
    private readonly IAgreementRepository _agreementRepository;

    public CommercialAgreementService(IAgreementRepository agreementRepository)
    {
        _agreementRepository = agreementRepository;
    }

    public async Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _agreementRepository.GetAllAsync(cancellationToken);
    }

    public async Task<CommercialAgreement> CreateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        return await _agreementRepository.AddAsync(agreement, cancellationToken);
    }

    public async Task UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default)
    {
        await _agreementRepository.UpdateAsync(agreement, cancellationToken);
    }
}
