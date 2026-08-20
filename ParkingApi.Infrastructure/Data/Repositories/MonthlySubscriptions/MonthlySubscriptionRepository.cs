using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;

namespace ParkingApi.Infrastructure.Data.Repositories.MonthlySubscriptions;

public class MonthlySubscriptionRepository : IMonthlySubscriptionRepository
{
    private readonly DataContext _context;

    public MonthlySubscriptionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MonthlySubscription>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MonthlySubscriptions
            .OrderByDescending(s => s.StartDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlySubscription>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.MonthlySubscriptions
            .Where(s => s.IsActive && s.EndDateUtc >= now)
            .OrderByDescending(s => s.StartDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<MonthlySubscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await _context.MonthlySubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task<MonthlySubscription?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plateNumber)) return null;
        var normalized = plateNumber.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        return await _context.MonthlySubscriptions
            .Where(s => s.IsActive && s.PlateNumber == normalized && s.EndDateUtc >= now)
            .OrderByDescending(s => s.EndDateUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<MonthlySubscription> AddAsync(MonthlySubscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.PlateNumber = (subscription.PlateNumber ?? string.Empty).Trim().ToUpperInvariant();
        subscription.CreatedAt = DateTime.UtcNow;
        _context.MonthlySubscriptions.Add(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<MonthlySubscription> UpdateAsync(MonthlySubscription subscription, CancellationToken cancellationToken = default)
    {
        subscription.PlateNumber = (subscription.PlateNumber ?? string.Empty).Trim().ToUpperInvariant();
        _context.MonthlySubscriptions.Update(subscription);
        await _context.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(subscriptionId, cancellationToken);
        if (item == null) return false;

        item.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
