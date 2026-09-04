using System;

namespace ParkingApi.Domain.Dtos.Plans;

public class PlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceCop { get; set; }
    public decimal? AnnualPriceCop { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxUsers { get; set; } = 5;
    public bool HasDesktopAccess { get; set; } = true;
    public bool HasWebAccess { get; set; } = true;
    public bool AllowMultipleSessions { get; set; } = false;
    public int MaxActiveSessionsPerUser { get; set; } = 1;
    public string? IncludedModulesWebJson { get; set; }
    public string? IncludedModulesDesktopJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int CompaniesCount { get; set; }
}

public class CreatePlanDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceCop { get; set; }
    public decimal? AnnualPriceCop { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxUsers { get; set; } = 5;
    public bool HasDesktopAccess { get; set; } = true;
    public bool HasWebAccess { get; set; } = true;
    public bool AllowMultipleSessions { get; set; } = false;
    public int MaxActiveSessionsPerUser { get; set; } = 1;
    public string? IncludedModulesWebJson { get; set; }
    public string? IncludedModulesDesktopJson { get; set; }
}

public class UpdatePlanDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceCop { get; set; }
    public decimal? AnnualPriceCop { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxUsers { get; set; } = 5;
    public bool HasDesktopAccess { get; set; } = true;
    public bool HasWebAccess { get; set; } = true;
    public bool AllowMultipleSessions { get; set; } = false;
    public int MaxActiveSessionsPerUser { get; set; } = 1;
    public string? IncludedModulesWebJson { get; set; }
    public string? IncludedModulesDesktopJson { get; set; }
    public bool IsActive { get; set; } = true;
}
