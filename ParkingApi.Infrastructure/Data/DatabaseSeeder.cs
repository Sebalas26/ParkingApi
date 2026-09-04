using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingApi.Domain.Common.Enums;
using ParkingApi.Domain.Models;
using ParkingApi.Infrastructure.Security;
using Action = ParkingApi.Domain.Models.Action;
using PaymentMethodModel = ParkingApi.Domain.Models.PaymentMethod;

namespace ParkingApi.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DataContext context)
    {
        // 1. Tipos de Identificación
        if (!await context.IdentificationType.AnyAsync())
        {
            context.IdentificationType.AddRange(
                new IdentificationType { Id = 1, Identification = "CC", IsActive = true, CreatedAt = DateTime.UtcNow },
                new IdentificationType { Id = 2, Identification = "CE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new IdentificationType { Id = 3, Identification = "NIT", IsActive = true, CreatedAt = DateTime.UtcNow },
                new IdentificationType { Id = 4, Identification = "PAS", IsActive = true, CreatedAt = DateTime.UtcNow },
                new IdentificationType { Id = 5, Identification = "PEP", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        // 2. Roles de Usuario
        if (!await context.UserRole.AnyAsync())
        {
            context.UserRole.AddRange(
                new UserRole { Id = 1, Role = "Super Administrador", IsActive = true, CreatedAt = DateTime.UtcNow },
                new UserRole { Id = 2, Role = "Operador", IsActive = true, CreatedAt = DateTime.UtcNow },
                new UserRole { Id = 3, Role = "Supervisor", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        // 3. Usuario Administrador Inicial
        if (!await context.User.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                UserRoleId = 1,
                IdentificationTypeId = 1,
                IdentificationNumber = "1000000000",
                FirstName = "Administrador",
                MiddleName = "",
                FirstSurname = "Principal",
                SecondLastName = "",
                FullName = "Administrador del Sistema",
                Username = "admin",
                Password = PasswordHasher.HashPassword("Admin2026*"),
                Email = "admin@parkflow.local",
                IsActive = true,
                MustChangePassword = false,
                CreatedAt = DateTime.UtcNow
            };
            context.User.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 4. Módulos del Sistema
        if (!await context.Module.AnyAsync())
        {
            context.Module.AddRange(
                new Module { Id = 1, Name = "Ingreso de Vehículos", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 2, Name = "Salida y Cobro", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 3, Name = "Mensualidades", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 4, Name = "Vehículos en Patio", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 5, Name = "Analítica y Finanzas", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 6, Name = "Control de Turnos", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 7, Name = "Configuración y Sistema", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 8, Name = "Gestión de Tarifas", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 9, Name = "Convenios y Comercios", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Module { Id = 10, Name = "Seguridad y Accesos", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        // 5. Operaciones
        if (!await context.Operation.AnyAsync())
        {
            context.Operation.AddRange(
                new Operation { Id = 1, Name = "READ", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Operation { Id = 2, Name = "CREATE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Operation { Id = 3, Name = "UPDATE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Operation { Id = 4, Name = "DELETE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Operation { Id = 5, Name = "EXECUTE", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Operation { Id = 6, Name = "PRINT", IsActive = true, CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }

        // 6. Acciones del Sistema
        if (!await context.Action.AnyAsync())
        {
            var actions = new List<Action>
            {
                new Action { Id = 1, ModuleId = 1, OperationId = 1, Name = "Ver módulo de ingreso", Slug = "checkin.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 2, ModuleId = 1, OperationId = 2, Name = "Generar e imprimir tiquete de ingreso", Slug = "checkin.create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 3, ModuleId = 1, OperationId = 6, Name = "Reimprimir último tiquete de ingreso", Slug = "checkin.reprint", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 4, ModuleId = 2, OperationId = 1, Name = "Ver módulo de cobro y liquidación", Slug = "checkout.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 5, ModuleId = 2, OperationId = 1, Name = "Buscar tiquete por placa o código", Slug = "checkout.search", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 6, ModuleId = 2, OperationId = 3, Name = "Aplicar convenios y descuentos comerciales", Slug = "checkout.apply_discount", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 7, ModuleId = 2, OperationId = 2, Name = "Procesar cobro y recaudar pago", Slug = "checkout.process_payment", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 8, ModuleId = 2, OperationId = 6, Name = "Reimprimir recibo de liquidación", Slug = "checkout.reprint_receipt", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 9, ModuleId = 2, OperationId = 5, Name = "Anular o reversar cobro de tiquete", Slug = "checkout.cancel", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 10, ModuleId = 2, OperationId = 5, Name = "Apertura manual de talanquera", Slug = "checkout.manual_barrier_open", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 11, ModuleId = 3, OperationId = 1, Name = "Ver módulo de mensualidades", Slug = "subscriptions.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 12, ModuleId = 3, OperationId = 2, Name = "Crear nueva suscripción de mensualidad", Slug = "subscriptions.create", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 13, ModuleId = 3, OperationId = 2, Name = "Renovar / recaudar cuota de mensualidad", Slug = "subscriptions.renew", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 14, ModuleId = 3, OperationId = 3, Name = "Editar datos de abonado y vehículo", Slug = "subscriptions.edit", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 15, ModuleId = 3, OperationId = 4, Name = "Cancelar / inactivar mensualidad", Slug = "subscriptions.cancel", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 16, ModuleId = 4, OperationId = 1, Name = "Ver vehículos en patio y recientes", Slug = "recent_entries.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 17, ModuleId = 4, OperationId = 6, Name = "Exportar listado de vehículos en patio", Slug = "recent_entries.export", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 18, ModuleId = 5, OperationId = 1, Name = "Ver dashboard financiero y analítica", Slug = "analytics.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 19, ModuleId = 5, OperationId = 1, Name = "Consultar métricas de ocupación e ingresos", Slug = "analytics.metrics", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 20, ModuleId = 5, OperationId = 6, Name = "Exportar informes contables y balance", Slug = "analytics.export", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 21, ModuleId = 6, OperationId = 1, Name = "Ver balance de turno y arqueo de caja", Slug = "shift.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 22, ModuleId = 6, OperationId = 2, Name = "Apertura de turno operativo", Slug = "shift.open", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 23, ModuleId = 6, OperationId = 5, Name = "Registrar retiros o sangrías", Slug = "shift.cash_withdrawal", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 24, ModuleId = 6, OperationId = 3, Name = "Cierre definitivo de turno", Slug = "shift.close", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 25, ModuleId = 6, OperationId = 3, Name = "Entrega y relevo de turno", Slug = "shift.handover", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 26, ModuleId = 6, OperationId = 1, Name = "Consultar histórico de turnos", Slug = "shift.history", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 27, ModuleId = 7, OperationId = 5, Name = "Ejecutar sincronización manual", Slug = "system.sync", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 28, ModuleId = 7, OperationId = 5, Name = "Limpiar caché local", Slug = "system.clean_cache", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 29, ModuleId = 7, OperationId = 3, Name = "Cambiar tema visual de la interfaz", Slug = "system.theme", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 30, ModuleId = 8, OperationId = 1, Name = "Ver catálogo de tarifas vehiculares", Slug = "rates.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 31, ModuleId = 8, OperationId = 3, Name = "Crear, editar y parametrizar tarifas", Slug = "rates.manage", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 32, ModuleId = 9, OperationId = 1, Name = "Ver convenios comerciales y aliados", Slug = "agreements.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 33, ModuleId = 9, OperationId = 3, Name = "Crear y administrar convenios y descuentos", Slug = "agreements.manage", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 34, ModuleId = 10, OperationId = 1, Name = "Ver usuarios, roles y permisos", Slug = "security.view", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 35, ModuleId = 10, OperationId = 3, Name = "Administrar usuarios y contraseñas", Slug = "users.manage", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 36, ModuleId = 10, OperationId = 3, Name = "Administrar roles y permisos del sistema", Slug = "roles.manage", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Action { Id = 37, ModuleId = 10, OperationId = 3, Name = "Asignar permisos y accesos a roles", Slug = "permissions.assign", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Action.AddRange(actions);
            await context.SaveChangesAsync();
        }

        // 7. Asignar RoleActions y UserRoleModule al Administrador (Rol 1)
        if (!await context.RoleAction.AnyAsync(ra => ra.RoleId == 1))
        {
            var allActions = await context.Action.ToListAsync();
            foreach (var action in allActions)
            {
                context.RoleAction.Add(new RoleAction
                {
                    RoleId = 1,
                    ActionId = action.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        if (!await context.UserRoleModule.AnyAsync(urm => urm.UserRoleId == 1))
        {
            var allModules = await context.Module.ToListAsync();
            foreach (var mod in allModules)
            {
                context.UserRoleModule.Add(new UserRoleModule
                {
                    UserRoleId = 1,
                    ModulesRoleId = mod.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Solo se inicializan Tipos de Identificación, Roles, Usuario Administrador Inicial, Módulos, Operaciones y Acciones RBAC.
        // Las Sedes (Branches), Medios de Pago (PaymentMethods), Tarifas Vehiculares (VehicleRates) y Convenios (Stores/Agreements)
        // se deben crear MANUALMENTE desde la PWA (Zero-Data Bootstrap).
    }

    public static async Task SeedWpfActionsAsync(DataContext context)
    {
        try
        {
            var wpfActions = new List<(string Slug, string Name, int ModuleId, int OperationId)>
            {
                // Módulo 1: CheckIn WPF POS
                ("wpf.checkin.view", "Ver terminal de ingreso WPF", 1, 1),
                ("wpf.checkin.create", "Generar e imprimir tiquete en terminal WPF", 1, 2),
                ("wpf.checkin.reprint", "Reimprimir tiquete de ingreso en terminal WPF", 1, 6),
                ("wpf.checkin.edit_plate", "Editar datos o placa en ingreso WPF", 1, 3),
                ("wpf.checkin.manual_barrier", "Abrir barrera de entrada manualmente en terminal WPF", 1, 5),

                // Módulo 2: CheckOut WPF POS
                ("wpf.checkout.view", "Ver terminal de cobro y salida WPF", 2, 1),
                ("wpf.checkout.process_payment", "Liquidar y cobrar tiquete en terminal WPF", 2, 2),
                ("wpf.checkout.apply_discount", "Aplicar convenios y descuentos en terminal WPF", 2, 2),
                ("wpf.checkout.waive_fee", "Exonerar cobro de tiquete en terminal WPF", 2, 3),
                ("wpf.checkout.reprint_receipt", "Reimprimir factura o comprobante en terminal WPF", 2, 6),
                ("wpf.checkout.manual_barrier", "Abrir barrera de salida manualmente en terminal WPF", 2, 5),

                // Módulo 3: Mensualidades y Abonados WPF POS
                ("wpf.subscriptions.view", "Ver catálogo de mensualidades en terminal WPF", 3, 1),
                ("wpf.subscriptions.create", "Registrar nuevo abonado en terminal WPF", 3, 2),
                ("wpf.subscriptions.renew", "Renovar mensualidad en terminal WPF", 3, 3),
                ("wpf.subscriptions.cancel", "Cancelar suscripción en terminal WPF", 3, 4),

                // Módulo 4: Monitoreo de Patio WPF POS
                ("wpf.monitoring.view_occupancy", "Ver mapa de ocupación en terminal WPF", 4, 1),
                ("wpf.monitoring.search_vehicles", "Buscar vehículos adentro en terminal WPF", 4, 1),
                ("wpf.monitoring.force_exit", "Forzar salida manual en terminal WPF", 4, 3),
                ("wpf.monitoring.export", "Exportar vehículos activos desde terminal WPF", 4, 1),

                // Módulo 5: Control de Turnos y Caja WPF POS
                ("wpf.shifts.view_current", "Ver turno actual y balance en terminal WPF", 5, 1),
                ("wpf.shifts.open", "Abrir turno con base en terminal WPF", 5, 2),
                ("wpf.shifts.blind_count", "Arqueo ciego / retiro de efectivo en terminal WPF", 5, 5),
                ("wpf.shifts.close", "Cerrar turno de caja y corte Z en terminal WPF", 5, 5),
                ("wpf.shifts.view_history", "Ver historial de turnos en terminal WPF", 5, 1),
                ("wpf.shifts.reprint_closure", "Reimprimir comprobante de cierre en terminal WPF", 5, 6)
            };

            bool hasChanges = false;
            var existingActions = await context.Action.Where(a => a.Slug.StartsWith("wpf.")).ToListAsync();
            var existingSlugs = existingActions.Select(a => a.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var def in wpfActions)
            {
                if (!existingSlugs.Contains(def.Slug))
                {
                    var newAction = new Action
                    {
                        Slug = def.Slug,
                        Name = def.Name,
                        ModuleId = def.ModuleId,
                        OperationId = def.OperationId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Action.Add(newAction);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }

            // Asignar al Rol 1 (Super Administrador)
            var unassignedSuperAdmin = await context.Action
                .Where(a => a.Slug.StartsWith("wpf.") && !context.RoleAction.Any(ra => ra.RoleId == 1 && ra.ActionId == a.Id))
                .ToListAsync();

            if (unassignedSuperAdmin.Any())
            {
                foreach (var act in unassignedSuperAdmin)
                {
                    context.RoleAction.Add(new RoleAction
                    {
                        RoleId = 1,
                        ActionId = act.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SeedWpfActionsAsync] Error sembrando acciones wpf.*: {ex.Message}");
        }
    }
}
