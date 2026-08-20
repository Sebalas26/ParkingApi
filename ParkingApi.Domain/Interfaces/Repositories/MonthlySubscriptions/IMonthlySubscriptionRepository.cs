using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;

public interface IMonthlySubscriptionRepository
{
    Task<IReadOnlyList<MonthlySubscription>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlySubscription>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<MonthlySubscription?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<MonthlySubscription?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<MonthlySubscription> AddAsync(MonthlySubscription subscription, CancellationToken cancellationToken = default);
    Task<MonthlySubscription> UpdateAsync(MonthlySubscription subscription, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
