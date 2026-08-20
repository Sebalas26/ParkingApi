using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.MonthlySubscriptions;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services.MonthlySubscriptions;

public interface IMonthlySubscriptionService
{
    Task<IReadOnlyList<MonthlySubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlySubscriptionDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<MonthlySubscriptionDto?> GetByIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<MonthlySubscriptionDto?> GetActiveByPlateAsync(string plateNumber, CancellationToken cancellationToken = default);
    Task<MonthlySubscriptionDto> CreateAsync(CreateMonthlySubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<MonthlySubscriptionDto?> RenewAsync(Guid subscriptionId, RenewSubscriptionDto dto, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
}
