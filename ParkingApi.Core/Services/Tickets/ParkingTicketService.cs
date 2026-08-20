using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Rates;
=======
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Tickets;

public class ParkingTicketService : IParkingTicketService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleRateRepository _rateRepository;
<<<<<<< HEAD
=======
    private readonly IStoreRepository _storeRepository;
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly ITicketDiscountRepository _discountRepository;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
    private readonly ILogger<ParkingTicketService> _logger;

    public ParkingTicketService(
        IParkingTicketRepository ticketRepository,
        IVehicleRateRepository rateRepository,
<<<<<<< HEAD
=======
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        ITicketDiscountRepository discountRepository,
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
        ILogger<ParkingTicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _rateRepository = rateRepository;
<<<<<<< HEAD
=======
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _discountRepository = discountRepository;
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
                throw new InvalidOperationException($"El vehÃ­culo con placa '{normalizedPlate}' ya se encuentra adentro.");
=======
                throw new InvalidOperationException($"El vehículo con placa '{normalizedPlate}' ya se encuentra adentro.");
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            }

            var rate = await _rateRepository.GetByTypeAsync(dto.VehicleType, cancellationToken);
            var hourRate = rate?.HourRate ?? 3000m;
<<<<<<< HEAD
            var todayCount = (await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken)).Count + (await _ticketRepository.GetActiveTicketsAsync(cancellationToken)).Count + 1;
            var ticketNumber = $"PKF-{DateTime.UtcNow:yyyyMMdd}-{todayCount:D3}";
=======
            var countToday = (await _ticketRepository.GetTodayCompletedTicketsAsync(cancellationToken)).Count + 1;
            var ticketNumber = $"PKF-{DateTime.UtcNow:yyyyMMdd}-{countToday:D3}";
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605

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

<<<<<<< HEAD
            await _ticketRepository.AddAsync(ticket, cancellationToken);
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CheckIn de vehÃ­culo: {PlateNumber}", dto.PlateNumber);
            throw;
        }
=======
            return await _ticketRepository.AddAsync(ticket, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error en CheckIn para placa {Plate}", Constants.TicketError, dto.PlateNumber);
            throw new Exception("Error interno al procesar el ingreso del vehículo.");
        }
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
                ticket.Discounts.Add(new TicketDiscount
=======
                var discount = new TicketDiscount
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
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
<<<<<<< HEAD
                });
=======
                };

                await _discountRepository.AddAsync(discount, cancellationToken);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            }

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            return ticket;
        }
        catch (Exception ex)
        {
<<<<<<< HEAD
            _logger.LogError(ex, "Error en CheckOut de tiquete: {TicketId}", dto.TicketId);
=======
            _logger.LogError(ex, "{Error}: Error en CheckOut para tiquete {TicketId}", Constants.TicketError, dto.TicketId);
>>>>>>> 90bdfc8b254eafadbadd6661c5529f3ac113a605
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetActiveTicketsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquetes activos", Constants.TicketError);
            return new List<ParkingTicket>();
        }
    }

    public async Task<ParkingTicket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetByIdAsync(id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete {Id}", Constants.TicketError, id);
            return null;
        }
    }

    public async Task<ParkingTicket?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetByTicketNumberAsync(ticketNumber, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar tiquete {Number}", Constants.TicketError, ticketNumber);
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetHistoryAsync(date, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar historial para {Date}", Constants.TicketError, date);
            return new List<ParkingTicket>();
        }
    }
}
