using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ParkingApi.Domain.Dtos.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Companies;
using ParkingApi.Domain.Interfaces.Repositories.Users;
using ParkingApi.Domain.Interfaces.Services.Companies;
using ParkingApi.Domain.Interfaces.Services.Realtime;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Data;
using ParkingApi.Infrastructure.Security;

namespace ParkingApi.Core.Services.Companies;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IRealtimeNotificationService _realtimeNotifier;
    private readonly DataContext _context;
    private readonly ILogger<CompanyService> _logger;
    private readonly IMemoryCache? _cache;

    public CompanyService(
        ICompanyRepository companyRepository,
        IUserSessionRepository userSessionRepository,
        IRealtimeNotificationService realtimeNotifier,
        DataContext context,
        ILogger<CompanyService> logger,
        IMemoryCache? cache = null)
    {
        _companyRepository = companyRepository;
        _userSessionRepository = userSessionRepository;
        _realtimeNotifier = realtimeNotifier;
        _context = context;
        _logger = logger;
        _cache = cache;
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

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 3. Resolver Plan y Parámetros
                int? planId = dto.PlanId;
                string planType = dto.PlanType?.Trim() ?? "Personalizado";
                int maxBranches = dto.MaxBranches > 0 ? dto.MaxBranches : 1;
                int maxUsers = dto.MaxUsers > 0 ? dto.MaxUsers : 5;
                bool hasDesktop = dto.HasDesktopAccess;
                bool hasWeb = dto.HasWebAccess;
                bool isCustomPlan = dto.IsCustomPlan;

                if (dto.PlanId.HasValue && dto.PlanId.Value > 0 && !dto.IsCustomPlan)
                {
                    var selectedPlan = await _context.Plans.FindAsync(new object[] { dto.PlanId.Value }, cancellationToken);
                    if (selectedPlan != null)
                    {
                        planId = selectedPlan.Id;
                        planType = selectedPlan.Name;
                        maxBranches = selectedPlan.MaxBranches;
                        maxUsers = selectedPlan.MaxUsers;
                        hasDesktop = selectedPlan.HasDesktopAccess;
                        hasWeb = selectedPlan.HasWebAccess;
                        isCustomPlan = false;
                    }
                }

                // Crear Empresa
                var company = new Company
                {
                    Name = dto.Name.Trim(),
                    LegalName = string.IsNullOrWhiteSpace(dto.LegalName) ? dto.Name.Trim() : dto.LegalName.Trim(),
                    Nit = dto.Nit.Trim(),
                    Email = dto.Email.Trim(),
                    Phone = dto.Phone?.Trim(),
                    Address = dto.Address?.Trim(),
                    City = dto.City?.Trim(),
                    Logo = dto.Logo?.Trim(),
                    PlanId = planId,
                    PlanType = planType,
                    IsCustomPlan = isCustomPlan,
                    MaxBranches = maxBranches,
                    MaxUsers = maxUsers,
                    HasDesktopAccess = hasDesktop,
                    HasWebAccess = hasWeb,
                    CustomModulesWebJson = dto.CustomModulesWebJson,
                    CustomModulesDesktopJson = dto.CustomModulesDesktopJson,
                    IsActive = true,
                    SubscriptionExpiresAt = dto.SubscriptionExpiresAt,
                    AllowMultipleSessions = dto.AllowMultipleSessions,
                    MaxActiveSessionsPerUser = dto.AllowMultipleSessions && dto.MaxActiveSessionsPerUser > 1 ? dto.MaxActiveSessionsPerUser : 1,
                    RequireOpenShiftToOperate = dto.RequireOpenShiftToOperate,
                    AllowMultipleOpenShifts = dto.RequireOpenShiftToOperate && dto.AllowMultipleOpenShifts,
                    MaxOpenShiftsPerUser = dto.RequireOpenShiftToOperate && dto.AllowMultipleOpenShifts && dto.MaxOpenShiftsPerUser > 1 ? dto.MaxOpenShiftsPerUser : 1,
                    RequireInitialCashAmount = dto.RequireOpenShiftToOperate && dto.RequireInitialCashAmount,
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
                var tenantModulesQuery = _context.Module
                    .Where(m => m.IsActive && m.Id != 16 && !m.Name.ToLower().Contains("saas"));

                // Si la empresa no requiere abrir caja para operar, se excluye el Módulo 5 (Control de Turnos y Caja)
                if (!company.RequireOpenShiftToOperate)
                {
                    tenantModulesQuery = tenantModulesQuery.Where(m => m.Id != 5 && !m.Name.ToLower().Contains("turno") && !m.Name.ToLower().Contains("caja"));
                }

                var tenantModules = await tenantModulesQuery.ToListAsync(cancellationToken);
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

                var tenantActionsQuery = _context.Action
                    .Where(a => a.IsActive && a.ModuleId != 16 && !a.Slug.StartsWith("companies."));

                // Si no requiere caja, excluir las acciones de turnos y arqueos (shifts.*)
                if (!company.RequireOpenShiftToOperate)
                {
                    tenantActionsQuery = tenantActionsQuery.Where(a => a.ModuleId != 5 && !a.Slug.StartsWith("shifts."));
                }

                var tenantActions = await tenantActionsQuery.ToListAsync(cancellationToken);
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

                // 6. Crear Sede Inicial Obligatoria para la Empresa (con código único y seguro)
                var branchCode = $"SEDE-{company.Id:D2}";
                var defaultBranch = new Branch
                {
                    CompanyId = company.Id,
                    Code = branchCode,
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
                var nameParts = dto.AdminFullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var firstName = nameParts.Length > 0 ? nameParts[0] : dto.AdminFullName.Trim();
                var firstSurname = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "Admin";

                var user = new User
                {
                    CompanyId = company.Id,
                    UserRoleId = adminRole.Id,
                    IdentificationTypeId = dto.AdminIdentificationTypeId > 0 ? dto.AdminIdentificationTypeId : 1,
                    IdentificationNumber = string.IsNullOrWhiteSpace(dto.AdminIdentificationNumber) ? dto.Nit.Trim() : dto.AdminIdentificationNumber.Trim(),
                    FirstName = firstName,
                    MiddleName = string.Empty,
                    FirstSurname = firstSurname,
                    SecondLastName = string.Empty,
                    FullName = dto.AdminFullName.Trim(),
                    Username = dto.AdminUsername.Trim().ToLowerInvariant(),
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

                // 9. Habilitar Medios de Pago Activos para la Sede Inicial (si existen en la plataforma)
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
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Empresa '{CompanyName}' (Id: {CompanyId}) aprovisionada exitosamente con sede '{BranchCode}' y administrador '{Username}'", company.Name, company.Id, defaultBranch.Code, user.Username);

                return MapToDto(company);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error al crear y aprovisionar la empresa {CompanyName}", dto.Name);
                throw;
            }
        });
    }

    public async Task<CompanyDto> UpdateCompanyAsync(int id, UpdateCompanyDto dto, int? responsibleUserId = null, CancellationToken cancellationToken = default)
    {
        var company = await _companyRepository.GetByIdAsync(id, cancellationToken);
        if (company == null)
        {
            throw new KeyNotFoundException($"Empresa con ID {id} no encontrada.");
        }

        bool multiSessionsDisabled = company.AllowMultipleSessions && !dto.AllowMultipleSessions;

        company.Name = dto.Name.Trim();
        company.LegalName = dto.LegalName?.Trim();
        company.Nit = dto.Nit.Trim();
        company.Email = dto.Email.Trim();
        company.Phone = dto.Phone?.Trim();
        company.Address = dto.Address?.Trim();
        company.City = dto.City?.Trim();
        company.Logo = dto.Logo?.Trim();

        if (dto.PlanId.HasValue && dto.PlanId.Value > 0 && !dto.IsCustomPlan)
        {
            var selectedPlan = await _context.Plans.FindAsync(new object[] { dto.PlanId.Value }, cancellationToken);
            if (selectedPlan != null)
            {
                company.PlanId = selectedPlan.Id;
                company.PlanType = selectedPlan.Name;
                company.MaxBranches = selectedPlan.MaxBranches;
                company.MaxUsers = selectedPlan.MaxUsers;
                company.HasDesktopAccess = selectedPlan.HasDesktopAccess;
                company.HasWebAccess = selectedPlan.HasWebAccess;
                company.IsCustomPlan = false;
            }
        }
        else if (dto.IsCustomPlan)
        {
            company.PlanId = dto.PlanId;
            company.IsCustomPlan = true;
            company.PlanType = string.IsNullOrWhiteSpace(dto.PlanType) ? "Personalizado" : dto.PlanType.Trim();
            company.MaxBranches = dto.MaxBranches > 0 ? dto.MaxBranches : company.MaxBranches;
            company.MaxUsers = dto.MaxUsers > 0 ? dto.MaxUsers : company.MaxUsers;
            company.HasDesktopAccess = dto.HasDesktopAccess;
            company.HasWebAccess = dto.HasWebAccess;
            company.CustomModulesWebJson = dto.CustomModulesWebJson;
            company.CustomModulesDesktopJson = dto.CustomModulesDesktopJson;
        }
        else
        {
            company.PlanId = dto.PlanId;
            company.PlanType = string.IsNullOrWhiteSpace(dto.PlanType) ? company.PlanType : dto.PlanType.Trim();
            company.MaxBranches = dto.MaxBranches > 0 ? dto.MaxBranches : company.MaxBranches;
            company.MaxUsers = dto.MaxUsers > 0 ? dto.MaxUsers : company.MaxUsers;
            company.HasDesktopAccess = dto.HasDesktopAccess;
            company.HasWebAccess = dto.HasWebAccess;
            company.CustomModulesWebJson = dto.CustomModulesWebJson;
            company.CustomModulesDesktopJson = dto.CustomModulesDesktopJson;
        }

        company.IsActive = dto.IsActive;
        company.SubscriptionExpiresAt = dto.SubscriptionExpiresAt;
        company.AllowMultipleSessions = dto.AllowMultipleSessions;
        company.MaxActiveSessionsPerUser = dto.AllowMultipleSessions && dto.MaxActiveSessionsPerUser > 1 ? dto.MaxActiveSessionsPerUser : 1;
        company.RequireOpenShiftToOperate = dto.RequireOpenShiftToOperate;
        company.AllowMultipleOpenShifts = dto.RequireOpenShiftToOperate && dto.AllowMultipleOpenShifts;
        company.MaxOpenShiftsPerUser = dto.RequireOpenShiftToOperate && dto.AllowMultipleOpenShifts && dto.MaxOpenShiftsPerUser > 1 ? dto.MaxOpenShiftsPerUser : 1;
        company.RequireInitialCashAmount = dto.RequireOpenShiftToOperate && dto.RequireInitialCashAmount;
        company.UpdatedAt = DateTime.UtcNow;

        await _companyRepository.UpdateAsync(company, cancellationToken);

        if (multiSessionsDisabled)
        {
            var revokedSessions = await _userSessionRepository.RevokeAllSessionsByCompanyIdAsync(id, "CompanyPolicyDisabled", cancellationToken);
            foreach (var (userId, jti) in revokedSessions)
            {
                _cache?.Remove($"SessionActive_{userId}_{jti}");
                _cache?.Set($"SessionActive_{userId}_{jti}", false, TimeSpan.FromMinutes(10));

                _ = _realtimeNotifier.NotifyCustomAsync(new ParkingApi.Domain.Dtos.Realtime.ConfigNotificationDto
                {
                    EventType = "UserSessionTerminated",
                    CompanyId = id,
                    SessionToken = jti,
                    Title = "Sesión Finalizada por Cambio de Política",
                    Message = "La empresa ha desactivado las sesiones concurrentes. Esta sesión ha sido cerrada automáticamente.",
                    TimestampUtc = DateTime.UtcNow
                }, cancellationToken);
            }
        }

        // Notificación en tiempo real de actualización de parámetros y límites de la empresa
        _ = _realtimeNotifier.NotifyCustomAsync(new ParkingApi.Domain.Dtos.Realtime.ConfigNotificationDto
        {
            EventType = "CompanyUpdated",
            CompanyId = id,
            Title = "Empresa Actualizada",
            Message = $"Los parámetros y cupo de sedes de '{company.Name}' han sido actualizados en tiempo real.",
            TimestampUtc = DateTime.UtcNow
        }, cancellationToken);

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

        _ = _realtimeNotifier.NotifyCustomAsync(new ParkingApi.Domain.Dtos.Realtime.ConfigNotificationDto
        {
            EventType = "CompanyStatusChanged",
            CompanyId = id,
            Title = company.IsActive ? "Empresa Habilitada" : "Empresa Deshabilitada",
            Message = $"El estado operativo de '{company.Name}' ha cambiado a {(company.IsActive ? "Activo" : "Inactivo")}.",
            TimestampUtc = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteCompanyAsync(int id, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
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

                // 7. Eliminar convenios comerciales y sus tiendas
                var stores = await _context.Stores
                    .Where(st => st.CompanyId == id || (st.BranchId.HasValue && branchIds.Contains(st.BranchId.Value)))
                    .ToListAsync(cancellationToken);
                if (stores.Any())
                {
                    var storeIds = stores.Select(s => s.StoreId).ToList();
                    var agreements = await _context.CommercialAgreements
                        .Where(ca => storeIds.Contains(ca.StoreId))
                        .ToListAsync(cancellationToken);
                    _context.CommercialAgreements.RemoveRange(agreements);
                    _context.Stores.RemoveRange(stores);
                }

                // 8. Eliminar tarifas de vehículos
                var rates = await _context.VehicleRates
                    .Where(vr => vr.CompanyId == id || (vr.BranchId.HasValue && branchIds.Contains(vr.BranchId.Value)))
                    .ToListAsync(cancellationToken);
                _context.VehicleRates.RemoveRange(rates);

                // 9. Eliminar mensualidades
                var subscriptions = await _context.MonthlySubscriptions
                    .Where(ms => ms.CompanyId == id || (ms.BranchId.HasValue && branchIds.Contains(ms.BranchId.Value)))
                    .ToListAsync(cancellationToken);
                _context.MonthlySubscriptions.RemoveRange(subscriptions);

                // 11. Eliminar Logins y PasswordResetTokens de usuarios
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
                _logger.LogError(ex, "Error al eliminar empresa {CompanyId}", id);
                throw;
            }
        });
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
            Logo = c.Logo,
            PlanType = c.PlanType,
            PlanId = c.PlanId,
            PlanName = c.Plan?.Name,
            IsCustomPlan = c.IsCustomPlan,
            MaxBranches = c.MaxBranches,
            MaxUsers = c.MaxUsers,
            HasDesktopAccess = c.HasDesktopAccess,
            HasWebAccess = c.HasWebAccess,
            CustomModulesWebJson = c.CustomModulesWebJson,
            CustomModulesDesktopJson = c.CustomModulesDesktopJson,
            IsActive = c.IsActive,
            SubscriptionExpiresAt = c.SubscriptionExpiresAt,
            AllowMultipleSessions = c.AllowMultipleSessions,
            MaxActiveSessionsPerUser = c.MaxActiveSessionsPerUser,
            AllowMultipleOpenShifts = c.AllowMultipleOpenShifts,
            MaxOpenShiftsPerUser = c.MaxOpenShiftsPerUser,
            RequireOpenShiftToOperate = c.RequireOpenShiftToOperate,
            RequireInitialCashAmount = c.RequireInitialCashAmount,
            BranchesCount = c.Branches?.Count ?? 0,
            UsersCount = c.Users?.Count ?? 0,
            CreatedAt = c.CreatedAt
        };
    }
}
