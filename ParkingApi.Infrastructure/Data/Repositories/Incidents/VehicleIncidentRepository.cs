using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
using ParkingApi.Domain.Models;

namespace ParkingApi.Infrastructure.Data.Repositories.Incidents;

public class VehicleIncidentRepository : IVehicleIncidentRepository
{
    private readonly DataContext _context;
    private readonly ILogger<VehicleIncidentRepository> _logger;

    public VehicleIncidentRepository(DataContext context, ILogger<VehicleIncidentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleIncident>> GetAllAsync(int? branchId = null, string? status = null, bool? isBlocked = null, string? search = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.VehicleIncidents
                .AsNoTracking()
                .Include(i => i.Branch)
                .Include(i => i.IncidentBranches)
                    .ThenInclude(ib => ib.Branch)
                .AsQueryable();

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(i => i.IsGlobal || i.BranchId == branchId || i.IncidentBranches.Any(ib => ib.BranchId == branchId.Value));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (isBlocked.HasValue)
            {
                query = query.Where(i => i.IsBlocked == isBlocked.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToUpper();
                query = query.Where(i =>
                    i.PlateNumber.Contains(term) ||
                    i.IncidentType.Contains(term) ||
                    i.ReportedBy.Contains(term) ||
                    i.Description.Contains(term)
                );
            }

            return await query
                .OrderByDescending(i => i.IsBlocked)
                .ThenByDescending(i => i.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedades de vehículos");
            return new List<VehicleIncident>();
        }
    }

    public async Task<VehicleIncident?> GetByIdAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.VehicleIncidents
                .Include(i => i.Branch)
                .Include(i => i.IncidentBranches)
                    .ThenInclude(ib => ib.Branch)
                .FirstOrDefaultAsync(i => i.IncidentId == incidentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedad {Id}", incidentId);
            return null;
        }
    }

    public async Task<IReadOnlyList<VehicleIncident>> GetByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPlate = plateNumber.Trim().ToUpper();
            return await _context.VehicleIncidents
                .AsNoTracking()
                .Include(i => i.Branch)
                .Include(i => i.IncidentBranches)
                    .ThenInclude(ib => ib.Branch)
                .Where(i => i.PlateNumber == normalizedPlate)
                .OrderByDescending(i => i.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar novedades para la placa {Plate}", plateNumber);
            return new List<VehicleIncident>();
        }
    }

    public async Task<VehicleIncident?> GetActiveBlockByPlateAsync(string plateNumber, int? branchId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPlate = plateNumber.Trim().ToUpper();
            var query = _context.VehicleIncidents
                .AsNoTracking()
                .Include(i => i.Branch)
                .Include(i => i.IncidentBranches)
                .Where(i => i.PlateNumber == normalizedPlate && i.IsBlocked && i.Status == "Activa");

            if (branchId.HasValue && branchId.Value > 0)
            {
                query = query.Where(i => i.IsGlobal || i.BranchId == branchId || i.IncidentBranches.Any(ib => ib.BranchId == branchId.Value));
            }

            return await query.OrderByDescending(i => i.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar bloqueo activo para la placa {Plate}", plateNumber);
            return null;
        }
    }

    public async Task<VehicleIncident> AddAsync(VehicleIncident incident, CancellationToken cancellationToken = default)
    {
        try
        {
            incident.CreatedAtUtc = DateTime.UtcNow;
            incident.PlateNumber = incident.PlateNumber.Trim().ToUpper();
            _context.VehicleIncidents.Add(incident);
            await _context.SaveChangesAsync(cancellationToken);
            return incident;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar novedad para la placa {Plate}", incident.PlateNumber);
            throw;
        }
    }

    public async Task<VehicleIncident?> UpdateAsync(VehicleIncident incident, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.VehicleIncidents
                .Include(i => i.IncidentBranches)
                .FirstOrDefaultAsync(i => i.IncidentId == incident.IncidentId, cancellationToken);
            if (existing == null) return null;

            existing.PlateNumber = incident.PlateNumber.Trim().ToUpper();
            existing.BranchId = incident.BranchId;
            existing.IsGlobal = incident.IsGlobal;
            existing.IncidentType = incident.IncidentType;
            existing.IsBlocked = incident.IsBlocked;
            existing.Description = incident.Description;
            existing.ReportedBy = incident.ReportedBy;
            existing.ContactPhone = incident.ContactPhone;
            existing.Status = incident.Status;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            // Actualizar sedes asignadas relacionalmente
            existing.IncidentBranches.Clear();
            if (incident.IncidentBranches != null && incident.IncidentBranches.Any())
            {
                foreach (var ib in incident.IncidentBranches)
                {
                    existing.IncidentBranches.Add(new VehicleIncidentBranch
                    {
                        IncidentId = existing.IncidentId,
                        BranchId = ib.BranchId
                    });
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar novedad {Id}", incident.IncidentId);
            throw;
        }
    }

    public async Task<bool> ResolveAsync(Guid incidentId, string resolvedNotes, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.VehicleIncidents.FindAsync(new object[] { incidentId }, cancellationToken);
            if (existing == null) return false;

            existing.Status = "Resuelta";
            existing.IsBlocked = false; // Al resolverse se levanta el bloqueo automáticamente
            existing.ResolvedNotes = resolvedNotes;
            existing.ResolvedAtUtc = DateTime.UtcNow;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resolver novedad {Id}", incidentId);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await _context.VehicleIncidents.FindAsync(new object[] { incidentId }, cancellationToken);
            if (existing == null) return false;

            _context.VehicleIncidents.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar novedad {Id}", incidentId);
            return false;
        }
    }
}
