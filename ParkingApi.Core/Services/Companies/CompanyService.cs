using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Services.Companies;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Companies;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly DataContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository companyRepository,
        DataContext context,
        ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetAllCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetAllAsync(cancellationToken);
        return companies.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<CompanyDto>> GetActiveCompaniesAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _companyRepository.GetActiveAsync(cancellationToken);
        return companies.Select(MapToDto).ToList();
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(id, cancellationToken);
        return company != null ? MapToDto(company) : null;
    }

    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        // 1. Validar NIT único
        var existingWithNit = await _companyRepository.GetByNitAsync(dto.Nit.Trim(), cancellationToken);
        if (existingWithNit != null)
        {
            throw new InvalidOperationException($"Ya existe una empresa registrada con el NIT/Documento '{dto.Nit.Trim()}'.");
        }

        // 2. Validar que el username del admin no exista
        var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Username.ToLower() == dto.AdminUsername.Trim().ToLower(), cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"El nombre de usuario '{dto.AdminUsername.Trim()}' ya está en uso.");
        }

        // 3. Crear Empresa
        var company = new Company
        {
            Name = dto.Name.Trim(),
            LegalName = string.IsNullOrWhiteSpace(dto.LegalName) ? dto.Name.Trim() : dto.LegalName.Trim(),
            Nit = dto.Nit.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            City = dto.City?.Trim(),
            PlanType = string.IsNullOrWhiteSpace(dto.PlanType) ? "Basic" : dto.PlanType.Trim(),
            MaxBranches = dto.MaxBranches > 0 ? dto.MaxBranches : 1,
            IsActive = true,
            SubscriptionExpiresAt = dto.SubscriptionExpiresAt,
            ResponsibleUserId = responsibleUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Crear Rol "Administrador" para la nueva empresa
        var adminRole = new UserRole
        {
            CompanyId = company.Id,
            Role = "Administrador",
            IsActive = true,
            ResponsibleUserId = responsibleUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserRole.Add(adminRole);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Asignar todos los módulos y acciones al nuevo rol de Administrador de la empresa
        var allModules = await _context.Module.Where(m => m.IsActive).ToListAsync(cancellationToken);
        foreach (var mod in allModules)
        {
            _context.UserRoleModule.Add(new UserRoleModule
            {
                UserRoleId = adminRole.Id,
                ModulesRoleId = mod.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ResponsibleUserId = responsibleUserId
            });
        }

        var allActions = await _context.Action.Where(a => a.IsActive).ToListAsync(cancellationToken);
        foreach (var act in allActions)
        {
            _context.RoleAction.Add(new RoleAction
            {
                RoleId = adminRole.Id,
                ActionId = act.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ResponsibleUserId = responsibleUserId
            });
        }

        // 6. Crear Usuario Administrador de la Empresa
        var hashedPassword = PasswordHasher.HashPassword(dto.AdminPassword.Trim());
        var user = new User
        {
            CompanyId = company.Id,
            UserRoleId = adminRole.Id,
            IdentificationTypeId = dto.AdminIdentificationTypeId > 0 ? dto.AdminIdentificationTypeId : 1,
            IdentificationNumber = string.IsNullOrWhiteSpace(dto.AdminIdentificationNumber) ? dto.Nit.Trim() : dto.AdminIdentificationNumber.Trim(),
            FirstName = dto.AdminFullName.Trim(),
            FullName = dto.AdminFullName.Trim(),
            Username = dto.AdminUsername.Trim(),
            Password = hashedPassword,
            Email = string.IsNullOrWhiteSpace(dto.AdminEmail) ? dto.Email.Trim() : dto.AdminEmail.Trim(),
            IsActive = true,
            MustChangePassword = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.User.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Empresa '{CompanyName}' (Id: {CompanyId}) creada exitosamente con administrador '{Username}'", company.Name, company.Id, user.Username);

        return MapToDto(company);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(int id, UpdateCompanyDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(id, cancellationToken);
        if (company == null)
        {
            throw new KeyNotFoundException($"Empresa con ID {id} no encontrada.");
        }

        company.Name = dto.Name.Trim();
        company.LegalName = dto.LegalName?.Trim();
        company.Nit = dto.Nit.Trim();
        company.Email = dto.Email.Trim();
        company.Phone = dto.Phone?.Trim();
        company.Address = dto.Address?.Trim();
        company.City = dto.City?.Trim();
        company.PlanType = string.IsNullOrWhiteSpace(dto.PlanType) ? company.PlanType : dto.PlanType.Trim();
        company.MaxBranches = dto.MaxBranches > 0 ? dto.MaxBranches : company.MaxBranches;
        company.IsActive = dto.IsActive;
        company.SubscriptionExpiresAt = dto.SubscriptionExpiresAt;
        company.UpdatedAt = DateTime.UtcNow;

        await _companyRepository.UpdateAsync(company, cancellationToken);
        return MapToDto(company);
    }

    public async Task<bool> ToggleCompanyStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(id, cancellationToken);
        if (company == null)
        {
            return false;
        }

        company.IsActive = !company.IsActive;
        company.UpdatedAt = DateTime.UtcNow;
        await _companyRepository.UpdateAsync(company, cancellationToken);
        return true;
    }

    private static CompanyDto MapToDto(Company c)
    {
        return new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            LegalName = c.LegalName,
            Nit = c.Nit,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
            City = c.City,
            PlanType = c.PlanType,
            MaxBranches = c.MaxBranches,
            IsActive = c.IsActive,
            SubscriptionExpiresAt = c.SubscriptionExpiresAt,
            BranchesCount = c.Branches?.Count ?? 0,
            UsersCount = c.Users?.Count ?? 0,
            CreatedAt = c.CreatedAt
        };
    }
}
