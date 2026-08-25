using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.Shifts;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services.Sync;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Sync;

public class SyncService : ISyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IMonthlySubscriptionRepository _monthlySubscriptionRepository;
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        IShiftRepository shiftRepository,
        IMonthlySubscriptionRepository monthlySubscriptionRepository,
        IParkingTicketRepository ticketRepository,
        IConfiguration configuration,
        ILogger<SyncService> logger)
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _shiftRepository = shiftRepository;
        _monthlySubscriptionRepository = monthlySubscriptionRepository;
        _ticketRepository = ticketRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var branches = await _branchRepository.GetActiveAsync(cancellationToken);
            var users = await _userRepository.GetAllActiveUsersAsync(cancellationToken);
            var paymentMethodsDtos = await _paymentMethodRepository.GetAllActiveAsync(cancellationToken);
            var rates = await _rateRepository.GetAllAsync(cancellationToken);
            var stores = await _storeRepository.GetAllAsync(cancellationToken);
            var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
            var shifts = await _shiftRepository.GetHistoryAsync(DateTime.UtcNow.AddDays(-30), null, cancellationToken);
            var subscriptions = await _monthlySubscriptionRepository.GetAllAsync(cancellationToken);
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
            var recentTickets = await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken);

            var totalCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;

            var paymentMethods = paymentMethodsDtos.Select(dto => new PaymentMethod
            {
                Id = dto.Id,
                Name = dto.Name,
                Icon = dto.Icon,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = dto.UpdatedAt
            }).ToList();

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = totalCapacity,
                Branches = branches.ToList(),
                Users = users.ToList(),
                PaymentMethods = paymentMethods,
                Rates = rates.ToList(),
                Stores = stores.ToList(),
                Agreements = agreements.ToList(),
                WorkShifts = shifts.ToList(),
                MonthlySubscriptions = subscriptions.ToList(),
                ActiveTickets = activeTickets.ToList(),
                RecentTickets = recentTickets.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar datos de sincronización inicial (bootstrap)");
            return new BootstrapSyncDto();
        }
    }
}
