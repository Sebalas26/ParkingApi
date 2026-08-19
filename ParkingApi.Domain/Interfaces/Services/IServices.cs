using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Analytics;
using ParkingApi.Domain.Dtos.Auth;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Domain.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IParkingTicketService
{
    Task<ParkingTicket> CheckInAsync(CheckInRequestDto dto, CancellationToken cancellationToken = default);
    Task<ParkingTicket?> CheckOutAsync(CheckOutRequestDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default);
    Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default);
}

public interface ISyncService
{
    Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default);
}

public interface IAnalyticsService
{
    Task<FinancialSummaryDto> GetDailySummaryAsync(CancellationToken cancellationToken = default);
    Task<OccupancyStatsDto> GetOccupancyStatsAsync(CancellationToken cancellationToken = default);
}

public interface IVehicleRateService
{
    Task<IReadOnlyList<VehicleRate>> GetAllRatesAsync(CancellationToken cancellationToken = default);
    Task<VehicleRate> UpdateRateAsync(Guid rateId, decimal hourRate, decimal minuteRate, decimal fullDayRate, int graceMinutes, CancellationToken cancellationToken = default);
}

public interface IStoreService
{
    Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Store> CreateAsync(Store store, CancellationToken cancellationToken = default);
    Task UpdateAsync(Store store, CancellationToken cancellationToken = default);
}

public interface IAgreementService
{
    Task<IReadOnlyList<CommercialAgreement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CommercialAgreement> CreateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
    Task UpdateAsync(CommercialAgreement agreement, CancellationToken cancellationToken = default);
}

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
