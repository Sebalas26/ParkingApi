using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Users;

namespace ParkingApi.Domain.Interfaces.Services.Users;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
