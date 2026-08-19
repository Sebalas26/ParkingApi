using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Users;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
