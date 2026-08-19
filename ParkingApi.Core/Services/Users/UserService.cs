using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Users;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(cancellationToken);
            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Operador",
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar usuarios.");
            return new List<UserDto>();
        }
    }

    public async Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var u = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (u == null) return null;

            return new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "Operador",
                IsActive = u.IsActive,
                CreatedAtUtc = u.CreatedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar usuario por ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _userRepository.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException($"El nombre de usuario '{dto.Username}' ya estÃ¡ en uso.");
            }

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
                RoleName = "Operador",
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario: {Username}", dto.Username);
            throw;
        }
    }

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return null;

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email?.Trim();
            user.RoleId = dto.RoleId;
            user.IsActive = dto.IsActive;
            user.UpdatedAtUtc = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            }

            await _userRepository.UpdateAsync(user, cancellationToken);

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name ?? "Operador",
                IsActive = user.IsActive,
                CreatedAtUtc = user.CreatedAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            return await _userRepository.DeleteAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario: {UserId}", userId);
            return false;
        }
    }
}
