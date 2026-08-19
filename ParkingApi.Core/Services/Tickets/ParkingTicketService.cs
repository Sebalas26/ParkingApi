using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Tickets;

public class ParkingTicketService : IParkingTicketService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly ILogger<ParkingTicketService> _logger;

    public ParkingTicketService(
        IParkingTicketRepository ticketRepository,
        IVehicleRateRepository rateRepository,
        ILogger<ParkingTicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _rateRepository = rateRepository;
        _logger = logger;
    }

    public async Task<ParkingTicket> CheckInAsync(CheckInRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPlate = dto.PlateNumber.Trim().ToUpperInvariant();
            var active = await _ticketRepository.GetActiveByPlateAsync(normalizedPlate, cancellationToken);
            if (active != null)
            {
                throw new InvalidOperationException($"El vehÃ­culo con placa '{normalizedPlate}' ya se encuentra adentro.");
            }

            var rate = await _rateRepository.GetByTypeAsync(dto.VehicleType, cancellationToken);
            var hourRate = rate?.HourRate ?? 3000m;
            var todayCount = (await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken)).Count + (await _ticketRepository.GetActiveTicketsAsync(cancellationToken)).Count + 1;
            var ticketNumber = $"PKF-{DateTime.UtcNow:yyyyMMdd}-{todayCount:D3}";

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
                IsSynchronized = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _ticketRepository.AddAsync(ticket, cancellationToken);
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CheckIn de vehÃ­culo: {PlateNumber}", dto.PlateNumber);
            throw;
        }
    }

    public async Task<ParkingTicket?> CheckOutAsync(CheckOutRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CheckOut de tiquete: {TicketId}", dto.TicketId);
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _ticketRepository.GetByTicketNumberAsync(ticketNumber, cancellationToken);
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _ticketRepository.GetByDateRangeAsync(start, end, cancellationToken);
    }
}
