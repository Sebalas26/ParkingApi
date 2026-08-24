using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
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
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IUserRepository userRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        IParkingTicketRepository ticketRepository,
        IConfiguration configuration,
        ILogger<SyncService> logger)
    {
        _userRepository = userRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _ticketRepository = ticketRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllActiveUsersAsync(cancellationToken);
            var rates = await _rateRepository.GetAllAsync(cancellationToken);
            var stores = await _storeRepository.GetAllAsync(cancellationToken);
            var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
            var recentTickets = await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken);

            var totalCapacity = int.TryParse(_configuration["ParkingSettings:TotalCapacity"], out var cap) ? cap : 100;

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = totalCapacity,
                Users = users.ToList(),
                Rates = rates.ToList(),
                Stores = stores.ToList(),
                Agreements = agreements.ToList(),
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
