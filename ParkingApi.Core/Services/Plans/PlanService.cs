using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Plans;
using ParkingApi.Domain.Interfaces.Repositories.Plans;
using ParkingApi.Domain.Interfaces.Services.Plans;
using ParkingApi.Domain.Models;

namespace ParkingApi.Core.Services.Plans;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;
    private readonly ILogger<PlanService> _logger;

    public PlanService(IPlanRepository planRepository, ILogger<PlanService> logger)
    {
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlanDto>> GetAllPlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.GetAllAsync(cancellationToken);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<PlanDto>> GetActivePlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _planRepository.GetActiveAsync(cancellationToken);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<PlanDto?> GetPlanByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        return plan != null ? MapToDto(plan) : null;
    }

    public async Task<PlanDto> CreatePlanAsync(CreatePlanDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("El nombre del plan es obligatorio.");

        if (dto.PriceCop < 0)
            throw new ArgumentException("El precio mensual en COP no puede ser negativo.");

        if (dto.AnnualPriceCop.HasValue && dto.AnnualPriceCop.Value < 0)
            throw new ArgumentException("El precio anual en COP no puede ser negativo.");

        if (dto.MaxBranches < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 sede.");

        if (dto.MaxUsers < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 usuario.");

        var plan = new SaaSPlan
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            PriceCop = dto.PriceCop,
            AnnualPriceCop = dto.AnnualPriceCop,
            MaxBranches = dto.MaxBranches,
            MaxUsers = dto.MaxUsers,
            HasDesktopAccess = dto.HasDesktopAccess,
            HasWebAccess = dto.HasWebAccess,
            AllowMultipleSessions = dto.AllowMultipleSessions,
            MaxActiveSessionsPerUser = Math.Max(1, dto.MaxActiveSessionsPerUser),
            IncludedModulesWebJson = dto.IncludedModulesWebJson,
            IncludedModulesDesktopJson = dto.IncludedModulesDesktopJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _planRepository.AddAsync(plan, cancellationToken);
        _logger.LogInformation("SaaS Plan '{PlanName}' creado exitosamente con ID {PlanId}.", created.Name, created.Id);

        return MapToDto(created);
    }

    public async Task<PlanDto> UpdatePlanAsync(int id, UpdatePlanDto dto, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null)
            throw new KeyNotFoundException($"Plan con ID {id} no encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("El nombre del plan es obligatorio.");

        if (dto.PriceCop < 0)
            throw new ArgumentException("El precio mensual en COP no puede ser negativo.");

        if (dto.AnnualPriceCop.HasValue && dto.AnnualPriceCop.Value < 0)
            throw new ArgumentException("El precio anual en COP no puede ser negativo.");

        if (dto.MaxBranches < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 sede.");

        if (dto.MaxUsers < 1)
            throw new ArgumentException("El plan debe permitir al menos 1 usuario.");

        plan.Name = dto.Name.Trim();
        plan.Description = dto.Description?.Trim();
        plan.PriceCop = dto.PriceCop;
        plan.AnnualPriceCop = dto.AnnualPriceCop;
        plan.MaxBranches = dto.MaxBranches;
        plan.MaxUsers = dto.MaxUsers;
        plan.HasDesktopAccess = dto.HasDesktopAccess;
        plan.HasWebAccess = dto.HasWebAccess;
        plan.AllowMultipleSessions = dto.AllowMultipleSessions;
        plan.MaxActiveSessionsPerUser = Math.Max(1, dto.MaxActiveSessionsPerUser);
        plan.IncludedModulesWebJson = dto.IncludedModulesWebJson;
        plan.IncludedModulesDesktopJson = dto.IncludedModulesDesktopJson;
        plan.IsActive = dto.IsActive;

        await _planRepository.UpdateAsync(plan, cancellationToken);
        _logger.LogInformation("SaaS Plan ID {PlanId} actualizado exitosamente.", plan.Id);

        return MapToDto(plan);
    }

    public async Task<bool> TogglePlanStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null) return false;

        plan.IsActive = !plan.IsActive;
        await _planRepository.UpdateAsync(plan, cancellationToken);
        _logger.LogInformation("Estado del plan ID {PlanId} cambiado a {Status}.", plan.Id, plan.IsActive);
        return true;
    }

    public async Task<bool> DeletePlanAsync(int id, CancellationToken cancellationToken = default)
    {
        var plan = await _planRepository.GetByIdAsync(id, cancellationToken);
        if (plan == null) return false;

        if (plan.Companies != null && plan.Companies.Count > 0)
        {
            throw new InvalidOperationException($"No se puede eliminar el plan '{plan.Name}' porque está asignado a {plan.Companies.Count} empresa(s). Desactívelo en su lugar.");
        }

        await _planRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("SaaS Plan ID {PlanId} eliminado exitosamente.", id);
        return true;
    }

    private static PlanDto MapToDto(SaaSPlan plan)
    {
        return new PlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            PriceCop = plan.PriceCop,
            AnnualPriceCop = plan.AnnualPriceCop,
            MaxBranches = plan.MaxBranches,
            MaxUsers = plan.MaxUsers,
            HasDesktopAccess = plan.HasDesktopAccess,
            HasWebAccess = plan.HasWebAccess,
            AllowMultipleSessions = plan.AllowMultipleSessions,
            MaxActiveSessionsPerUser = plan.MaxActiveSessionsPerUser,
            IncludedModulesWebJson = plan.IncludedModulesWebJson,
            IncludedModulesDesktopJson = plan.IncludedModulesDesktopJson,
            IsActive = plan.IsActive,
            CreatedAt = plan.CreatedAt,
            CompaniesCount = plan.Companies?.Count ?? 0
        };
    }
}
