using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Users;

public interface IUserRepository
{
    Task<IEnumerable<GetUsersDto>> GetUsers(CancellationToken cancellation = default);
    Task<GetUsersDto?> GetUserById(int id, CancellationToken cancellation = default);
    Task<LoginUserDto?> GetUser(string username, CancellationToken cancellation = default);
    Task<bool> CreateUser(User user, CancellationToken cancellation = default);
    Task<bool> UpdateUser(User user, CancellationToken cancellation = default);
    Task<bool> DeleteUser(int userId, CancellationToken cancellation = default);
    Task<bool> UpdateUserToken(LoginUserDto user, CancellationToken cancellation = default);
    Task<GetUsersDto?> ValidateExist(string username, string numberIdentification, CancellationToken cancellation = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellation = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellation = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellation = default);
    Task<User?> GetByIdentifierAsync(string identifier, CancellationToken cancellation = default);
    Task<IReadOnlyList<User>> GetAllActiveUsersAsync(CancellationToken cancellation = default);
}
