using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Login;

public interface ILoginRepository
{
    Task<bool> AddUserLogin(Domain.Models.Login login, CancellationToken cancellation = default);
}
