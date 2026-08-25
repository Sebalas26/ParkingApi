using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
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
            return await _userRepository.GetUsers(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return Enumerable.Empty<GetUsersDto>();
        }
    }

    public async Task<GetUsersDto?> GetUserById(int id, CancellationToken cancellation = default)
    {
        try
        {
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

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellation = default)
    {
        try
        {
            return await _userRepository.DeleteUser(userId, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {UserId}", userId);
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
            return false;
        }
    }
}
