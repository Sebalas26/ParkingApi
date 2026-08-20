using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Dtos.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.Tickets;
using ParkingApi.Domain.Interfaces.Repositories.VehicleRates;
using ParkingApi.Domain.Models;

namespace ParkingApi.Controllers;

/// <summary>
/// Endpoint público accesible sin autenticación para consulta de tiquetes y cobros
/// vía escaneo de código de barras o código QR desde dispositivos móviles.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/public/tickets")]
public class PublicTicketsController : ControllerBase
{
    private readonly IParkingTicketRepository _ticketRepository;
    private readonly IVehicleRateRepository _rateRepository;
    private readonly ILogger<PublicTicketsController> _logger;

    public PublicTicketsController(
        IParkingTicketRepository ticketRepository,
        IVehicleRateRepository rateRepository,
        ILogger<PublicTicketsController> logger)
    {
        _ticketRepository = ticketRepository;
        _rateRepository = rateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Consulta el estado público en tiempo real de un vehículo en el parqueadero por placa o número de tiquete.
    /// </summary>
    /// <param name="plate">Placa del vehículo (ej: XD21G, ABC123)</param>
    /// <param name="ticket">Número de tiquete opcional (ej: PKF-20260820-001)</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    [HttpGet("status")]
    public async Task<IActionResult> GetTicketStatus(
        [FromQuery] string? plate,
        [FromQuery] string? ticket,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(plate) && string.IsNullOrWhiteSpace(ticket))
        {
            return BadRequest(new PublicTicketStatusDto
            {
                IsFound = false,
                Message = "Debe proporcionar una placa o un número de tiquete para realizar la consulta."
            });
        }

        try
        {
            ParkingTicket? foundTicket = null;

            if (!string.IsNullOrWhiteSpace(ticket))
            {
                foundTicket = await _ticketRepository.GetByTicketNumberAsync(ticket.Trim(), cancellationToken);
            }

            if (foundTicket == null && !string.IsNullOrWhiteSpace(plate))
            {
                var normalizedPlate = plate.Trim().ToUpperInvariant().Replace("-", "").Replace(" ", "");
                foundTicket = await _ticketRepository.GetActiveByPlateAsync(normalizedPlate, cancellationToken);

                // Si no está activo, buscar el más reciente completado de esa placa
                if (foundTicket == null)
                {
                    var all = await _ticketRepository.GetAllAsync(cancellationToken);
                    foundTicket = all
                        .Where(t => t.PlateNumber.Trim().ToUpperInvariant() == normalizedPlate)
                        .OrderByDescending(t => t.EntryTimeUtc)
                        .FirstOrDefault();
                }
            }

            if (foundTicket == null)
            {
                return Ok(new PublicTicketStatusDto
                {
                    IsFound = false,
                    PlateNumber = plate?.ToUpperInvariant() ?? string.Empty,
                    TicketNumber = ticket ?? string.Empty,
                    Message = "No se encontró ningún registro de parqueadero para los datos suministrados."
                });
            }

            var now = DateTime.UtcNow;
            var entryUtc = foundTicket.EntryTimeUtc;
            var exitUtc = foundTicket.ExitTimeUtc ?? now;
            var duration = exitUtc - entryUtc;
            if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;

            var elapsedMinutes = (int)Math.Ceiling(duration.TotalMinutes);
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            var formattedDuration = hours > 0 ? $"{hours}h {minutes}m" : $"{minutes} min";

            decimal estimatedAmount = foundTicket.NetAmount > 0 ? foundTicket.NetAmount : foundTicket.GrossAmount;
            if (foundTicket.Status == TicketStatus.Active) // Activo: calcular tarifa en tiempo real
            {
                var rate = await _rateRepository.GetByTypeAsync(foundTicket.VehicleType, cancellationToken);
                var hourlyRate = rate?.HourRate ?? foundTicket.HourlyRate;
                var gracePeriod = rate?.GracePeriodMinutes ?? 15;

                if (elapsedMinutes <= gracePeriod)
                {
                    estimatedAmount = 0m;
                }
                else
                {
                    var billableHours = (decimal)Math.Ceiling(elapsedMinutes / 60.0);
                    if (billableHours < 1m) billableHours = 1m;
                    estimatedAmount = billableHours * hourlyRate;
                }
            }

            var vehicleTypeName = foundTicket.VehicleType switch
            {
                VehicleType.Car => "Automóvil / Sedán",
                VehicleType.Motorcycle => "Motocicleta",
                VehicleType.Suv => "Camioneta / SUV",
                VehicleType.Van => "Furgón / Minibús",
                VehicleType.Truck => "Vehículo Pesado / Camión",
                VehicleType.Bicycle => "Bicicleta",
                _ => "Vehículo"
            };

            var statusDescription = foundTicket.Status switch
            {
                TicketStatus.Active => "Vehículo actualmente adentro (Turno Activo)",
                TicketStatus.Completed => "Tiquete Liquidado y Pagado (Salida Registrada)",
                TicketStatus.Cancelled => "Tiquete Anulado / Cancelado",
                _ => "Desconocido"
            };

            var publicUrl = $"{Request.Scheme}://{Request.Host}/api/public/tickets/status?plate={foundTicket.PlateNumber}";

            return Ok(new PublicTicketStatusDto
            {
                IsFound = true,
                Message = "Consulta exitosa de tiquete.",
                TicketId = foundTicket.TicketId,
                TicketNumber = foundTicket.TicketNumber,
                PlateNumber = foundTicket.PlateNumber,
                VehicleType = (int)foundTicket.VehicleType,
                VehicleTypeName = vehicleTypeName,
                EntryTimeUtc = foundTicket.EntryTimeUtc,
                EntryTimeLocal = foundTicket.EntryTimeUtc.ToLocalTime(),
                ExitTimeUtc = foundTicket.ExitTimeUtc,
                ExitTimeLocal = foundTicket.ExitTimeUtc?.ToLocalTime(),
                ElapsedMinutes = elapsedMinutes,
                FormattedDuration = formattedDuration,
                HourlyRate = foundTicket.HourlyRate,
                EstimatedAmount = estimatedAmount,
                TotalPaid = foundTicket.Status == TicketStatus.Completed ? foundTicket.AmountPaid : 0m,
                Status = (int)foundTicket.Status,
                StatusDescription = statusDescription,
                ParkingName = "Parqueadero ParkFlow Central",
                ConsultationUrl = publicUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en consulta pública de tiquete para placa {Plate}", plate);
            return StatusCode(500, new PublicTicketStatusDto
            {
                IsFound = false,
                Message = "Ocurrió un error en el servidor al consultar el estado del tiquete."
            });
        }
    }
}
