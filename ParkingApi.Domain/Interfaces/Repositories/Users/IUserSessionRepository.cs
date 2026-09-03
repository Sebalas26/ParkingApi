using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Users;

public interface IUserSessionRepository
{
    Task<UserSession> AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<bool> IsSessionActiveAsync(int userId, string jti, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> CountActiveSessionsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionAsync(Guid sessionId, string reason = "Revoked", CancellationToken cancellationToken = default);
    Task<bool> RevokeSessionByJtiAsync(string jti, string reason = "Revoked", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> RevokeExcessSessionsAsync(int userId, int maxAllowed, string reason = "MaxSessionsExceeded", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> RevokeAllUserSessionsExceptLatestAsync(int userId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> RevokeAllSessionsByCompanyIdExceptLatestAsync(int companyId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(int UserId, string Jti)>> RevokeAllSessionsByCompanyIdAsync(int companyId, string reason = "CompanyPolicyDisabled", CancellationToken cancellationToken = default);
}
