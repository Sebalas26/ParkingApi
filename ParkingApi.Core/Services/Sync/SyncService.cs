using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Dtos.Sync;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Sync;

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

    public async Task<SyncResultDto> ProcessPendingBatchAsync(PendingSyncBatchDto batch, CancellationToken cancellationToken = default)
    {
        int syncedTickets = 0;
        int syncedDiscounts = 0;

        foreach (var pendingTicket in batch.PendingTickets)
        {
            var existing = await _ticketRepository.GetByIdAsync(pendingTicket.TicketId, cancellationToken);
            if (existing == null)
            {
                pendingTicket.IsSynchronized = true;
                await _ticketRepository.AddAsync(pendingTicket, cancellationToken);
                syncedTickets++;
            }
            else
            {
                existing.ExitTimeUtc = pendingTicket.ExitTimeUtc;
                existing.TotalDurationMinutes = pendingTicket.TotalDurationMinutes;
                existing.GrossAmount = pendingTicket.GrossAmount;
                existing.DiscountAmount = pendingTicket.DiscountAmount;
                existing.NetAmount = pendingTicket.NetAmount;
                existing.AmountPaid = pendingTicket.AmountPaid;
                existing.ChangeGiven = pendingTicket.ChangeGiven;
                existing.PaymentMethod = pendingTicket.PaymentMethod;
                existing.Status = pendingTicket.Status;
                existing.IsSynchronized = true;
                await _ticketRepository.UpdateAsync(existing, cancellationToken);
                syncedTickets++;
            }
        }

        return new SyncResultDto
        {
            Success = true,
            SyncedTicketsCount = syncedTickets,
            SyncedDiscountsCount = syncedDiscounts,
            Message = "Lote sincronizado exitosamente en el servidor central."
        };
    }
}
