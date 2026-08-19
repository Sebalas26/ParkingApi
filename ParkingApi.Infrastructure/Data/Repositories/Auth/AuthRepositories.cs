using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.Auth;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Data.Repositories.Base;

namespace ParkingApi.Infrastructure.Data.Repositories.Auth;

public class UserSessionRepository : BaseRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(DataContext context) : base(context) { }

    public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync(cancellationToken);
    }
}

public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(DataContext context) : base(context) { }

    public async Task<PasswordResetToken?> GetValidTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && t.IsActive && !t.IsUsed && t.ExpirationDateUtc > DateTime.UtcNow, cancellationToken);
    }
}
