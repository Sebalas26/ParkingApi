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

        // 5. Asignar módulos y acciones operativas y administrativas al nuevo rol de Administrador de la empresa (Excluyendo Módulo 16 SaaS Global)
        var tenantModules = await _context.Module
            .Where(m => m.IsActive && m.Id != 16 && !m.Name.ToLower().Contains("saas"))
            .ToListAsync(cancellationToken);
        foreach (var mod in tenantModules)
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

        var tenantActions = await _context.Action
            .Where(a => a.IsActive && a.ModuleId != 16 && !a.Slug.StartsWith("companies."))
            .ToListAsync(cancellationToken);
        foreach (var act in tenantActions)
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

        // 6. Crear Sede Inicial Obligatoria para la Empresa
        var defaultBranch = new Branch
        {
            CompanyId = company.Id,
            Code = "SEDE-01",
            Name = "Sede Principal",
            Address = string.IsNullOrWhiteSpace(company.Address) ? "Calle Principal # 1-01" : company.Address.Trim(),
            Phone = company.Phone?.Trim(),
            City = string.IsNullOrWhiteSpace(company.City) ? "Ciudad Principal" : company.City.Trim(),
            TotalCapacity = 100,
            Notes = $"Sede principal de operaciones de {company.Name}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Branches.Add(defaultBranch);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Crear Usuario Administrador de la Empresa
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

        // 8. Vincular al Administrador con la Sede Inicial (UserBranches)
        _context.UserBranches.Add(new UserBranch
        {
            UserId = user.Id,
            BranchId = defaultBranch.Id,
            IsDefault = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ResponsibleUserId = responsibleUserId
        });

        // 9. Sembrar Catálogo Inicial de Tarifas de Vehículos para la Empresa (CompanyId)
        var defaultRates = new List<VehicleRate>
        {
            new() { CompanyId = company.Id, BranchId = null, VehicleType = Domain.Common.Enums.VehicleType.Car, DisplayName = "Automóvil / Sedán", HourRate = 3000, MinuteRate = 50, FullDayRate = 25000, GracePeriodMinutes = 15, IconKey = "IconCar", IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { CompanyId = company.Id, BranchId = null, VehicleType = Domain.Common.Enums.VehicleType.Motorcycle, DisplayName = "Motocicleta", HourRate = 1500, MinuteRate = 25, FullDayRate = 12000, GracePeriodMinutes = 15, IconKey = "IconBike", IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { CompanyId = company.Id, BranchId = null, VehicleType = Domain.Common.Enums.VehicleType.Suv, DisplayName = "Camioneta / SUV", HourRate = 3500, MinuteRate = 60, FullDayRate = 30000, GracePeriodMinutes = 15, IconKey = "IconCar", IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { CompanyId = company.Id, BranchId = null, VehicleType = Domain.Common.Enums.VehicleType.Truck, DisplayName = "Vehículo Pesado / Camión", HourRate = 5000, MinuteRate = 90, FullDayRate = 45000, GracePeriodMinutes = 15, IconKey = "IconTruck", IsActive = true, CreatedAtUtc = DateTime.UtcNow },
            new() { CompanyId = company.Id, BranchId = null, VehicleType = Domain.Common.Enums.VehicleType.Bicycle, DisplayName = "Bicicleta", HourRate = 1000, MinuteRate = 15, FullDayRate = 8000, GracePeriodMinutes = 15, IconKey = "IconBike", IsActive = true, CreatedAtUtc = DateTime.UtcNow }
        };
        _context.VehicleRates.AddRange(defaultRates);

        // 10. Sembrar Resolución de Facturación Inicial para la Empresa (CompanyId)
        var defaultResolution = new BillingResolution
        {
            CompanyId = company.Id,
            BranchId = defaultBranch.Id,
            Name = "Resolución POS Inicial",
            DocumentType = "Documento equivalente electrónico del tiquete de máquina registradora con sistema P.O.S.",
            Prefix = "POS",
            ResolutionNumber = "18764000001",
            FromNumber = 1,
            ToNumber = 100000,
            CurrentNumber = 1,
            ValidFrom = DateTime.UtcNow.Date,
            ValidTo = DateTime.UtcNow.Date.AddYears(2),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.BillingResolutions.Add(defaultResolution);

        // 11. Habilitar Medios de Pago Activos para la Sede Inicial
        var activePaymentMethods = await _context.PaymentMethod.Where(p => p.IsActive).ToListAsync(cancellationToken);
        foreach (var pm in activePaymentMethods)
        {
            _context.BranchPaymentMethods.Add(new BranchPaymentMethod
            {
                BranchId = defaultBranch.Id,
                PaymentMethodId = pm.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ResponsibleUserId = responsibleUserId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Empresa '{CompanyName}' (Id: {CompanyId}) aprovisionada exitosamente con sede '{BranchCode}' y administrador '{Username}'", company.Name, company.Id, defaultBranch.Code, user.Username);

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

    public async Task<bool> DeleteCompanyAsync(int id, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var company = await _context.Companies
                .Include(c => c.Branches)
                .Include(c => c.Users)
                .Include(c => c.UserRoles)
                .Include(c => c.VehicleRates)
                .Include(c => c.Stores)
                .Include(c => c.BillingResolutions)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (company == null) return false;

            var branchIds = company.Branches.Select(b => b.Id).ToList();
            var userIds = company.Users.Select(u => u.Id).ToList();

            // 1. Eliminar Tickets y sus descuentos
            var tickets = await _context.ParkingTickets
                .Where(t => t.BranchId.HasValue && branchIds.Contains(t.BranchId.Value))
                .ToListAsync(cancellationToken);
            if (tickets.Any())
            {
                var ticketIds = tickets.Select(t => t.TicketId).ToList();
                var discounts = await _context.TicketDiscounts
                    .Where(d => ticketIds.Contains(d.TicketId))
                    .ToListAsync(cancellationToken);
                _context.TicketDiscounts.RemoveRange(discounts);
                _context.ParkingTickets.RemoveRange(tickets);
            }

            // 2. Eliminar incidentes y sus relaciones
            var incidentBranches = await _context.VehicleIncidentBranches
                .Where(ib => branchIds.Contains(ib.BranchId))
                .ToListAsync(cancellationToken);
            _context.VehicleIncidentBranches.RemoveRange(incidentBranches);

            var incidents = await _context.VehicleIncidents
                .Where(i => i.CompanyId == id || (i.BranchId.HasValue && branchIds.Contains(i.BranchId.Value)))
                .ToListAsync(cancellationToken);
            _context.VehicleIncidents.RemoveRange(incidents);

            // 3. Eliminar Turnos de trabajo (WorkShifts)
            var shifts = await _context.WorkShifts
                .Where(s => (s.BranchId.HasValue && branchIds.Contains(s.BranchId.Value)) || userIds.Contains(s.UserId))
                .ToListAsync(cancellationToken);
            _context.WorkShifts.RemoveRange(shifts);

            // 4. Eliminar resoluciones DIAN
            var resolutions = await _context.BillingResolutions
                .Where(r => r.CompanyId == id || (r.BranchId.HasValue && branchIds.Contains(r.BranchId.Value)))
                .ToListAsync(cancellationToken);
            _context.BillingResolutions.RemoveRange(resolutions);

            // 5. Eliminar medios de pago específicos de sedes
            var branchPaymentMethods = await _context.BranchPaymentMethods
                .Where(bpm => branchIds.Contains(bpm.BranchId))
                .ToListAsync(cancellationToken);
            _context.BranchPaymentMethods.RemoveRange(branchPaymentMethods);

            // 6. Eliminar UserBranches
            var userBranches = await _context.UserBranches
                .Where(ub => branchIds.Contains(ub.BranchId) || userIds.Contains(ub.UserId))
                .ToListAsync(cancellationToken);
            _context.UserBranches.RemoveRange(userBranches);

            // 7. Eliminar Convenios Comerciales y Aliados (Stores)
            var agreements = await _context.CommercialAgreements
                .Where(a => a.Store.CompanyId == id || branchIds.Contains(a.Store.BranchId ?? 0))
                .ToListAsync(cancellationToken);
            _context.CommercialAgreements.RemoveRange(agreements);

            var stores = await _context.Stores
                .Where(s => s.CompanyId == id || (s.BranchId.HasValue && branchIds.Contains(s.BranchId.Value)))
                .ToListAsync(cancellationToken);
            _context.Stores.RemoveRange(stores);

            // 8. Eliminar Tarifas de vehículos (VehicleRates)
            var rates = await _context.VehicleRates
                .Where(r => r.CompanyId == id || (r.BranchId.HasValue && branchIds.Contains(r.BranchId.Value)))
                .ToListAsync(cancellationToken);
            _context.VehicleRates.RemoveRange(rates);

            // 9. Eliminar Mensualidades (MonthlySubscriptions)
            var subs = await _context.MonthlySubscriptions
                .Where(s => s.CompanyId == id || (s.BranchId.HasValue && branchIds.Contains(s.BranchId.Value)))
                .ToListAsync(cancellationToken);
            _context.MonthlySubscriptions.RemoveRange(subs);

            // 10. Eliminar Medios de Pago propios de la compañía
            var methods = await _context.PaymentMethod
                .Where(m => m.CompanyId == id)
                .ToListAsync(cancellationToken);
            _context.PaymentMethod.RemoveRange(methods);

            // 11. Eliminar Logins y Tokens de los usuarios
            var userLogins = await _context.Login
                .Where(l => userIds.Contains(l.UserId))
                .ToListAsync(cancellationToken);
            _context.Login.RemoveRange(userLogins);

            var userResetTokens = await _context.PasswordResetToken
                .Where(t => userIds.Contains(t.UserId))
                .ToListAsync(cancellationToken);
            _context.PasswordResetToken.RemoveRange(userResetTokens);

            // 12. Eliminar Roles de Usuario
            var roles = await _context.UserRole
                .Where(r => r.CompanyId == id)
                .ToListAsync(cancellationToken);
            
            // Eliminar RoleActions
            if (roles.Any())
            {
                var roleIds = roles.Select(r => r.Id).ToList();
                var roleActions = await _context.RoleAction
                    .Where(ra => roleIds.Contains(ra.RoleId))
                    .ToListAsync(cancellationToken);
                _context.RoleAction.RemoveRange(roleActions);

                var roleModules = await _context.UserRoleModule
                    .Where(rm => roleIds.Contains(rm.UserRoleId))
                    .ToListAsync(cancellationToken);
                _context.UserRoleModule.RemoveRange(roleModules);
            }

            // 13. Eliminar Usuarios y UserParkings asociados
            var userParkings = await _context.UserParkings
                .Where(up => userIds.Contains(up.UserId))
                .ToListAsync(cancellationToken);
            _context.UserParkings.RemoveRange(userParkings);

            var users = await _context.User
                .Where(u => u.CompanyId == id)
                .ToListAsync(cancellationToken);
            _context.User.RemoveRange(users);

            // 14. Eliminar Sedes (Branches)
            _context.Branches.RemoveRange(company.Branches);

            // 15. Eliminar Roles
            _context.UserRole.RemoveRange(roles);

            // 16. Eliminar la propia compañía
            _context.Companies.Remove(company);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Empresa con ID {CompanyId} y todos sus datos relacionados eliminados permanentemente", id);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error al eliminar permanentemente la empresa con ID {CompanyId} en cascada", id);
            throw;
        }
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
