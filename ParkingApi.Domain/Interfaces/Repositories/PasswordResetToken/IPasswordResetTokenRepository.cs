using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.PasswordResetToken;

public interface IPasswordResetTokenRepository
{
    Task<Domain.Models.PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellation = default);
    Task<List<Domain.Models.PasswordResetToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellation = default);
    Task<bool> AddAsync(Domain.Models.PasswordResetToken resetToken, CancellationToken cancellation = default);
    Task<bool> UpdateAsync(Domain.Models.PasswordResetToken resetToken, CancellationToken cancellation = default);
}
