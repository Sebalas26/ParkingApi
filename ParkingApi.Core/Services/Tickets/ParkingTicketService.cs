using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Tickets;

public class ParkingTicketService : IParkingTicketService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly IAgreementRepository _agreementRepository;

    public ParkingTicketService(
        IParkingTicketRepository ticketRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        IAgreementRepository agreementRepository)
    {
        _ticketRepository = ticketRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<ParkingTicket> CheckInAsync(CheckInRequestDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedPlate = dto.PlateNumber.Trim().ToUpperInvariant();
        var active = await _ticketRepository.GetActiveByPlateAsync(normalizedPlate, cancellationToken);
        if (active != null)
        {
            throw new InvalidOperationException($"El vehículo con placa '{normalizedPlate}' ya se encuentra adentro.");
        }

        var rate = await _rateRepository.GetByTypeAsync(dto.VehicleType, cancellationToken);
        var hourRate = rate?.HourRate ?? 3000m;
        var countToday = (await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken)).Count + 1;
        var ticketNumber = $"PKF-{DateTime.UtcNow:yyyyMMdd}-{countToday:D3}";

        var ticket = new ParkingTicket
        {
            TicketId = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            PlateNumber = normalizedPlate,
            VehicleType = dto.VehicleType,
            CustomerPhone = dto.PhoneNumber,
            Notes = dto.Notes,
            EntryTimeUtc = dto.EntryTimeUtc ?? DateTime.UtcNow,
            HourlyRate = hourRate,
            Status = TicketStatus.Active,
            OperatorName = string.IsNullOrWhiteSpace(dto.OperatorName) ? "Operador General" : dto.OperatorName,
            IsSynchronized = true
        };

        return await _ticketRepository.AddAsync(ticket, cancellationToken);
    }

    public async Task<ParkingTicket?> CheckOutAsync(CheckOutRequestDto dto, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(dto.TicketId, cancellationToken);
        if (ticket == null || ticket.Status != TicketStatus.Active)
        {
            return null;
        }

        var exitTime = dto.ExitTimeUtc ?? DateTime.UtcNow;
        var totalMinutes = (int)Math.Max(0, (exitTime - ticket.EntryTimeUtc).TotalMinutes);
        var billableHours = (int)Math.Max(1, Math.Ceiling(totalMinutes / 60.0));
        var gross = billableHours * ticket.HourlyRate;
        var net = Math.Max(0m, gross - dto.DiscountAmount);

        ticket.ExitTimeUtc = exitTime;
        ticket.TotalDurationMinutes = totalMinutes;
        ticket.GrossAmount = gross;
        ticket.DiscountAmount = dto.DiscountAmount;
        ticket.NetAmount = net;
        ticket.AmountPaid = dto.AmountPaid;
        ticket.ChangeGiven = Math.Max(0m, dto.AmountPaid - net);
        ticket.PaymentMethod = dto.PaymentMethod;
        ticket.Status = TicketStatus.Completed;
        ticket.IsSynchronized = true;

        if (dto.StoreId.HasValue && dto.AgreementId.HasValue && !string.IsNullOrWhiteSpace(dto.InvoiceNumber) && dto.DiscountAmount > 0)
        {
            ticket.Discounts.Add(new TicketDiscount
            {
                TicketDiscountId = Guid.NewGuid(),
                TicketId = ticket.TicketId,
                StoreId = dto.StoreId.Value,
                AgreementId = dto.AgreementId.Value,
                InvoiceNumber = dto.InvoiceNumber.Trim(),
                PurchaseAmount = dto.PurchaseAmount ?? 0m,
                AppliedDiscountAmount = dto.DiscountAmount,
                ValidatedAtUtc = DateTime.UtcNow,
                IsSynchronized = true
            });
        }

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        return ticket;
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.FindAsync(t => t.EntryTimeUtc.Date == date.Date || (t.ExitTimeUtc.HasValue && t.ExitTimeUtc.Value.Date == date.Date), cancellationToken);
    }
}
