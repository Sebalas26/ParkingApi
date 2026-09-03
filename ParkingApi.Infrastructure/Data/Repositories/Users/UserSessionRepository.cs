using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.Users;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly DataContext _context;

    public UserSessionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<UserSession> AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        await _context.UserSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<bool> IsSessionActiveAsync(int userId, string jti, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions.AnyAsync(
            s => s.UserId == userId && s.Jti == jti && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .CountAsync(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow, cancellationToken);
    }

    public async Task<bool> RevokeSessionAsync(Guid sessionId, string reason = "Revoked", CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions.FindAsync(new object[] { sessionId }, cancellationToken);
        if (session == null || session.IsRevoked) return false;

        session.IsRevoked = true;
        session.RevokedAtUtc = DateTime.UtcNow;
        session.RevokedReason = reason;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokeSessionByJtiAsync(string jti, string reason = "Revoked", CancellationToken cancellationToken = default)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.Jti == jti && !s.IsRevoked)
            .ToListAsync(cancellationToken);

        if (!sessions.Any()) return false;

        foreach (var s in sessions)
        {
            s.IsRevoked = true;
            s.RevokedAtUtc = DateTime.UtcNow;
            s.RevokedReason = reason;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<string>> RevokeExcessSessionsAsync(int userId, int maxAllowed, string reason = "MaxSessionsExceeded", CancellationToken cancellationToken = default)
    {
        var activeSessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count < maxAllowed)
        {
            return Array.Empty<string>();
        }

        // Dejar solo (maxAllowed - 1) para que quepa la nueva sesión
        var toRevoke = activeSessions.Skip(Math.Max(0, maxAllowed - 1)).ToList();
        var revokedJtis = new List<string>();

        foreach (var s in toRevoke)
        {
            s.IsRevoked = true;
            s.RevokedAtUtc = DateTime.UtcNow;
            s.RevokedReason = reason;
            revokedJtis.Add(s.Jti);
        }

        if (toRevoke.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return revokedJtis;
    }

    public async Task<IReadOnlyList<string>> RevokeAllUserSessionsExceptLatestAsync(int userId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default)
    {
        var activeSessions = await _context.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        if (activeSessions.Count <= 1)
        {
            return Array.Empty<string>();
        }

        var toRevoke = activeSessions.Skip(1).ToList();
        var revokedJtis = new List<string>();

        foreach (var s in toRevoke)
        {
            s.IsRevoked = true;
            s.RevokedAtUtc = DateTime.UtcNow;
            s.RevokedReason = reason;
            revokedJtis.Add(s.Jti);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return revokedJtis;
    }

    public async Task<IReadOnlyList<string>> RevokeAllSessionsByCompanyIdExceptLatestAsync(int companyId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default)
    {
        var companyUserIds = await _context.User
            .Where(u => u.CompanyId == companyId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var allRevokedJtis = new List<string>();

        foreach (var userId in companyUserIds)
        {
            var revoked = await RevokeAllUserSessionsExceptLatestAsync(userId, reason, cancellationToken);
            allRevokedJtis.AddRange(revoked);
        }

        return allRevokedJtis;
    }

    public async Task<IReadOnlyList<(int UserId, string Jti)>> RevokeAllSessionsByCompanyIdAsync(int companyId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default)
    {
        var companyUserIds = await _context.User
            .Where(u => u.CompanyId == companyId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var activeSessions = await _context.UserSessions
            .Where(s => companyUserIds.Contains(s.UserId) && !s.IsRevoked && s.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        var revoked = new List<(int UserId, string Jti)>();
        foreach (var s in activeSessions)
        {
            s.IsRevoked = true;
            s.RevokedAtUtc = DateTime.UtcNow;
            s.RevokedReason = reason;
            revoked.Add((s.UserId, s.Jti));
        }

        if (revoked.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return revoked;
    }
}
