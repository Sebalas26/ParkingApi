using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Dtos.Options;
using ParkingApi.Domain.Dtos.Realtime;
using ParkingApi.Domain.Dtos.Users;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Login;
using ParkingApi.Domain.Interfaces.Repositories.PasswordResetToken;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Auth;
using ParkingApi.Domain.Interfaces.Services.Login;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Interfaces.Services.Users;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Helpers.Jwt;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IRoleActionRepository _roleActionRepository;
    private readonly ILoginService _loginService;
    private readonly IPasswordResetTokenRepository _resetTokenRepository;
    private readonly IRealtimeNotificationService _realtimeNotifier;
    private readonly IMemoryCache _cache;
    private readonly JwtOptions _options;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserService userService,
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IRoleActionRepository roleActionRepository,
        ILoginService loginService,
        IPasswordResetTokenRepository resetTokenRepository,
        IRealtimeNotificationService realtimeNotifier,
        IMemoryCache cache,
        IOptions<JwtOptions> options,
        ILogger<AuthService> logger)
    {
        _userService = userService;
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _roleActionRepository = roleActionRepository;
        _loginService = loginService;
        _resetTokenRepository = resetTokenRepository;
        _realtimeNotifier = realtimeNotifier;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IncomeDto?> Login(AuthDto auth, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userService.GetUser(auth.Username, cancellation);
            if (user is null || !PasswordHasher.VerifyPassword(auth.Password, user.Password))
            {
                return null;
            }

            var jwtResult = user.CreateJwt(_options);
            if (string.IsNullOrEmpty(jwtResult.Token))
            {
                return null;
            }

            var oldToken = user.Token;
            user.Token = jwtResult.Jti;
            user.ExpireToken = _options.AccessTokenMinutes;

            await _userService.UpdateUserToken(user, cancellation);
            await _loginService.AddUserLogin(user, cancellation);

            _cache.Set($"ActiveToken_User_{user.Id}", jwtResult.Jti, TimeSpan.FromMinutes(_options.AccessTokenMinutes));

            if (!string.IsNullOrEmpty(oldToken) && !string.Equals(oldToken, jwtResult.Jti, StringComparison.Ordinal))
            {
                _ = _realtimeNotifier.NotifyCustomAsync(new ConfigNotificationDto
                {
                    EventType = "UserSessionTerminated",
                    UserId = user.Id,
                    SessionToken = jwtResult.Jti,
                    Title = "Sesión Cerrada en Otro Dispositivo",
                    Message = $"Se ha iniciado una nueva sesión para '{user.UserName}' desde otra ubicación o dispositivo.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellation);
            }

            return new IncomeDto
            {
                Fullname = user.Fullname,
                Token = jwtResult.Token,
                Success = true,
                IdUser = user.Id,
                IdRoleUser = user.IdUserRole ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el proceso de Login");
            return null;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginMobileDto credentials, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(credentials.Email.Trim(), cancellation);
            if (user == null || !PasswordHasher.VerifyPassword(credentials.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Correo o contraseña incorrectos.");
            }

            if (user.CompanyId.HasValue && user.Company != null && !user.Company.IsActive)
            {
                throw new UnauthorizedAccessException("La suscripción de la empresa se encuentra inactiva o suspendida. Comuníquese con el administrador.");
            }

            var roleName = user.UserRoleIdNavigation?.Role ?? "Operador";
            var isSuperAdmin = !user.CompanyId.HasValue || roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            var isAdmin = isSuperAdmin || roleName.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            var jwtResult = user.CreateJwt(roleName, _options);

            var oldToken = user.Token;
            user.Token = jwtResult.Jti;
            user.ExpirationDate = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUser(user, cancellation);

            _cache.Set($"ActiveToken_User_{user.Id}", jwtResult.Jti, TimeSpan.FromMinutes(_options.AccessTokenMinutes));

            if (!string.IsNullOrEmpty(oldToken) && !string.Equals(oldToken, jwtResult.Jti, StringComparison.Ordinal))
            {
                _ = _realtimeNotifier.NotifyCustomAsync(new ConfigNotificationDto
                {
                    EventType = "UserSessionTerminated",
                    UserId = user.Id,
                    SessionToken = jwtResult.Jti,
                    Title = "Sesión Cerrada en Otro Dispositivo",
                    Message = $"Se ha iniciado una nueva sesión para '{user.Username}' desde otra ubicación o dispositivo.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellation);
            }

            IReadOnlyList<Branch> userBranches;
            if (isSuperAdmin)
            {
                userBranches = await _branchRepository.GetActiveAsync(cancellation);
            }
            else if (isAdmin && user.CompanyId.HasValue)
            {
                userBranches = await _branchRepository.GetBranchesByCompanyIdAsync(user.CompanyId.Value, cancellation);
            }
            else
            {
                userBranches = await _branchRepository.GetBranchesByUserIdAsync(user.Id, cancellation);
            }

            var branchDtos = userBranches.Select(b => new Domain.Dtos.Branches.BranchDto
            {
                Id = b.Id,
                CompanyId = b.CompanyId,
                CompanyName = user.Company?.Name,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                Phone = b.Phone,
                City = b.City,
                TotalCapacity = b.TotalCapacity,
                Notes = b.Notes,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt
            }).ToList();

            return new LoginResponseDto
            {
                Token = jwtResult.Token,
                Role = roleName,
                MustChangePassword = user.MustChangePassword,
                UserId = user.Id,
                FullName = user.FullName,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                IsSuperAdmin = isSuperAdmin,
                Branches = branchDtos
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión para {Email}", credentials.Email);
            throw;
        }
    }

    public async Task<AuthResponseDto> LoginStandardAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return new AuthResponseDto { Success = false, ErrorMessage = "Usuario y contraseña requeridos." };
            }

            var user = await _userRepository.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);
            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.Password))
            {
                return new AuthResponseDto { Success = false, ErrorMessage = "Credenciales incorrectas o usuario inactivo." };
            }

            if (user.CompanyId.HasValue && user.Company != null && !user.Company.IsActive)
            {
                return new AuthResponseDto { Success = false, ErrorMessage = "La suscripción de la empresa se encuentra inactiva o suspendida. Comuníquese con el administrador." };
            }

            var roleName = user.UserRoleIdNavigation?.Role ?? "Operador";
            var isSuperAdmin = !user.CompanyId.HasValue || roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            var isAdmin = isSuperAdmin || roleName.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            var jwtResult = user.CreateJwt(roleName, _options);

            var oldToken = user.Token;
            user.Token = jwtResult.Jti;
            user.ExpirationDate = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateUser(user, cancellationToken);

            _cache.Set($"ActiveToken_User_{user.Id}", jwtResult.Jti, TimeSpan.FromMinutes(_options.AccessTokenMinutes));

            if (!string.IsNullOrEmpty(oldToken) && !string.Equals(oldToken, jwtResult.Jti, StringComparison.Ordinal))
            {
                _ = _realtimeNotifier.NotifyCustomAsync(new ConfigNotificationDto
                {
                    EventType = "UserSessionTerminated",
                    UserId = user.Id,
                    SessionToken = jwtResult.Jti,
                    Title = "Sesión Cerrada en Otro Dispositivo",
                    Message = $"Se ha iniciado una nueva sesión para '{user.Username}' desde otra ubicación o dispositivo.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }

            IReadOnlyList<Branch> userBranches;
            if (isSuperAdmin)
            {
                userBranches = await _branchRepository.GetActiveAsync(cancellationToken);
            }
            else if (isAdmin && user.CompanyId.HasValue)
            {
                userBranches = await _branchRepository.GetBranchesByCompanyIdAsync(user.CompanyId.Value, cancellationToken);
            }
            else
            {
                userBranches = await _branchRepository.GetBranchesByUserIdAsync(user.Id, cancellationToken);
            }

            var branchDtos = userBranches.Select(b => new Domain.Dtos.Branches.BranchDto
            {
                Id = b.Id,
                CompanyId = b.CompanyId,
                CompanyName = user.Company?.Name,
                Code = b.Code,
                Name = b.Name,
                Address = b.Address,
                Phone = b.Phone,
                City = b.City,
                TotalCapacity = b.TotalCapacity,
                Notes = b.Notes,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt
            }).ToList();

            var rolePermissions = isSuperAdmin || isAdmin
                ? new List<string>()
                : (await _roleActionRepository.GetActionsByRoleAsync(user.UserRoleId, cancellationToken))
                    .Where(ra => ra.IsActive && !string.IsNullOrWhiteSpace(ra.ActionName))
                    .Select(ra => ra.ActionName!)
                    .ToList();

            return new AuthResponseDto
            {
                Success = true,
                Token = jwtResult.Token,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                RoleName = roleName,
                RoleId = user.UserRoleId,
                IsAdmin = isAdmin,
                IsSuperAdmin = isSuperAdmin,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name,
                Branches = branchDtos,
                Permissions = rolePermissions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoginStandardAsync");
            return new AuthResponseDto { Success = false, ErrorMessage = "Error interno del servidor." };
        }
    }

    public async Task<bool> LogoutAsync(int userId, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellation);
            if (user != null)
            {
                user.Token = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUser(user, cancellation);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar logout para usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellation = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("El correo electrónico es requerido.");
            }

            var user = await _userRepository.GetByEmailAsync(email.Trim(), cancellation);
            if (user == null)
            {
                throw new System.Collections.Generic.KeyNotFoundException("El correo electrónico no se encuentra registrado en el sistema.");
            }

            var previousTokens = await _resetTokenRepository.GetActiveByUserIdAsync(user.Id, cancellation);
            foreach (var t in previousTokens)
            {
                t.IsActive = false;
                t.UpdatedAt = DateTime.UtcNow;
                await _resetTokenRepository.UpdateAsync(t, cancellation);
            }

            var tokenString = Guid.NewGuid().ToString("N");
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = tokenString,
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                IsUsed = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ResponsibleUserId = user.Id
            };

            return await _resetTokenRepository.AddAsync(resetToken, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar token de recuperación para {Email}", email);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellation = default)
    {
        try
        {
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                throw new ArgumentException("Las contraseñas no coinciden.");
            }

            var tokenRecord = await _resetTokenRepository.GetByTokenAsync(dto.Token, cancellation);
            if (tokenRecord == null || tokenRecord.ExpirationDate <= DateTime.UtcNow)
            {
                return false;
            }

            var user = tokenRecord.User;
            if (user == null) return false;

            user.Password = PasswordHasher.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            tokenRecord.IsUsed = true;
            tokenRecord.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUser(user, cancellation);
            await _resetTokenRepository.UpdateAsync(tokenRecord, cancellation);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restablecer la contraseña usando el token");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellation = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellation);
            if (user == null) return false;

            if (!PasswordHasher.VerifyPassword(dto.CurrentPassword, user.Password))
            {
                throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
            }

            user.Password = PasswordHasher.HashPassword(dto.NewPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            return await _userRepository.UpdateUser(user, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al realizar cambio de contraseña para usuario {UserId}", userId);
            throw;
        }
    }
}
