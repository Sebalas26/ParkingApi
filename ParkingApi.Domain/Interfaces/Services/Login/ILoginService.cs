using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Users;

namespace ParkingApi.Domain.Interfaces.Services.Login;

public interface ILoginService
{
    Task<bool> AddUserLogin(LoginUserDto user, CancellationToken cancellation = default);
}
