using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Incidents;
using ParkingApi.Domain.Interfaces.Repositories.Branches;
using ParkingApi.Domain.Interfaces.Repositories.Incidents;
using ParkingApi.Domain.Interfaces.Services;
using ParkingApi.Domain.Interfaces.Services.Incidents;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Incidents;

public class VehicleIncidentService : IVehicleIncidentService
{
    private readonly IVehicleIncidentRepository _repository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<VehicleIncidentService> _logger;

    public VehicleIncidentService(
        IVehicleIncidentRepository repository,
        IBranchRepository branchRepository,
        ICurrentUserService currentUser,
        ILogger<VehicleIncidentService> logger)
    {
        _repository = repository;
        _branchRepository = branchRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<VehicleIncidentDto>> GetAllAsync(int? branchId = null, string? status = null, bool? isBlocked = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetAllAsync(branchId, status, isBlocked, search, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<VehicleIncidentDto?> GetByIdAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(incidentId, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IReadOnlyList<VehicleIncidentDto>> GetByPlateAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetByPlateAsync(plateNumber, cancellationToken);
        return list.Select(MapToDto).ToList();
    }

    public async Task<PlateCheckResultDto> CheckPlateAsync(string plateNumber, int? branchId = null, CancellationToken cancellationToken = default)
    {
        var cleanPlate = plateNumber.Replace(" ", "").Replace("-", "").Trim().ToUpper();
        var blockedIncident = await _repository.GetActiveBlockByPlateAsync(cleanPlate, branchId, cancellationToken);

        if (blockedIncident != null)
        {
            return new PlateCheckResultDto
            {
                PlateNumber = cleanPlate,
                HasIncidents = true,
                IsBlocked = true,
                Reason = $"VEHÍCULO CON NOVEDAD ACTIVA / BLOQUEADO: {blockedIncident.IncidentType}",
                IncidentType = blockedIncident.IncidentType,
                Description = blockedIncident.Description,
                ReportedBy = blockedIncident.ReportedBy,
                ReportedAtUtc = blockedIncident.CreatedAtUtc,
                IncidentId = blockedIncident.IncidentId
            };
        }

        // Buscar si tiene alguna otra novedad activa
        var allIncidents = await _repository.GetByPlateAsync(cleanPlate, cancellationToken);
        var activeIncident = allIncidents.FirstOrDefault(i => i.Status != "Resuelta" && i.Status != "Resolved" && i.Status != "Inactiva" && i.Status != "Cerrada");

        if (activeIncident != null)
        {
            return new PlateCheckResultDto
            {
                PlateNumber = cleanPlate,
                HasIncidents = true,
                IsBlocked = true,
                Reason = $"VEHÍCULO CON NOVEDAD ACTIVA / BLOQUEADO: {activeIncident.IncidentType}",
                IncidentType = activeIncident.IncidentType,
                Description = activeIncident.Description,
                ReportedBy = activeIncident.ReportedBy,
                ReportedAtUtc = activeIncident.CreatedAtUtc,
                IncidentId = activeIncident.IncidentId
            };
        }

        return new PlateCheckResultDto
        {
            PlateNumber = cleanPlate,
            HasIncidents = false,
            IsBlocked = false,
            Reason = null
        };
    }

    public async Task<VehicleIncidentDto> CreateAsync(SaveVehicleIncidentDto dto, CancellationToken cancellationToken = default)
    {
        // Resolver CompanyId mediante cascada estricta (DTO -> Claim JWT -> Sede relacional)
        int? resolvedCompanyId = dto.CompanyId.HasValue && dto.CompanyId.Value > 0 ? dto.CompanyId.Value : null;

        if (!resolvedCompanyId.HasValue && _currentUser != null)
        {
            resolvedCompanyId = _currentUser.GetEffectiveCompanyId(dto.CompanyId);
        }

        if (!resolvedCompanyId.HasValue && dto.BranchId.HasValue && dto.BranchId.Value > 0)
        {
            var branch = await _branchRepository.GetByIdAsync(dto.BranchId.Value, cancellationToken);
            if (branch != null && branch.CompanyId > 0)
            {
                resolvedCompanyId = branch.CompanyId;
            }
        }

        if (!resolvedCompanyId.HasValue || resolvedCompanyId.Value <= 0)
        {
            throw new InvalidOperationException("La empresa (CompanyId) es obligatoria para registrar la novedad del vehículo.");
        }

        var isGlobal = dto.IsGlobal || (dto.BranchId == null && (dto.BranchIds == null || !dto.BranchIds.Any()));
        if (!isGlobal && (!dto.BranchId.HasValue || dto.BranchId.Value <= 0) && (dto.BranchIds == null || !dto.BranchIds.Any()))
        {
            throw new InvalidOperationException("Debe asociar al menos una sede válida o marcar la novedad como global.");
        }

        var entity = new VehicleIncident
        {
            IncidentId = dto.IncidentId ?? Guid.NewGuid(),
            CompanyId = resolvedCompanyId.Value,
            PlateNumber = dto.PlateNumber.Trim().ToUpper(),
            BranchId = dto.BranchId,
            IsGlobal = isGlobal,
            IncidentType = dto.IncidentType.Trim(),
            IsBlocked = dto.IsBlocked,
            Description = dto.Description.Trim(),
            ReportedBy = string.IsNullOrWhiteSpace(dto.ReportedBy) ? "Operador" : dto.ReportedBy.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Activa" : dto.Status.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        if (!entity.IsGlobal && dto.BranchIds != null && dto.BranchIds.Any())
        {
            foreach (var bId in dto.BranchIds.Distinct())
            {
                entity.IncidentBranches.Add(new VehicleIncidentBranch
                {
                    IncidentId = entity.IncidentId,
                    BranchId = bId
                });
            }
        }
        else if (!entity.IsGlobal && dto.BranchId.HasValue)
        {
            entity.IncidentBranches.Add(new VehicleIncidentBranch
            {
                IncidentId = entity.IncidentId,
                BranchId = dto.BranchId.Value
            });
        }

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    public async Task<VehicleIncidentDto?> UpdateAsync(Guid incidentId, SaveVehicleIncidentDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new VehicleIncident
        {
            IncidentId = incidentId,
            PlateNumber = dto.PlateNumber.Trim().ToUpper(),
            BranchId = dto.BranchId,
            IsGlobal = dto.IsGlobal || (dto.BranchId == null && (dto.BranchIds == null || !dto.BranchIds.Any())),
            IncidentType = dto.IncidentType.Trim(),
            IsBlocked = dto.IsBlocked,
            Description = dto.Description.Trim(),
            ReportedBy = dto.ReportedBy.Trim(),
            ContactPhone = dto.ContactPhone?.Trim(),
            Status = dto.Status.Trim()
        };

        if (!entity.IsGlobal && dto.BranchIds != null && dto.BranchIds.Any())
        {
            foreach (var bId in dto.BranchIds.Distinct())
            {
                entity.IncidentBranches.Add(new VehicleIncidentBranch
                {
                    IncidentId = entity.IncidentId,
                    BranchId = bId
                });
            }
        }
        else if (!entity.IsGlobal && dto.BranchId.HasValue)
        {
            entity.IncidentBranches.Add(new VehicleIncidentBranch
            {
                IncidentId = entity.IncidentId,
                BranchId = dto.BranchId.Value
            });
        }

        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated != null ? MapToDto(updated) : null;
    }

    public async Task<bool> ResolveAsync(Guid incidentId, ResolveIncidentDto dto, CancellationToken cancellationToken = default)
    {
        var notes = string.IsNullOrWhiteSpace(dto.ResolvedNotes) ? "Novedad resuelta y bloqueo levantado." : dto.ResolvedNotes.Trim();
        return await _repository.ResolveAsync(incidentId, notes, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(incidentId, cancellationToken);
    }

    private static VehicleIncidentDto MapToDto(VehicleIncident i)
    {
        var branchIds = i.IncidentBranches?.Select(ib => ib.BranchId).ToList() ?? new List<int>();
        var branchNames = i.IncidentBranches?
            .Where(ib => ib.Branch != null && !string.IsNullOrWhiteSpace(ib.Branch.Name))
            .Select(ib => ib.Branch!.Name)
            .ToList() ?? new List<string>();

        if (!branchIds.Any() && i.BranchId.HasValue)
        {
            branchIds.Add(i.BranchId.Value);
            if (i.Branch != null && !string.IsNullOrWhiteSpace(i.Branch.Name))
            {
                branchNames.Add(i.Branch.Name);
            }
        }

        return new VehicleIncidentDto
        {
            IncidentId = i.IncidentId,
            CompanyId = i.CompanyId,
            PlateNumber = i.PlateNumber,
            BranchId = i.BranchId,
            BranchName = i.Branch?.Name,
            IsGlobal = i.IsGlobal,
            BranchIds = branchIds,
            BranchNames = branchNames,
            IncidentType = i.IncidentType,
            IsBlocked = i.IsBlocked,
            Description = i.Description,
            ReportedBy = i.ReportedBy,
            ContactPhone = i.ContactPhone,
            Status = i.Status,
            ResolvedNotes = i.ResolvedNotes,
            ResolvedAtUtc = i.ResolvedAtUtc,
            CreatedAtUtc = i.CreatedAtUtc,
            UpdatedAtUtc = i.UpdatedAtUtc
        };
    }
}
