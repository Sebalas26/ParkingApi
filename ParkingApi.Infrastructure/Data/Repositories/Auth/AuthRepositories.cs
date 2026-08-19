using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Auth;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Auth;

public sealed class UserSessionRepository : IUserSessionRepository
{
    private readonly DataContext _context;
    private readonly ILogger<UserSessionRepository> _logger;

    public UserSessionRepository(DataContext context, ILogger<UserSessionRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sesiones activas de usuario: {UserId}", userId);
            return new List<UserSession>();
        }
    }

    public async Task<bool> AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        try
        {
            session.DeviceIdentifier ??= "Terminal POS / Web";
            session.IpAddress ??= "127.0.0.1";
            await _context.UserSessions.AddAsync(session, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar sesiÃ³n de usuario: {UserId}", session.UserId);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.UserSessions.Update(session);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sesiÃ³n: {SessionId}", session.SessionId);
            return false;
        }
    }
}

public sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PasswordResetTokenRepository> _logger;

    public PasswordResetTokenRepository(DataContext context, ILogger<PasswordResetTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed && t.IsActive && t.ExpirationDateUtc > DateTime.UtcNow, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar token de recuperaciÃ³n.");
            return null;
        }
    }

    public async Task<bool> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar token de recuperaciÃ³n.");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.PasswordResetTokens.Update(token);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar token de recuperaciÃ³n.");
            return false;
        }
    }
}
