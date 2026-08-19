using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Helpers.Jwt;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return new AuthResponseDto { Success = false, ErrorMessage = "Usuario y contraseña requeridos." };
        }

        var user = await _userRepository.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);
        if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            return new AuthResponseDto { Success = false, ErrorMessage = "Credenciales incorrectas o usuario inactivo." };
        }

        var roleName = user.Role?.Name ?? "Operador";
        var jwtResult = TokenHelper.CreateJwt(user, roleName, _jwtOptions);

        var session = new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            SessionToken = jwtResult.Token,
            StartedAtUtc = DateTime.UtcNow,
            LastHeartbeatUtc = DateTime.UtcNow,
            IsActive = true
        };
        await _sessionRepository.AddAsync(session, cancellationToken);

        return new AuthResponseDto
        {
            Success = true,
            Token = jwtResult.Token,
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            RoleName = roleName,
            IsAdmin = roleName.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !PasswordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return true;
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _sessionRepository.FindAsync(s => s.UserId == userId && s.IsActive, cancellationToken);
        foreach (var session in sessions)
        {
            session.IsActive = false;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }
    }
}
