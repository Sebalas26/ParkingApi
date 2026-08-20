using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Dtos.IdentificationTypes;
using ParkingApi.Domain.Dtos.UserRoles;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Users;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(DataContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<GetUsersDto>> GetUsers(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.User
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Include(x => x.IdentificationTypeIdNavigation)
                .Select(x => new GetUsersDto
                {
                    Id = x.Id,
                    UserRoleId = x.UserRoleId,
                    IdentificationTypeId = x.IdentificationTypeId,
                    IdentificationNumber = x.IdentificationNumber,
                    FirstName = x.FirstName,
                    MiddleName = x.MiddleName,
                    FirstSurname = x.FirstSurname,
                    SecondLastName = x.SecondLastName,
                    FullName = x.FullName,
                    Username = x.Username,
                    Password = x.Password,
                    Email = x.Email,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserRoleDto = new GetUserRoleDto
                    {
                        IdUserRol = x.UserRoleIdNavigation.Id,
                        RoleName = x.UserRoleIdNavigation.Role,
                        IsActive = x.UserRoleIdNavigation.IsActive
                    },
                    IdentificationTypeDto = new GetIdentificationTypeDto
                    {
                        Id = x.IdentificationTypeIdNavigation.Id,
                        Name = x.IdentificationTypeIdNavigation.Identification,
                        IsActive = x.IdentificationTypeIdNavigation.IsActive
                    }
                })
                .ToListAsync(cancellation);
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
            return await _context.User
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Include(x => x.IdentificationTypeIdNavigation)
                .Where(x => x.Id == id)
                .Select(x => new GetUsersDto
                {
                    Id = x.Id,
                    UserRoleId = x.UserRoleId,
                    IdentificationTypeId = x.IdentificationTypeId,
                    IdentificationNumber = x.IdentificationNumber,
                    FirstName = x.FirstName,
                    MiddleName = x.MiddleName,
                    FirstSurname = x.FirstSurname,
                    SecondLastName = x.SecondLastName,
                    FullName = x.FullName,
                    Username = x.Username,
                    Password = x.Password,
                    Email = x.Email,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    UserRoleDto = new GetUserRoleDto
                    {
                        IdUserRol = x.UserRoleIdNavigation.Id,
                        RoleName = x.UserRoleIdNavigation.Role,
                        IsActive = x.UserRoleIdNavigation.IsActive
                    },
                    IdentificationTypeDto = new GetIdentificationTypeDto
                    {
                        Id = x.IdentificationTypeIdNavigation.Id,
                        Name = x.IdentificationTypeIdNavigation.Identification,
                        IsActive = x.IdentificationTypeIdNavigation.IsActive
                    }
                })
                .FirstOrDefaultAsync(cancellation);
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
            var normalizedUser = username.Trim().ToLower();
            return await _context.User
                .AsNoTracking()
                .Include(x => x.UserRoleIdNavigation)
                .Where(x => x.Username.ToLower() == normalizedUser && x.IsActive)
                .Select(x => new LoginUserDto
                {
                    Id = x.Id,
                    UserName = x.Username,
                    Password = x.Password,
                    Fullname = x.FullName,
                    Token = x.Token,
                    IdUserRole = x.UserRoleId
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<bool> CreateUser(User user, CancellationToken cancellation = default)
    {
        try
        {
            user.CreatedAt = DateTime.UtcNow;
            await _context.User.AddAsync(user, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario en base de datos");
            return false;
        }
    }

    public async Task<bool> UpdateUser(User user, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.User.FirstOrDefaultAsync(u => u.Id == user.Id, cancellation);
            if (existing == null) return false;

            existing.UserRoleId = user.UserRoleId;
            existing.IdentificationTypeId = user.IdentificationTypeId;
            existing.IdentificationNumber = user.IdentificationNumber;
            existing.FirstName = user.FirstName;
            existing.MiddleName = user.MiddleName;
            existing.FirstSurname = user.FirstSurname;
            existing.SecondLastName = user.SecondLastName;
            existing.FullName = user.FullName;
            existing.Username = user.Username;
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.Password = user.Password;
            }
            existing.Email = user.Email;
            existing.IsActive = user.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario");
            return false;
        }
    }

    public async Task<bool> UpdateUserToken(LoginUserDto user, CancellationToken cancellation = default)
    {
        try
        {
            var existing = await _context.User.FirstOrDefaultAsync(u => u.Id == user.Id, cancellation);
            if (existing == null) return false;

            existing.Token = user.Token;
            existing.AssignmentDate = DateTime.UtcNow;
            existing.ExpirationDate = DateTime.UtcNow.AddMinutes(user.ExpireToken ?? 60);
            existing.UpdatedAt = DateTime.UtcNow;

            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar token de usuario");
            return false;
        }
    }

    public async Task<GetUsersDto?> ValidateExist(string username, string numberIdentification, CancellationToken cancellation = default)
    {
        try
        {
            var normalizedUser = username.Trim().ToLower();
            var normalizedDoc = numberIdentification.Trim();

            return await _context.User
                .AsNoTracking()
                .Where(x => x.Username.ToLower() == normalizedUser || x.IdentificationNumber == normalizedDoc)
                .Select(x => new GetUsersDto
                {
                    Id = x.Id,
                    UserRoleId = x.UserRoleId,
                    IdentificationTypeId = x.IdentificationTypeId,
                    IdentificationNumber = x.IdentificationNumber,
                    FirstName = x.FirstName,
                    MiddleName = x.MiddleName,
                    FirstSurname = x.FirstSurname,
                    SecondLastName = x.SecondLastName,
                    FullName = x.FullName,
                    Username = x.Username,
                    Password = x.Password,
                    Email = x.Email,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.User
                .Include(u => u.UserRoleIdNavigation)
                .Include(u => u.IdentificationTypeIdNavigation)
                .FirstOrDefaultAsync(u => u.Id == id, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = username.Trim().ToLower();
            return await _context.User
                .Include(u => u.UserRoleIdNavigation)
                .Include(u => u.IdentificationTypeIdNavigation)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == normalized && u.IsActive, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default)
    {
        try
        {
            var normalized = email.Trim().ToLower();
            return await _context.User
                .Include(u => u.UserRoleIdNavigation)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalized && u.IsActive, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Constants.GetUserError);
            return null;
        }
    }

    public async Task<IReadOnlyList<User>> GetAllActiveUsersAsync(CancellationToken cancellation = default)
    {
        try
        {
            return await _context.User
                .AsNoTracking()
                .Where(u => u.IsActive)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar usuarios activos");
            return new List<User>();
        }
    }
}
