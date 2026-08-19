using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Interfaces.Repositories.Base;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.Auth;

public interface IUserSessionRepository : IBaseRepository<UserSession>
{
    Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IPasswordResetTokenRepository : IBaseRepository<PasswordResetToken>
{
    Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default);
}
