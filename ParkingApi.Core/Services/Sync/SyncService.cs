using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
using ParkingApi.Domain.Interfaces.Repositories.MonthlySubscriptions;
using ParkingApi.Domain.Interfaces.Repositories.PaymentMethods;
using ParkingApi.Domain.Interfaces.Repositories.RoleActions;
using ParkingApi.Domain.Interfaces.Repositories.Shifts;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.UserRoles;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services.Sync;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Sync;

public class SyncService : ISyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleActionRepository _roleActionRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IMonthlySubscriptionRepository _monthlySubscriptionRepository;
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleIncidentRepository _incidentRepository;
    private readonly IBillingResolutionRepository _billingResolutionRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IUserRepository userRepository,
        IBranchRepository branchRepository,
        IUserRoleRepository userRoleRepository,
        IRoleActionRepository roleActionRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        IShiftRepository shiftRepository,
        IMonthlySubscriptionRepository monthlySubscriptionRepository,
        IParkingTicketRepository ticketRepository,
        IVehicleIncidentRepository incidentRepository,
        IBillingResolutionRepository billingResolutionRepository,
        IConfiguration configuration,
        ILogger<SyncService> logger)
    {
        _userRepository = userRepository;
        _branchRepository = branchRepository;
        _userRoleRepository = userRoleRepository;
        _roleActionRepository = roleActionRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _shiftRepository = shiftRepository;
        _monthlySubscriptionRepository = monthlySubscriptionRepository;
        _ticketRepository = ticketRepository;
        _incidentRepository = incidentRepository;
        _billingResolutionRepository = billingResolutionRepository;
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
            int? targetCompanyId = null;
            Company? targetCompany = null;

            if (branchId.HasValue)
            {
                var branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
                if (branch != null)
                {
                    targetCompany = branch.Company;
                    targetCompanyId = branch.CompanyId;
                    totalCapacity = branch.TotalCapacity;
                    branches = new List<Branch> { branch };
                }
                else
                {
                    branches = (await _branchRepository.GetActiveAsync(null, cancellationToken)).ToList();
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
                    var paymentMethodsDtos = await _paymentMethodRepository.GetAllActiveAsync(targetCompanyId, cancellationToken);
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
                branches = (await _branchRepository.GetActiveAsync(null, cancellationToken)).ToList();
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

            // Sincronización de Roles y Acciones (RBAC Data-Driven)
            var userRolesDtos = await _userRoleRepository.GetUserRoles(targetCompanyId, branchId, cancellationToken);
            var userRoles = userRolesDtos.Select(r => new UserRoleSyncDto
            {
                Id = r.IdUserRol,
                Role = r.RoleName,
                Description = r.RoleName,
                IsActive = r.IsActive
            }).ToList();

            var roleActionsList = new List<RoleActionSyncDto>();
            foreach (var r in userRoles)
            {
                var actions = await _roleActionRepository.GetActionsByRoleAsync(r.Id, cancellationToken);
                foreach (var a in actions.Where(x => x.IsActive && !string.IsNullOrWhiteSpace(x.ActionName)))
                {
                    roleActionsList.Add(new RoleActionSyncDto
                    {
                        RoleId = r.Id,
                        ActionSlug = a.ActionName,
                        ActionName = a.ActionName,
                        IsActive = a.IsActive
                    });
                }
            }

            var allRates = await _rateRepository.GetAllAsync(targetCompanyId, cancellationToken);
            var rates = branchId.HasValue
                ? allRates.Where(r => r.IsActive && (r.BranchId == null || r.BranchId == branchId.Value)).ToList()
                : allRates.Where(r => r.IsActive).ToList();

            var allStores = await _storeRepository.GetAllAsync(targetCompanyId, cancellationToken);
            var allAgreements = await _agreementRepository.GetAllAsync(targetCompanyId, cancellationToken);
            var stores = branchId.HasValue
                ? allStores.Where(s => s.IsActive && (s.BranchId == null || s.BranchId == branchId.Value)).ToList()
                : allStores.Where(s => s.IsActive).ToList();

            var storeIds = stores.Select(s => s.StoreId).ToHashSet();
            List<CommercialAgreement> agreements;
            if (branchId.HasValue)
            {
                var branchAgreements = await _branchRepository.GetAgreementsByBranchIdAsync(branchId.Value, cancellationToken);
                if (branchAgreements.Any(ba => ba.IsActive))
                {
                    var activeAgIds = branchAgreements.Where(ba => ba.IsActive).Select(ba => ba.AgreementId).ToHashSet();
                    agreements = allAgreements.Where(a => a.IsActive && activeAgIds.Contains(a.AgreementId)).ToList();
                }
                else
                {
                    agreements = allAgreements.Where(a => a.IsActive && storeIds.Contains(a.StoreId)).ToList();
                }
            }
            else
            {
                agreements = allAgreements.Where(a => a.IsActive && storeIds.Contains(a.StoreId)).ToList();
            }

            var allShifts = await _shiftRepository.GetHistoryAsync(DateTime.UtcNow.AddDays(-30), null, branchId, cancellationToken);
            var shifts = branchId.HasValue
                ? allShifts.Where(ws => ws.BranchId == branchId.Value).ToList()
                : allShifts.ToList();

            var allSubs = await _monthlySubscriptionRepository.GetAllAsync(targetCompanyId, branchId, cancellationToken);
            var subscriptions = allSubs.Where(s => s.IsActive).ToList();

            var activeTickets = (await _ticketRepository.GetActiveTicketsAsync(branchId, targetCompanyId, cancellationToken)).ToList();
            var recentTickets = (await _ticketRepository.GetTodayCompletedTicketsAsync(branchId, targetCompanyId, cancellationToken)).ToList();

            var allIncidents = await _incidentRepository.GetAllAsync(branchId: branchId, status: "Activa", isBlocked: null, search: null, cancellationToken: cancellationToken);
            var incidents = branchId.HasValue
                ? allIncidents.Where(i => i.IsGlobal || i.BranchId == branchId.Value || i.IncidentBranches.Any(ib => ib.BranchId == branchId.Value)).ToList()
                : allIncidents.ToList();

            var allResolutions = await _billingResolutionRepository.GetAllAsync(branchId, targetCompanyId, cancellationToken);
            var resolutions = allResolutions.Where(r => r.IsActive).ToList();

            var branchPmsForBootstrap = branchId.HasValue
                ? (await _branchRepository.GetPaymentMethodsByBranchIdAsync(branchId.Value, cancellationToken)).ToList()
                : new List<BranchPaymentMethod>();

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = totalCapacity,
                RequireOpenShiftToOperate = targetCompany?.RequireOpenShiftToOperate ?? true,
                RequireInitialCashAmount = targetCompany?.RequireInitialCashAmount ?? true,
                AllowMultipleSessions = targetCompany?.AllowMultipleSessions ?? false,
                MaxActiveSessionsPerUser = targetCompany?.MaxActiveSessionsPerUser ?? 1,
                AllowMultipleOpenShifts = targetCompany?.AllowMultipleOpenShifts ?? false,
                MaxOpenShiftsPerUser = targetCompany?.MaxOpenShiftsPerUser ?? 1,
                Branches = branches,
                Users = users,
                UserRoles = userRoles,
                RoleActions = roleActionsList,
                PaymentMethods = paymentMethods,
                BranchPaymentMethods = branchPmsForBootstrap,
                Rates = rates,
                Stores = stores,
                Agreements = agreements,
                WorkShifts = shifts,
                MonthlySubscriptions = subscriptions,
                ActiveTickets = activeTickets,
                RecentTickets = recentTickets,
                Incidents = incidents,
                Resolutions = resolutions
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar datos de sincronización inicial (bootstrap) para sede {BranchId}", branchId);
            return new BootstrapSyncDto();
        }
    }
}

