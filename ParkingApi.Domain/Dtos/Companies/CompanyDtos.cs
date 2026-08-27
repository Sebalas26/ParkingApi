using System;

namespace ParkingApi.Domain.Dtos.Companies;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string PlanType { get; set; } = "Basic";
    public int MaxBranches { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
    public int BranchesCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string PlanType { get; set; } = "Basic";
    public int MaxBranches { get; set; } = 1;
    public DateTime? SubscriptionExpiresAt { get; set; }

    // Datos del Administrador Inicial de la Empresa
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFullName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminIdentificationNumber { get; set; } = string.Empty;
    public int AdminIdentificationTypeId { get; set; } = 1;
}

public class UpdateCompanyDto
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string PlanType { get; set; } = "Basic";
    public int MaxBranches { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
}
