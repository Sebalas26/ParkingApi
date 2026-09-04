using System;
using System.Collections.Generic;

namespace ParkingApi.Domain.Models;

public class SaaSPlan : GeneralEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal PriceCop { get; set; } = 0;
    public decimal? AnnualPriceCop { get; set; }
    public int MaxBranches { get; set; } = 1;
    public int MaxUsers { get; set; } = 5;
    public bool HasDesktopAccess { get; set; } = true;
    public bool HasWebAccess { get; set; } = true;
    public bool AllowMultipleSessions { get; set; } = false;
    public int MaxActiveSessionsPerUser { get; set; } = 1;
    public string? IncludedModulesWebJson { get; set; }
    public string? IncludedModulesDesktopJson { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
