using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
=======
using ParkingApi.Domain.Common.Constants;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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

    public async Task<IEnumerable<GetUsersDto>> GetUsers(CancellationToken cancellation = default)
    {
        try
        {
<<<<<<< HEAD
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
=======
            return await _userRepository.GetUsers(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return Enumerable.Empty<GetUsersDto>();
        }
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    }

    public async Task<GetUsersDto?> GetUserById(int id, CancellationToken cancellation = default)
    {
        try
        {
<<<<<<< HEAD
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) return false;

            return await _userRepository.DeleteAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario: {UserId}", userId);
=======
            return await _userRepository.GetUserById(id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<LoginUserDto?> GetUser(string username, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRepository.GetUser(username, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<GetUsersDto?> CreateOrEditUser(GetUsersDto userDto, CancellationToken cancellation = default)
    {
        try
        {
            var isExist = await ValidateExist(userDto.Username, userDto.IdentificationNumber, cancellation);
            if (userDto.Id == 0 && !isExist)
            {
                var newUser = new User
                {
                    UserRoleId = userDto.UserRoleId,
                    IdentificationTypeId = userDto.IdentificationTypeId,
                    IdentificationNumber = userDto.IdentificationNumber.Trim(),
                    FirstName = userDto.FirstName.Trim(),
                    MiddleName = userDto.MiddleName?.Trim() ?? string.Empty,
                    FirstSurname = userDto.FirstSurname.Trim(),
                    SecondLastName = userDto.SecondLastName?.Trim() ?? string.Empty,
                    FullName = string.IsNullOrWhiteSpace(userDto.FullName)
                        ? $"{userDto.FirstName} {userDto.FirstSurname}".Trim()
                        : userDto.FullName.Trim(),
                    Username = userDto.Username.Trim().ToLower(),
                    Password = PasswordHasher.HashPassword(userDto.Password),
                    Email = userDto.Email.Trim(),
                    IsActive = userDto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateUser(newUser, cancellation);
                return await _userRepository.ValidateExist(newUser.Username, newUser.IdentificationNumber, cancellation);
            }
            else if (userDto.Id != 0)
            {
                var existingUser = await _userRepository.GetByIdAsync(userDto.Id, cancellation);
                if (existingUser == null) return null;

                existingUser.UserRoleId = userDto.UserRoleId;
                existingUser.IdentificationTypeId = userDto.IdentificationTypeId;
                existingUser.IdentificationNumber = userDto.IdentificationNumber.Trim();
                existingUser.FirstName = userDto.FirstName.Trim();
                existingUser.MiddleName = userDto.MiddleName?.Trim() ?? string.Empty;
                existingUser.FirstSurname = userDto.FirstSurname.Trim();
                existingUser.SecondLastName = userDto.SecondLastName?.Trim() ?? string.Empty;
                existingUser.FullName = string.IsNullOrWhiteSpace(userDto.FullName)
                    ? $"{userDto.FirstName} {userDto.FirstSurname}".Trim()
                    : userDto.FullName.Trim();
                existingUser.Username = userDto.Username.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(userDto.Password))
                {
                    existingUser.Password = PasswordHasher.HashPassword(userDto.Password);
                }
                existingUser.Email = userDto.Email.Trim();
                existingUser.IsActive = userDto.IsActive;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateUser(existingUser, cancellation);
                return await _userRepository.GetUserById(existingUser.Id, cancellation);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en crear o editar usuario");
            return null;
        }
    }

    public async Task<bool> UpdateUserToken(LoginUserDto user, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRepository.UpdateUserToken(user, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellation);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            return await _userRepository.UpdateUser(user, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario {UserId}", userId);
            return false;
        }
    }

    private async Task<bool> ValidateExist(string username, string numberIdentification, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userRepository.ValidateExist(username, numberIdentification, cancellation);
            return user != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return false;
        }
    }
}
