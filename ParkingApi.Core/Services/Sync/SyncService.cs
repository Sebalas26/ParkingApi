using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly ILogger<SyncService> _logger;

    public SyncService(
        IUserRepository userRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        IParkingTicketRepository ticketRepository,
        ILogger<SyncService> logger)
    {
        _userRepository = userRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
    }

    public async Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rates = await _rateRepository.GetAllAsync(cancellationToken);
            var stores = await _storeRepository.GetAllAsync(cancellationToken);
            var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = 120,
                Users = new List<User>(),
                Rates = rates.ToList(),
                Stores = stores.ToList(),
                Agreements = agreements.ToList(),
                ActiveTickets = activeTickets.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar datos de sincronización inicial (bootstrap)");
            return new BootstrapSyncDto();
        }
    }
}
