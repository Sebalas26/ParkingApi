using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Users;

namespace ParkingApi.Domain.Interfaces.Services.Users;

public interface IUserService
{
    Task<IEnumerable<GetUsersDto>> GetUsers(CancellationToken cancellation = default);
    Task<GetUsersDto?> GetUserById(int id, CancellationToken cancellation = default);
    Task<LoginUserDto?> GetUser(string username, CancellationToken cancellation = default);
    Task<GetUsersDto?> CreateOrEditUser(GetUsersDto userDto, CancellationToken cancellation = default);
    Task<bool> UpdateUserToken(LoginUserDto user, CancellationToken cancellation = default);
    Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellation = default);
    Task<bool> DeleteUserAsync(int userId, CancellationToken cancellation = default);
}
