using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Domain.Interfaces.Repositories.Auth;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Auth;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Helpers.Jwt;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IPasswordResetTokenRepository resetTokenRepository,
        IMemoryCache memoryCache,
        ILogger<AuthService> logger,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _resetTokenRepository = resetTokenRepository;
        _memoryCache = memoryCache;
        _logger = logger;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResponseDto { Success = false, ErrorMessage = "Usuario y contraseÃ±a requeridos." };
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
                DeviceIdentifier = "Terminal POS / Web",
                IpAddress = "127.0.0.1",
                StartedAtUtc = DateTime.UtcNow,
                LastHeartbeatUtc = DateTime.UtcNow,
                IsActive = true
            };

            await _sessionRepository.AddAsync(session, cancellationToken);
            _memoryCache.Set($"active_session_user_{user.UserId}", jwtResult.Jti, TimeSpan.FromMinutes(_jwtOptions.AccessTokenMinutes));

            return new AuthResponseDto
            {
                Success = true,
                Token = jwtResult.Token,
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                RoleName = roleName,
                IsAdmin = roleName.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                ExpiresAtUtc = jwtResult.ExpiresAtUtc
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el proceso de autenticaciÃ³n para {Username}", dto.Username);
            return new AuthResponseDto { Success = false, ErrorMessage = "Error interno durante la autenticaciÃ³n." };
        }
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive) return null;

            return new CurrentUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role?.Name ?? "Operador",
                IsAdmin = (user.Role?.Name ?? "").Equals("Administrador", StringComparison.OrdinalIgnoreCase)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario actual: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                throw new ArgumentException("La confirmaciÃ³n de la nueva contraseÃ±a no coincide.");
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null || !PasswordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            user.UpdatedAtUtc = DateTime.UtcNow;
            return await _userRepository.UpdateAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseÃ±a de usuario: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email.Trim(), cancellationToken);
            if (user == null) return false;

            var resetToken = new PasswordResetToken
            {
                ResetTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                Token = Guid.NewGuid().ToString("N"),
                ExpirationDateUtc = DateTime.UtcNow.AddHours(2),
                IsUsed = false,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            return await _resetTokenRepository.AddAsync(resetToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al solicitar recuperaciÃ³n de contraseÃ±a.");
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new ArgumentException("Las contraseÃ±as no coinciden.");
            }

            var tokenRecord = await _resetTokenRepository.GetValidTokenAsync(dto.Token.Trim(), cancellationToken);
            if (tokenRecord == null || tokenRecord.User == null) return false;

            tokenRecord.User.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);
            tokenRecord.User.UpdatedAtUtc = DateTime.UtcNow;
            tokenRecord.IsUsed = true;
            tokenRecord.IsActive = false;

            await _userRepository.UpdateAsync(tokenRecord.User, cancellationToken);
            await _resetTokenRepository.UpdateAsync(tokenRecord, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer contraseÃ±a con token.");
            return false;
        }
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _sessionRepository.GetActiveSessionsByUserIdAsync(userId, cancellationToken);
            foreach (var session in sessions)
            {
                session.IsActive = false;
                await _sessionRepository.UpdateAsync(session, cancellationToken);
            }
            _memoryCache.Remove($"active_session_user_{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante logout de usuario: {UserId}", userId);
        }
    }
}
