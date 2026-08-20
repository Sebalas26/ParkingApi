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
<<<<<<< HEAD
        IAgreementRepository agreementRepository,
=======
        ICommercialAgreementRepository agreementRepository,
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
            var users = await _userRepository.GetAllAsync(cancellationToken);
=======
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            var rates = await _rateRepository.GetAllAsync(cancellationToken);
            var stores = await _storeRepository.GetAllAsync(cancellationToken);
            var agreements = await _agreementRepository.GetAllAsync(cancellationToken);
            var activeTickets = await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
<<<<<<< HEAD

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar paquete de bootstrap.");
            return new BootstrapSyncDto();
        }
    }

    public async Task<SyncResultDto> ProcessPendingBatchAsync(PendingSyncBatchDto batch, CancellationToken cancellationToken = default)
    {
        int syncedTickets = 0;
        int syncedDiscounts = 0;

        try
        {
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
=======

            return new BootstrapSyncDto
            {
                ServerTimeUtc = DateTime.UtcNow,
                TotalCapacity = 120,
                Users = new List<User>(),
                Rates = rates.ToList(),
                Stores = stores.ToList(),
                Agreements = agreements.ToList(),
                ActiveTickets = activeTickets.ToList()
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            };
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error al procesar lote de sincronizaciÃ³n.");
            return new SyncResultDto
            {
                Success = false,
                SyncedTicketsCount = syncedTickets,
                SyncedDiscountsCount = syncedDiscounts,
                Message = $"Fallo al procesar lote: {ex.Message}"
            };
=======
            _logger.LogError(ex, "Error al generar datos de sincronización inicial (bootstrap)");
            return new BootstrapSyncDto();
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
        }
    }
}
