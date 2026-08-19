using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Interfaces.Services;

namespace ParkingApi.Core.Services.Sync;

public class SyncService : ISyncService
{
    private readonly IUserRepository _userRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IAgreementRepository _agreementRepository;
    private readonly IParkingTicketRepository _ticketRepository;

    public SyncService(
        IUserRepository userRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        IAgreementRepository agreementRepository,
        IParkingTicketRepository ticketRepository)
    {
        _userRepository = userRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<BootstrapSyncDto> GetBootstrapDataAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var rates = await _rateRepository.GetAllAsync(cancellationToken);
        var stores = await _storeRepository.GetAllAsync(cancellationToken);
        var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
        var activeTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);

        return new BootstrapSyncDto
        {
            ServerTimeUtc = DateTime.UtcNow,
            TotalCapacity = 120,
            Users = users.Where(u => u.IsActive).ToList(),
            Rates = rates.Where(r => r.IsActive).ToList(),
            Stores = stores.Where(s => s.IsActive).ToList(),
            Agreements = agreements.Where(a => a.IsActive).ToList(),
            ActiveTickets = activeTickets.ToList()
        };
    }
}
