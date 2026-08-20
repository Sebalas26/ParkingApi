using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Auth;

namespace ParkingApi.Domain.Interfaces.Services.Auth;

public interface IAuthService
{
    Task<IncomeDto?> Login(AuthDto auth, CancellationToken cancellation = default);
    Task<LoginResponseDto> LoginAsync(LoginMobileDto credentials, CancellationToken cancellation = default);
    Task<AuthResponseDto> LoginStandardAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(int userId, CancellationToken cancellation = default);
    Task<bool> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellation = default);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellation = default);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellation = default);
}
