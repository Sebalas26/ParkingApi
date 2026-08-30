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
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
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
    private readonly IVehicleIncidentRepository _incidentRepository;
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
        IVehicleIncidentRepository incidentRepository,
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
        _incidentRepository = incidentRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BootstrapSyncDto> GetBootstrapDataAsync(int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var totalCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;
            List<Branch> branches;
            List<User> users;
            List<PaymentMethod> paymentMethods;

            if (branchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
                branches = branch != null ? new List<Branch> { branch } : (await _branchRepository.GetActiveAsync(cancellationToken)).ToList();
                if (branch != null)
                {
                    totalCapacity = branch.TotalCapacity;
                }

                users = (await _branchRepository.GetUsersByBranchIdAsync(branchId.Value, cancellationToken)).ToList();

                var branchPms = await _branchRepository.GetPaymentMethodsByBranchIdAsync(branchId.Value, cancellationToken);
                if (branchPms.Any())
                {
                    paymentMethods = branchPms.Select(bpm => new PaymentMethod
                    {
                        Id = bpm.PaymentMethod.Id,
                        Name = bpm.PaymentMethod.Name,
                        Icon = bpm.PaymentMethod.Icon,
                        IsActive = bpm.PaymentMethod.IsActive && bpm.IsActive,
                        CreatedAt = bpm.PaymentMethod.CreatedAt,
                        UpdatedAt = bpm.PaymentMethod.UpdatedAt
                    }).ToList();
                }
                else
                {
                    var paymentMethodsDtos = await _paymentMethodRepository.GetAllActiveAsync(null, cancellationToken);
                    paymentMethods = paymentMethodsDtos.Select(dto => new PaymentMethod
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Icon = dto.Icon,
                        IsActive = dto.IsActive,
                        CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = dto.UpdatedAt
                    }).ToList();
                }
            }
            else
            {
                branches = (await _branchRepository.GetActiveAsync(cancellationToken)).ToList();
                users = (await _userRepository.GetAllActiveUsersAsync(cancellationToken)).ToList();
                var paymentMethodsDtos = await _paymentMethodRepository.GetAllActiveAsync(null, cancellationToken);
                paymentMethods = paymentMethodsDtos.Select(dto => new PaymentMethod
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Icon = dto.Icon,
                    IsActive = dto.IsActive,
                    CreatedAt = dto.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = dto.UpdatedAt
                }).ToList();
            }

            var allRates = await _rateRepository.GetAllAsync(null, cancellationToken);
            var rates = branchId.HasValue
                ? allRates.Where(r => r.IsActive && r.BranchId == branchId.Value).ToList()
                : allRates.Where(r => r.IsActive).ToList();

            var allStores = await _storeRepository.GetAllAsync(null, cancellationToken);
            var allAgreements = await _agreementRepository.GetAllAsync(null, cancellationToken);
            var stores = branchId.HasValue
                ? allStores.Where(s => s.IsActive && s.BranchId == branchId.Value).ToList()
                : allStores.Where(s => s.IsActive).ToList();

            var storeIds = stores.Select(s => s.StoreId).ToHashSet();
            var agreements = allAgreements.Where(a => a.IsActive && storeIds.Contains(a.StoreId)).ToList();

            var allShifts = await _shiftRepository.GetHistoryAsync(DateTime.UtcNow.AddDays(-30), null, null, cancellationToken);
            var shifts = branchId.HasValue
                ? allShifts.Where(ws => ws.BranchId == branchId.Value).ToList()
                : allShifts.ToList();

            var allSubs = await _monthlySubscriptionRepository.GetAllAsync(cancellationToken);
            var subscriptions = branchId.HasValue
                ? allSubs.Where(s => s.IsActive && s.BranchId == branchId.Value).ToList()
                : allSubs.Where(s => s.IsActive).ToList();

            var allActiveTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
            var allRecentTickets = await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken);
            var activeTickets = branchId.HasValue
                ? allActiveTickets.Where(t => t.BranchId == branchId.Value).ToList()
                : allActiveTickets.ToList();
            var recentTickets = branchId.HasValue
                ? allRecentTickets.Where(t => t.BranchId == branchId.Value).ToList()
                : allRecentTickets.ToList();

            var allIncidents = await _incidentRepository.GetAllAsync(branchId: null, status: "Activa", isBlocked: null, search: null, cancellationToken: cancellationToken);
            var incidents = branchId.HasValue
                ? allIncidents.Where(i => i.IsGlobal || i.BranchId == branchId.Value || i.IncidentBranches.Any(ib => ib.BranchId == branchId.Value)).ToList()
                : allIncidents.ToList();

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = totalCapacity,
                Branches = branches,
                Users = users,
                PaymentMethods = paymentMethods,
                Rates = rates,
                Stores = stores,
                Agreements = agreements,
                WorkShifts = shifts,
                MonthlySubscriptions = subscriptions,
                ActiveTickets = activeTickets,
                RecentTickets = recentTickets,
                Incidents = incidents
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar datos de sincronización inicial (bootstrap) para sede {BranchId}", branchId);
            return new BootstrapSyncDto();
        }
    }
}
