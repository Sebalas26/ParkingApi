using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Constants;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Agreements;
using ParkingApi.Domain.Interfaces.Repositories.Billing;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Discounts;
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
using ParkingApi.Domain.Interfaces.Repositories.Stores;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Tickets;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Tickets;

public class ParkingTicketService : IParkingTicketService
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ICommercialAgreementRepository _agreementRepository;
    private readonly ITicketDiscountRepository _discountRepository;
    private readonly IVehicleIncidentRepository _incidentRepository;
    private readonly IBillingResolutionRepository _resolutionRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ParkingTicketService> _logger;

    public ParkingTicketService(
        IParkingTicketRepository ticketRepository,
        IVehicleRateRepository rateRepository,
        IStoreRepository storeRepository,
        ICommercialAgreementRepository agreementRepository,
        ITicketDiscountRepository discountRepository,
        IVehicleIncidentRepository incidentRepository,
        IBillingResolutionRepository resolutionRepository,
        IBranchRepository branchRepository,
        ICurrentUserService currentUser,
        ILogger<ParkingTicketService> logger)
    {
        _ticketRepository = ticketRepository;
        _rateRepository = rateRepository;
        _storeRepository = storeRepository;
        _agreementRepository = agreementRepository;
        _discountRepository = discountRepository;
        _incidentRepository = incidentRepository;
        _resolutionRepository = resolutionRepository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ParkingTicket> CheckInAsync(CheckInRequestDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!dto.BranchId.HasValue || dto.BranchId.Value <= 0)
            {
                throw new InvalidOperationException("La sede (BranchId) es obligatoria para registrar el ingreso vehicular.");
            }

            // Resolver CompanyId mediante cascada estricta (DTO -> Claim JWT -> Sede relacional)
            int? resolvedCompanyId = dto.CompanyId.HasValue && dto.CompanyId.Value > 0 ? dto.CompanyId.Value : null;

            if (!resolvedCompanyId.HasValue && _currentUser != null)
            {
                resolvedCompanyId = _currentUser.GetEffectiveCompanyId(dto.CompanyId);
            }

            if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
            {
                var branch = await _branchRepository.GetByIdAsync(dto.BranchId.Value, cancellationToken);
                if (branch != null && branch.CompanyId > 0)
                {
                    resolvedCompanyId = branch.CompanyId;
                }
            }

            if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
            {
                throw new InvalidOperationException("La empresa (CompanyId) es obligatoria para registrar el ingreso vehicular.");
            }

            var normalizedPlate = dto.PlateNumber.Trim().ToUpperInvariant();

            // 1. Validar bloqueo activo por novedad / lista negra (impide ingreso tanto en WPF como API)
            var blockedIncident = await _incidentRepository.GetActiveBlockByPlateAsync(normalizedPlate, dto.BranchId, cancellationToken);
            if (blockedIncident != null)
            {
                throw new InvalidOperationException($"VEHÍCULO BLOQUEADO: La placa '{normalizedPlate}' tiene un bloqueo activo registrado por novedad: '{blockedIncident.IncidentType}' ({blockedIncident.Description}). No está permitido su ingreso.");
            }

            // 2. Validar que el vehículo no se encuentre ya adentro
            var active = await _ticketRepository.GetActiveByPlateAsync(normalizedPlate, dto.BranchId, null, cancellationToken);
            if (active != null)
            {
                throw new InvalidOperationException($"El vehículo con placa '{normalizedPlate}' ya se encuentra adentro.");
            }

            var rate = await _rateRepository.GetByTypeAsync(dto.VehicleType, cancellationToken);
            var hourRate = rate?.HourRate ?? 3000m;

            var ticketId = dto.TicketId.HasValue && dto.TicketId.Value != Guid.Empty
                ? dto.TicketId.Value
                : Guid.NewGuid();

            string ticketNumber;
            if (!string.IsNullOrWhiteSpace(dto.TicketNumber))
            {
                ticketNumber = dto.TicketNumber.Trim();
                var existingWithNumber = await _ticketRepository.GetByTicketNumberAsync(ticketNumber, cancellationToken);
                if (existingWithNumber != null)
                {
                    var countToday = await _ticketRepository.CountTodayTotalAsync(null, resolvedCompanyId.Value, cancellationToken) + 1;
                    ticketNumber = $"PKF-C{resolvedCompanyId.Value}-{DateTime.UtcNow:yyyyMMdd}-{countToday:D3}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
                }
            }
            else
            {
                var countToday = await _ticketRepository.CountTodayTotalAsync(null, resolvedCompanyId.Value, cancellationToken) + 1;
                ticketNumber = $"PKF-C{resolvedCompanyId.Value}-{DateTime.UtcNow:yyyyMMdd}-{countToday:D3}";
            }

            var ticket = new ParkingTicket
            {
                TicketId = ticketId,
                CompanyId = resolvedCompanyId.Value,
                BranchId = dto.BranchId.Value,
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

            return await _ticketRepository.AddAsync(ticket, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error en CheckIn para placa {Plate}", Constants.TicketError, dto.PlateNumber);
            throw new Exception($"Error al procesar el ingreso: {ex.Message}");
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
            ticket.PaymentMethod = (Domain.Common.Enums.PaymentMethod)(int)dto.PaymentMethod;
            // Guardar el ID real del catálogo maestro (tiene prioridad para analytics y dashboard)
            ticket.PaymentMethodId = dto.PaymentMethodId.HasValue && dto.PaymentMethodId.Value > 0
                ? dto.PaymentMethodId.Value
                : (int)dto.PaymentMethod;
            ticket.Status = TicketStatus.Completed;
            ticket.IsSynchronized = true;

            if (dto.BranchId.HasValue && ticket.BranchId == null)
            {
                ticket.BranchId = dto.BranchId.Value;
            }

            if (!ticket.CompanyId.HasValue || ticket.CompanyId.Value <= 0)
            {
                int? resolvedComp = dto.CompanyId.HasValue && dto.CompanyId.Value > 0 ? dto.CompanyId.Value : null;
                if (!resolvedComp.HasValue && _currentUser != null)
                {
                    resolvedComp = _currentUser.GetEffectiveCompanyId(dto.CompanyId);
                }
                if ((!resolvedComp.HasValue || resolvedComp.Value <= 0) && ticket.BranchId.HasValue)
                {
                    var branch = await _branchRepository.GetByIdAsync(ticket.BranchId.Value, cancellationToken);
                    if (branch != null && branch.CompanyId > 0)
                    {
                        resolvedComp = branch.CompanyId;
                    }
                }
                if (resolvedComp.HasValue && resolvedComp.Value > 0)
                {
                    ticket.CompanyId = resolvedComp.Value;
                }
            }

            if (dto.ResolutionId.HasValue)
            {
                ticket.ResolutionId = dto.ResolutionId.Value;
                ticket.ResolutionName = dto.ResolutionName;
                ticket.InvoiceNumber = dto.FiscalInvoiceNumber;
                ticket.IsElectronicInvoice = !string.IsNullOrWhiteSpace(dto.FiscalInvoiceNumber);

                try
                {
                    var resolution = await _resolutionRepository.GetByIdAsync(dto.ResolutionId.Value, cancellationToken);
                    if (resolution != null)
                    {
                        resolution.CurrentNumber++;
                        resolution.UpdatedAtUtc = DateTime.UtcNow;
                        await _resolutionRepository.UpdateAsync(resolution, cancellationToken);
                    }
                }
                catch (Exception resEx)
                {
                    _logger.LogWarning(resEx, "No se pudo incrementar el consecutivo de la resolución {ResolutionId}", dto.ResolutionId.Value);
                }
            }
            else
            {
                // Auto-asignar resolución activa de la sede si existe
                try
                {
                    var activeResolutions = await _resolutionRepository.GetActiveAsync(ticket.BranchId, ticket.CompanyId, cancellationToken);
                    var activeRes = activeResolutions.FirstOrDefault();
                    if (activeRes != null)
                    {
                        ticket.ResolutionId = activeRes.ResolutionId;
                        ticket.ResolutionName = !string.IsNullOrWhiteSpace(activeRes.Prefix) && !string.IsNullOrWhiteSpace(activeRes.Name)
                            ? $"{activeRes.Prefix} - {activeRes.Name}"
                            : activeRes.Name;
                        ticket.InvoiceNumber = $"{activeRes.Prefix}{activeRes.CurrentNumber}";
                        ticket.IsElectronicInvoice = true;

                        activeRes.CurrentNumber++;
                        activeRes.UpdatedAtUtc = DateTime.UtcNow;
                        await _resolutionRepository.UpdateAsync(activeRes, cancellationToken);
                    }
                }
                catch (Exception autoResEx)
                {
                    _logger.LogWarning(autoResEx, "No se pudo auto-asignar resolución activa en CheckOut para tiquete {TicketId}", ticket.TicketId);
                }
            }

            if (dto.StoreId.HasValue && dto.AgreementId.HasValue && !string.IsNullOrWhiteSpace(dto.InvoiceNumber) && dto.DiscountAmount > 0)
            {
                var discount = new TicketDiscount
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
                };

                await _discountRepository.AddAsync(discount, cancellationToken);
            }

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error en CheckOut para tiquete {TicketId}", Constants.TicketError, dto.TicketId);
            return null;
        }
    }

    public async Task<IReadOnlyList<ParkingTicket>> GetActiveTicketsAsync(int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetActiveTicketsAsync(branchId, companyId, cancellationToken);
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

    public async Task<IReadOnlyList<ParkingTicket>> GetHistoryAsync(DateTime date, int? branchId = null, int? companyId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _ticketRepository.GetHistoryAsync(date, branchId, companyId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Error}: Error al consultar historial para {Date}", Constants.TicketError, date);
            return new List<ParkingTicket>();
        }
    }
}
