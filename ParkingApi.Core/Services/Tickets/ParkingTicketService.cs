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

            var branch = await _branchRepository.GetByIdAsync(dto.BranchId.Value, cancellationToken);
            if (branch == null)
            {
                throw new InvalidOperationException($"La sede con ID {dto.BranchId.Value} no existe.");
            }

            if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
            {
                if (branch.CompanyId > 0)
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

            // 3. Validar capacidad máxima de la sede
            if (branch.TotalCapacity > 0)
            {
                var activeTickets = await _ticketRepository.GetActiveTicketsAsync(dto.BranchId.Value, resolvedCompanyId, cancellationToken);
                if (activeTickets.Count >= branch.TotalCapacity)
                {
                    throw new InvalidOperationException($"Capacidad máxima de la sede alcanzada ({branch.TotalCapacity} cupos). No hay cupos disponibles.");
                }
            }

            // 3.1. Validar horario operativo de la sede (COT - Colombia UTC-5)
            // Si el ingreso es fuera de horario, NO se bloquea la venta ni la entrada: se emite el tiquete normalmente
            // y se registra una novedad de auditoría automática ("INGRESO_EXTEMPORANEO") transparente para el operador.
            try
            {
                var operatingHours = await _branchRepository.GetOperatingHoursByBranchIdAsync(dto.BranchId.Value, cancellationToken);
                if (operatingHours != null && operatingHours.Count > 0)
                {
                    var entryTime = dto.EntryTimeUtc ?? DateTime.UtcNow;
                    var colombiaTime = entryTime.AddHours(-5);
                    var dayConfig = operatingHours.FirstOrDefault(h => h.DayOfWeek == colombiaTime.DayOfWeek);

                    if (dayConfig != null)
                    {
                        bool isExtemporaneous = false;
                        string reason = string.Empty;

                        if (!dayConfig.IsOpen)
                        {
                            isExtemporaneous = true;
                            reason = $"La sede se encuentra configurada como CERRADA el día {colombiaTime.DayOfWeek}.";
                        }
                        else
                        {
                            var currentTime = colombiaTime.TimeOfDay;
                            var allowedStart = dayConfig.OpeningTime.Subtract(TimeSpan.FromMinutes(dayConfig.BufferMinutesBefore));
                            var allowedEnd = dayConfig.ClosingTime.Add(TimeSpan.FromMinutes(dayConfig.BufferMinutesAfter));

                            if (currentTime < allowedStart || currentTime > allowedEnd)
                            {
                                isExtemporaneous = true;
                                reason = $"Ingreso a las {colombiaTime:HH:mm} (COT), fuera del horario oficial ({dayConfig.OpeningTime:hh\\:mm} - {dayConfig.ClosingTime:hh\\:mm}) con márgenes ({dayConfig.BufferMinutesBefore}m antes / {dayConfig.BufferMinutesAfter}m después).";
                            }
                        }

                        if (isExtemporaneous)
                        {
                            var auditIncident = new VehicleIncident
                            {
                                IncidentId = Guid.NewGuid(),
                                CompanyId = resolvedCompanyId.Value,
                                BranchId = dto.BranchId.Value,
                                PlateNumber = normalizedPlate,
                                IncidentType = "INGRESO_EXTEMPORANEO",
                                Description = $"{reason} Registrado automáticamente por el sistema para auditoría y control operativo.",
                                IsBlocked = false,
                                IsGlobal = false,
                                Status = "Registrada",
                                ReportedBy = string.IsNullOrWhiteSpace(dto.OperatorName) ? "Sistema (Auditoría Automática)" : dto.OperatorName,
                                CreatedAtUtc = DateTime.UtcNow
                            };

                            await _incidentRepository.AddAsync(auditIncident, cancellationToken);
                            _logger.LogInformation("Novedad de ingreso extemporáneo registrada automáticamente para la placa {Plate} en sede {BranchId}", normalizedPlate, dto.BranchId.Value);
                        }
                    }
                }
            }
            catch (Exception exHours)
            {
                _logger.LogWarning(exHours, "Error no bloqueante al verificar horario operativo de sede {BranchId} para placa {Plate}", dto.BranchId.Value, normalizedPlate);
            }

            // 4. Obtener tarifa horaria: prioridad DTO -> tarifa específica de la sede (considerando día de la semana COT) -> tarifa de empresa
            decimal hourRate = 0m;
            if (dto.HourlyRate.HasValue && dto.HourlyRate.Value > 0)
            {
                hourRate = dto.HourlyRate.Value;
            }
            else
            {
                var entryTime = dto.EntryTimeUtc ?? DateTime.UtcNow;
                var cotDay = entryTime.AddHours(-5).DayOfWeek;
                var rate = await _rateRepository.GetByTypeAsync(dto.VehicleType, dto.BranchId, resolvedCompanyId, cotDay, cancellationToken);
                if (rate != null && rate.HourRate > 0)
                {
                    hourRate = rate.HourRate;
                }
            }

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

            // Cargar convenio comercial si fue seleccionado para verificar si otorga Tiempo Libre de estadía
            int discountMinutes = 0;
            CommercialAgreement? agreement = null;
            if (dto.AgreementId.HasValue)
            {
                agreement = await _agreementRepository.GetByIdAsync(dto.AgreementId.Value, cancellationToken);
                if (agreement != null && (agreement.FreeMinutes.GetValueOrDefault() > 0 || agreement.FreeHours.GetValueOrDefault() > 0 || agreement.DiscountType == 2))
                {
                    discountMinutes = (agreement.FreeHours.GetValueOrDefault() * 60) + agreement.FreeMinutes.GetValueOrDefault();
                }
            }

            // Duración neta tasable tras deducir el tiempo libre otorgado por el convenio
            var effectiveMinutes = Math.Max(0, totalMinutes - discountMinutes);
            var billableHours = (int)Math.Max(1, Math.Ceiling(effectiveMinutes / 60.0));

            // Cargar sede para obtener parámetros de tarifa plena, pernocta y tiquete perdido
            Branch? branch = null;
            if (ticket.BranchId.HasValue)
            {
                branch = await _branchRepository.GetByIdAsync(ticket.BranchId.Value, cancellationToken);
            }
            else if (dto.BranchId.HasValue)
            {
                branch = await _branchRepository.GetByIdAsync(dto.BranchId.Value, cancellationToken);
            }

            // Determinar monto bruto: prioridad valor liquidado por terminal -> cálculo por motor de tarifas
            decimal gross;
            if (dto.GrossAmount.HasValue && dto.GrossAmount.Value > 0)
            {
                gross = dto.GrossAmount.Value;
            }
            else
            {
                decimal calculatedGross = 0m;
                var cotExitDay = exitTime.AddHours(-5).DayOfWeek;
                var rate = await _rateRepository.GetByTypeAsync(ticket.VehicleType, ticket.BranchId, ticket.CompanyId, cotExitDay, cancellationToken);

                if (rate != null)
                {
                    var grace = rate.GracePeriodMinutes;
                    if (effectiveMinutes <= grace)
                    {
                        calculatedGross = 0m;
                    }
                    else
                    {
                        // 1. Validar Pernocta / Tarifa Nocturna
                        bool isNightStay = false;
                        if (rate.NightRate > 0)
                        {
                            var localEntry = ticket.EntryTimeUtc.AddHours(-5);
                            var localExit = exitTime.AddHours(-5);
                            var nightStart = (branch?.NightStartTime) ?? new TimeSpan(18, 0, 0);
                            var nightEnd = (branch?.NightEndTime) ?? new TimeSpan(6, 0, 0);
                            int minNightStay = (branch?.NightStayMinMinutes.GetValueOrDefault() > 0) ? branch.NightStayMinMinutes.Value : 360;

                            bool enteredDuringNight = localEntry.TimeOfDay >= nightStart || localEntry.TimeOfDay < nightEnd;
                            bool exitedDuringNightOrMorning = localExit.TimeOfDay >= nightStart || localExit.TimeOfDay < nightEnd || localExit.Date > localEntry.Date;

                            if (enteredDuringNight && exitedDuringNightOrMorning && effectiveMinutes >= minNightStay)
                            {
                                isNightStay = true;
                                calculatedGross = rate.NightRate;
                            }
                        }

                        // 2. Tarifa Plena Cíclica (si no aplicó pernocta)
                        if (!isNightStay)
                        {
                            int fullDayThreshold = (branch != null && branch.FullDayThresholdMinutes.GetValueOrDefault() > 0) ? branch.FullDayThresholdMinutes.Value : 720;
                            bool fullDayApplies = true;
                            if (branch != null && !string.IsNullOrWhiteSpace(branch.FullDayApplicableDays))
                            {
                                var currentDayStr = exitTime.AddHours(-5).DayOfWeek.ToString();
                                fullDayApplies = branch.FullDayApplicableDays.Contains(currentDayStr, StringComparison.OrdinalIgnoreCase)
                                              || branch.FullDayApplicableDays.Equals("All", StringComparison.OrdinalIgnoreCase);
                            }

                            if (rate.FullDayRate > 0 && fullDayApplies && effectiveMinutes >= fullDayThreshold)
                            {
                                int fullDaysCount = effectiveMinutes / fullDayThreshold;
                                int remMins = effectiveMinutes % fullDayThreshold;
                                decimal remFee = 0m;

                                if (remMins > 0)
                                {
                                    if (rate.MinuteRate > 0 && rate.HourRate > 0)
                                    {
                                        var remH = remMins / 60;
                                        var remM = remMins % 60;
                                        remFee = (remH * rate.HourRate) + Math.Min(rate.HourRate, remM * rate.MinuteRate);
                                    }
                                    else if (rate.MinuteRate > 0)
                                    {
                                        remFee = remMins * rate.MinuteRate;
                                    }
                                    else if (rate.HourRate > 0)
                                    {
                                        var remH = (int)Math.Max(1, Math.Ceiling(remMins / 60.0));
                                        remFee = remH * rate.HourRate;
                                    }
                                    else
                                    {
                                        remFee = rate.FullDayRate;
                                    }

                                    if (remFee > rate.FullDayRate)
                                    {
                                        remFee = rate.FullDayRate;
                                    }
                                }

                                calculatedGross = (fullDaysCount * rate.FullDayRate) + remFee;
                            }
                            else
                            {
                                // Cálculo regular por minuto / hora
                                if (rate.MinuteRate > 0 && rate.HourRate > 0)
                                {
                                    var hours = effectiveMinutes / 60;
                                    var rem = effectiveMinutes % 60;
                                    calculatedGross = (hours * rate.HourRate) + Math.Min(rate.HourRate, rem * rate.MinuteRate);
                                }
                                else if (rate.MinuteRate > 0)
                                {
                                    calculatedGross = effectiveMinutes * rate.MinuteRate;
                                }
                                else if (rate.HourRate > 0)
                                {
                                    calculatedGross = billableHours * rate.HourRate;
                                }
                                else if (rate.FullDayRate > 0)
                                {
                                    calculatedGross = rate.FullDayRate;
                                }

                                if (rate.FullDayRate > 0 && calculatedGross > rate.FullDayRate && fullDayApplies)
                                {
                                    calculatedGross = rate.FullDayRate;
                                }
                            }
                        }
                    }
                }
                else if (ticket.HourlyRate > 0)
                {
                    calculatedGross = billableHours * ticket.HourlyRate;
                }

                gross = calculatedGross > 0 ? calculatedGross : dto.AmountPaid;
            }

            // Validar recargo por Tiquete Perdido
            ticket.IsLostTicket = dto.IsLostTicket;
            if (dto.IsLostTicket)
            {
                decimal lostFee = dto.LostTicketFee.HasValue && dto.LostTicketFee.Value > 0
                    ? dto.LostTicketFee.Value
                    : (branch?.LostTicketFee ?? 0m);

                ticket.LostTicketFee = lostFee;
                gross += lostFee;
            }

            // Determinar monto del descuento comercial
            decimal calculatedDiscount = dto.DiscountAmount;
            if (agreement != null && calculatedDiscount <= 0)
            {
                if (agreement.DiscountPercentage.HasValue && agreement.DiscountPercentage.Value > 0)
                {
                    calculatedDiscount = Math.Round(gross * (agreement.DiscountPercentage.Value / 100m), 2);
                }
                else if (agreement.DiscountFixedAmount.HasValue && agreement.DiscountFixedAmount.Value > 0)
                {
                    calculatedDiscount = agreement.DiscountFixedAmount.Value;
                }
            }

            // Determinar monto neto
            decimal net;
            if (dto.NetAmount.HasValue && dto.NetAmount.Value > 0)
            {
                net = dto.NetAmount.Value;
            }
            else
            {
                net = Math.Max(0m, gross - calculatedDiscount);
                if (net == 0 && dto.AmountPaid > 0)
                {
                    net = dto.AmountPaid;
                }
            }

            ticket.ExitTimeUtc = exitTime;
            ticket.TotalDurationMinutes = totalMinutes;
            ticket.GrossAmount = gross;
            ticket.DiscountAmount = dto.DiscountAmount;
            ticket.NetAmount = net;
            ticket.AmountPaid = dto.AmountPaid > 0 ? dto.AmountPaid : net;
            ticket.ChangeGiven = Math.Max(0m, ticket.AmountPaid - net);
            ticket.PaymentMethod = (Domain.Common.Enums.PaymentMethod)(int)dto.PaymentMethod;
            // Guardar el ID real del catálogo maestro de medios de pago
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
                    branch ??= await _branchRepository.GetByIdAsync(ticket.BranchId.Value, cancellationToken);
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
