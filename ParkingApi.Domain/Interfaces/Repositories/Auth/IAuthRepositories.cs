using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Auth;

public interface IUserSessionRepository
{
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
}

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}
