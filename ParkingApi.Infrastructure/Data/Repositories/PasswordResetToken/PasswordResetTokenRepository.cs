using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.PasswordResetToken;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.PasswordResetToken;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly DataContext _context;
    private readonly ILogger<PasswordResetTokenRepository> _logger;

    public PasswordResetTokenRepository(DataContext context, ILogger<PasswordResetTokenRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Domain.Models.PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.PasswordResetToken
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.IsActive && !t.IsUsed, cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar token de recuperación");
            return null;
        }
    }

    public async Task<List<Domain.Models.PasswordResetToken>> GetActiveByUserIdAsync(int userId, CancellationToken cancellation = default)
    {
        try
        {
            return await _context.PasswordResetToken
                .Where(t => t.UserId == userId && t.IsActive && !t.IsUsed)
                .ToListAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar tokens activos del usuario {UserId}", userId);
            return new List<Domain.Models.PasswordResetToken>();
        }
    }

    public async Task<bool> AddAsync(Domain.Models.PasswordResetToken resetToken, CancellationToken cancellation = default)
    {
        try
        {
            resetToken.CreatedAt = DateTime.UtcNow;
            await _context.PasswordResetToken.AddAsync(resetToken, cancellation);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar token de recuperación");
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Domain.Models.PasswordResetToken resetToken, CancellationToken cancellation = default)
    {
        try
        {
            resetToken.UpdatedAt = DateTime.UtcNow;
            _context.PasswordResetToken.Update(resetToken);
            return await _context.SaveChangesAsync(cancellation) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar token de recuperación");
            return false;
        }
    }
}
