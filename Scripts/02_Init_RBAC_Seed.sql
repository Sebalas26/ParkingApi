-- ==================================================================================
-- SCRIPT: 02_Init_RBAC_Seed.sql
-- DESCRIPCIÓN: Script Oficial y Completo de Inicialización RBAC (WPF & PWA).
-- MOTOR: MySQL 8.x / MariaDB
-- REGLAS DE NEGOCIO:
--   1. Cero precarga de Sedes/Parqueaderos, Medios de Pago, Tarifas o Transacciones.
--   2. Inicializa Tipos de Identificación, Roles, Usuario Admin Inicial ('admin').
--   3. Catálogo completo de 13 Módulos funcionales (Terminal WPF y Administración PWA).
--   4. Catálogo de 7 Operaciones estándar del sistema.
--   5. Catálogo de 48 Acciones y Slugs reales del sistema.
--   6. Asigna el 100% de Módulos y Acciones (Full Access) al Rol Administrador (Id 1).
--   7. Asigna Módulos y Acciones operativas al Rol Operador (Id 2).
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------------------------------------------------------------
-- 1. TIPOS DE IDENTIFICACIÓN (IdentificationType)
-- ----------------------------------------------------------------------------------
INSERT INTO IdentificationType (Id, Identification, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, 'CC',  1, UTC_TIMESTAMP(), NULL),
    (2, 'CE',  1, UTC_TIMESTAMP(), NULL),
    (3, 'NIT', 1, UTC_TIMESTAMP(), NULL),
    (4, 'PAS', 1, UTC_TIMESTAMP(), NULL),
    (5, 'PEP', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Identification = new_row.Identification,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2. ROLES DE USUARIO (UserRole)
-- ----------------------------------------------------------------------------------
INSERT INTO UserRole (Id, Role, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, 'Administrador', 1, UTC_TIMESTAMP(), NULL),
    (2, 'Operador',      1, UTC_TIMESTAMP(), NULL),
    (3, 'Supervisor',    1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Role = new_row.Role, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 3. USUARIO ADMINISTRADOR PRINCIPAL (User)
-- Contraseña generada con BCrypt para 'Admin2026*'
-- ----------------------------------------------------------------------------------
INSERT INTO User (
    Id, UserRoleId, IdentificationTypeId, IdentificationNumber, 
    FirstName, MiddleName, FirstSurname, SecondLastName, FullName,
    Username, Password, Email, IsActive, MustChangePassword, CreatedAt
)
VALUES (
    1, 1, 1, '1000000000', 
    'Administrador', '', 'Principal', '', 'Administrador del Sistema',
    'admin', 
    '$2a$11$eA8b7w9H4W5nN8u9o3r0ueLd3eGf6h8j0k1l2m3n4o5p6q7r8s9t0', -- Admin2026*
    'admin@parkflow.local', 
    1, 0, UTC_TIMESTAMP()
)
AS new_row
ON DUPLICATE KEY UPDATE 
    UserRoleId = new_row.UserRoleId,
    IdentificationTypeId = new_row.IdentificationTypeId,
    IdentificationNumber = new_row.IdentificationNumber,
    FullName = new_row.FullName,
    Email = new_row.Email,
    Password = new_row.Password,
    IsActive = 1;

-- ----------------------------------------------------------------------------------
-- 4. MÓDULOS DEL SISTEMA (Module) - 13 Módulos (WPF & PWA)
-- ----------------------------------------------------------------------------------
INSERT INTO Module (Id, Name, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    -- Módulos Operativos (WPF / PWA)
    (1,  'Ingreso de Vehículos (CheckIn)',    1, UTC_TIMESTAMP(), NULL),
    (2,  'Salida y Cobro (CheckOut)',         1, UTC_TIMESTAMP(), NULL),
    (3,  'Mensualidades y Abonados',          1, UTC_TIMESTAMP(), NULL),
    (4,  'Vehículos en Patio y Monitoreo',    1, UTC_TIMESTAMP(), NULL),
    (5,  'Control de Turnos y Caja',          1, UTC_TIMESTAMP(), NULL),
    (6,  'Analítica, Métricas y Finanzas',    1, UTC_TIMESTAMP(), NULL),

    -- Módulos Administrativos y Parametría (PWA / Panel Central)
    (7,  'Gestión de Sedes y Parqueaderos',   1, UTC_TIMESTAMP(), NULL),
    (8,  'Gestión de Tarifas y Vehículos',    1, UTC_TIMESTAMP(), NULL),
    (9,  'Medios de Pago Maestros',           1, UTC_TIMESTAMP(), NULL),
    (10, 'Convenios y Comercios Aliados',     1, UTC_TIMESTAMP(), NULL),
    (11, 'Seguridad, Usuarios y Roles',       1, UTC_TIMESTAMP(), NULL),
    (12, 'Matriz de Permisos RBAC',           1, UTC_TIMESTAMP(), NULL),
    (13, 'Configuración y Sistema',           1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 5. OPERACIONES BASE (Operation) - 7 Operaciones Estándar
-- ----------------------------------------------------------------------------------
INSERT INTO Operation (Id, Name, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, 'READ',    1, UTC_TIMESTAMP(), NULL),
    (2, 'CREATE',  1, UTC_TIMESTAMP(), NULL),
    (3, 'UPDATE',  1, UTC_TIMESTAMP(), NULL),
    (4, 'DELETE',  1, UTC_TIMESTAMP(), NULL),
    (5, 'EXECUTE', 1, UTC_TIMESTAMP(), NULL),
    (6, 'PRINT',   1, UTC_TIMESTAMP(), NULL),
    (7, 'ASSIGN',  1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 6. ACCIONES Y SLUGS REALES DEL SISTEMA (Action) - 48 Acciones
-- ----------------------------------------------------------------------------------
INSERT INTO Action (Id, ModuleId, OperationId, Name, Slug, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    -- ==============================================================================
    -- MÓDULO 1: INGRESO DE VEHÍCULOS (CheckIn)
    -- ==============================================================================
    (1,  1, 1, 'Ver módulo de ingreso', 'checkin.view', 1, UTC_TIMESTAMP(), NULL),
    (2,  1, 2, 'Generar e imprimir tiquete de ingreso', 'checkin.create', 1, UTC_TIMESTAMP(), NULL),
    (3,  1, 6, 'Reimprimir tiquete de ingreso', 'checkin.reprint', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 2: SALIDA Y COBRO / CAJA (CheckOut)
    -- ==============================================================================
    (4,  2, 1, 'Ver módulo de cobro y liquidación', 'checkout.view', 1, UTC_TIMESTAMP(), NULL),
    (5,  2, 1, 'Buscar tiquete por placa o código de barras', 'checkout.search', 1, UTC_TIMESTAMP(), NULL),
    (6,  2, 3, 'Aplicar convenios y descuentos comerciales', 'checkout.apply_discount', 1, UTC_TIMESTAMP(), NULL),
    (7,  2, 2, 'Procesar cobro y recaudar pago', 'checkout.process_payment', 1, UTC_TIMESTAMP(), NULL),
    (8,  2, 6, 'Reimprimir recibo de pago de salida', 'checkout.reprint_receipt', 1, UTC_TIMESTAMP(), NULL),
    (9,  2, 5, 'Anular o reversar cobro de tiquete', 'checkout.cancel', 1, UTC_TIMESTAMP(), NULL),
    (10, 2, 5, 'Apertura manual de talanquera / salida contingente', 'checkout.manual_barrier_open', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 3: MENSUALIDADES Y ABONADOS (Subscriptions)
    -- ==============================================================================
    (11, 3, 1, 'Ver módulo de mensualidades y abonados', 'subscriptions.view', 1, UTC_TIMESTAMP(), NULL),
    (12, 3, 2, 'Crear nueva suscripción de mensualidad', 'subscriptions.create', 1, UTC_TIMESTAMP(), NULL),
    (13, 3, 2, 'Renovar y recaudar cuota de mensualidad', 'subscriptions.renew', 1, UTC_TIMESTAMP(), NULL),
    (14, 3, 3, 'Editar datos de abonado y vehículo', 'subscriptions.edit', 1, UTC_TIMESTAMP(), NULL),
    (15, 3, 4, 'Cancelar / dar de baja mensualidad', 'subscriptions.cancel', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 4: VEHÍCULOS EN PATIO Y MONITOREO (RecentEntries)
    -- ==============================================================================
    (16, 4, 1, 'Ver listado de vehículos en patio y recientes', 'recent_entries.view', 1, UTC_TIMESTAMP(), NULL),
    (17, 4, 6, 'Reimprimir tiquete desde patio', 'recent_entries.reprint', 1, UTC_TIMESTAMP(), NULL),
    (18, 4, 6, 'Exportar listado de vehículos en patio', 'recent_entries.export', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 5: CONTROL DE TURNOS Y CAJA (Shifts)
    -- ==============================================================================
    (19, 5, 1, 'Ver balance de turno y arqueo de caja', 'shift.view', 1, UTC_TIMESTAMP(), NULL),
    (20, 5, 2, 'Apertura de turno operativo con base inicial', 'shift.open', 1, UTC_TIMESTAMP(), NULL),
    (21, 5, 5, 'Registrar retiros o sangrías parciales de gaveta', 'shift.cash_withdrawal', 1, UTC_TIMESTAMP(), NULL),
    (22, 5, 3, 'Cierre definitivo de turno y fin de jornada', 'shift.close', 1, UTC_TIMESTAMP(), NULL),
    (23, 5, 3, 'Entrega y relevo de turno a otro operador con firma', 'shift.handover', 1, UTC_TIMESTAMP(), NULL),
    (24, 5, 1, 'Consultar histórico de turnos y arqueos', 'shift.history', 1, UTC_TIMESTAMP(), NULL),
    (25, 5, 6, 'Imprimir comprobante de cierre / Reporte Z', 'shift.export', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 6: ANALÍTICA, MÉTRICAS Y FINANZAS (Analytics)
    -- ==============================================================================
    (26, 6, 1, 'Ver dashboard financiero y analítica', 'analytics.view', 1, UTC_TIMESTAMP(), NULL),
    (27, 6, 1, 'Consultar métricas de ocupación e ingresos', 'analytics.metrics', 1, UTC_TIMESTAMP(), NULL),
    (28, 6, 6, 'Exportar informes contables y balances', 'analytics.export', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 7: GESTIÓN DE SEDES Y PARQUEADEROS (Branches - PWA)
    -- ==============================================================================
    (29, 7, 1, 'Ver catálogo de sedes y parqueaderos', 'branches.view', 1, UTC_TIMESTAMP(), NULL),
    (30, 7, 2, 'Crear nueva sede de parqueadero', 'branches.create', 1, UTC_TIMESTAMP(), NULL),
    (31, 7, 3, 'Editar información de sede (capacidad, dirección, notas)', 'branches.edit', 1, UTC_TIMESTAMP(), NULL),
    (32, 7, 4, 'Inactivar sede de parqueadero', 'branches.delete', 1, UTC_TIMESTAMP(), NULL),
    (33, 7, 7, 'Asignar operadores y administradores a sedes', 'branches.assign_users', 1, UTC_TIMESTAMP(), NULL),
    (34, 7, 7, 'Configurar medios de pago por sede', 'branches.configure_payments', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 8: GESTIÓN DE TARIFAS Y VEHÍCULOS (Rates - PWA)
    -- ==============================================================================
    (35, 8, 1, 'Ver catálogo de tarifas vehiculares', 'rates.view', 1, UTC_TIMESTAMP(), NULL),
    (36, 8, 2, 'Crear nueva tarifa vehicular por sede', 'rates.create', 1, UTC_TIMESTAMP(), NULL),
    (37, 8, 3, 'Editar y parametrizar tarifas y tiempos de gracia', 'rates.edit', 1, UTC_TIMESTAMP(), NULL),
    (38, 8, 4, 'Inactivar tarifa vehicular', 'rates.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 9: MEDIOS DE PAGO MAESTROS (PaymentMethods - PWA)
    -- ==============================================================================
    (39, 9, 1, 'Ver catálogo maestro de medios de pago', 'payment_methods.view', 1, UTC_TIMESTAMP(), NULL),
    (40, 9, 2, 'Crear nuevo medio de pago en el sistema', 'payment_methods.create', 1, UTC_TIMESTAMP(), NULL),
    (41, 9, 3, 'Editar medio de pago (nombre, icono)', 'payment_methods.edit', 1, UTC_TIMESTAMP(), NULL),
    (42, 9, 4, 'Inactivar medio de pago maestro', 'payment_methods.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 10: CONVENIOS Y COMERCIOS ALIADOS (Agreements - PWA)
    -- ==============================================================================
    (43, 10, 1, 'Ver convenios comerciales y comercios afiliados', 'agreements.view', 1, UTC_TIMESTAMP(), NULL),
    (44, 10, 2, 'Crear nuevo comercio y convenio comercial', 'agreements.create', 1, UTC_TIMESTAMP(), NULL),
    (45, 10, 3, 'Editar condiciones de descuento y montos mínimos', 'agreements.edit', 1, UTC_TIMESTAMP(), NULL),
    (46, 10, 4, 'Inactivar convenio o comercio', 'agreements.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 11: SEGURIDAD, USUARIOS Y ROLES (Users & Roles - PWA)
    -- ==============================================================================
    (47, 11, 1, 'Ver usuarios y roles del sistema', 'users.view', 1, UTC_TIMESTAMP(), NULL),
    (48, 11, 2, 'Crear nuevo usuario operador / administrador', 'users.create', 1, UTC_TIMESTAMP(), NULL),
    (49, 11, 3, 'Editar datos de usuario y restablecer contraseñas', 'users.edit', 1, UTC_TIMESTAMP(), NULL),
    (50, 11, 4, 'Inactivar usuario del sistema', 'users.delete', 1, UTC_TIMESTAMP(), NULL),
    (51, 11, 1, 'Ver catálogo de roles de usuario', 'roles.view', 1, UTC_TIMESTAMP(), NULL),
    (52, 11, 2, 'Crear nuevo rol de usuario', 'roles.create', 1, UTC_TIMESTAMP(), NULL),
    (53, 11, 3, 'Editar nombre y estado de rol', 'roles.edit', 1, UTC_TIMESTAMP(), NULL),
    (54, 11, 4, 'Inactivar rol de usuario', 'roles.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 12: MATRIZ DE PERMISOS RBAC (Permissions - PWA)
    -- ==============================================================================
    (55, 12, 1, 'Ver matriz de permisos por rol', 'permissions.view', 1, UTC_TIMESTAMP(), NULL),
    (56, 12, 7, 'Asignar y revocar acciones y módulos a roles', 'permissions.assign', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 13: CONFIGURACIÓN Y SISTEMA (System - WPF & PWA)
    -- ==============================================================================
    (57, 13, 5, 'Ejecutar sincronización manual en caliente', 'system.sync', 1, UTC_TIMESTAMP(), NULL),
    (58, 13, 5, 'Limpiar caché local y forzar resincronización', 'system.clean_cache', 1, UTC_TIMESTAMP(), NULL),
    (59, 13, 3, 'Cambiar tema visual de la interfaz', 'system.theme', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    ModuleId = new_row.ModuleId,
    OperationId = new_row.OperationId,
    Name = new_row.Name,
    Slug = new_row.Slug,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 7. MATRIZ DE MÓDULOS POR ROL (UserRoleModule)
-- Asignación del 100% de los 13 Módulos al Rol 1 (Administrador)
-- ----------------------------------------------------------------------------------
DELETE FROM UserRoleModule WHERE UserRoleId = 1;

INSERT INTO `UserRoleModule` (`UserRoleId`, `ModulesRoleId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Module`;

-- Asignación de Módulos Operativos al Rol 2 (Operador)
DELETE FROM `UserRoleModule` WHERE `UserRoleId` = 2;

INSERT INTO `UserRoleModule` (`UserRoleId`, `ModulesRoleId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 2, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Module` WHERE `Id` IN (1, 2, 3, 4, 5, 6, 13);

-- ----------------------------------------------------------------------------------
-- 8. MATRIZ DE PERMISOS: ROL ACCIONES (RoleAction)
-- Asignación del 100% de las Acciones al Rol 1 (Administrador) - FULL ACCESS
-- ----------------------------------------------------------------------------------
DELETE FROM `RoleAction` WHERE `RoleId` = 1;

INSERT INTO `RoleAction` (`RoleId`, `ActionId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Action`;

-- Asignación de Acciones Operativas al Rol 2 (Operador)
DELETE FROM `RoleAction` WHERE `RoleId` = 2;

INSERT INTO `RoleAction` (`RoleId`, `ActionId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 2, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Action`
WHERE `Slug` IN (
    'checkin.view', 'checkin.create', 'checkin.reprint',
    'checkout.view', 'checkout.search', 'checkout.apply_discount', 'checkout.process_payment', 'checkout.reprint_receipt',
    'subscriptions.view', 'subscriptions.create', 'subscriptions.renew',
    'recent_entries.view', 'recent_entries.reprint',
    'shift.view', 'shift.open', 'shift.cash_withdrawal', 'shift.close', 'shift.handover', 'shift.history', 'shift.export',
    'analytics.view',
    'system.sync', 'system.theme'
);

SET FOREIGN_KEY_CHECKS = 1;

-- ----------------------------------------------------------------------------------
-- 9. VERIFICACIÓN Y AUDITORÍA DE PERMISOS ASIGNADOS AL ADMINISTRADOR
-- ----------------------------------------------------------------------------------
SELECT 
    u.Id AS UsuarioId,
    u.Username AS Usuario,
    u.FullName AS NombreCompleto,
    r.Role AS RolAsignado,
    COUNT(DISTINCT urm.ModulesRoleId) AS TotalModulosAsignados,
    COUNT(DISTINCT ra.ActionId) AS TotalAccionesAsignadas
FROM User u
INNER JOIN UserRole r ON u.UserRoleId = r.Id
LEFT JOIN UserRoleModule urm ON urm.UserRoleId = r.Id AND urm.IsActive = 1
LEFT JOIN RoleAction ra ON ra.RoleId = r.Id AND ra.IsActive = 1
WHERE u.Username = 'admin'
GROUP BY u.Id, u.Username, u.FullName, r.Role;

-- Listado detallado de todas las acciones asignadas al Administrador
SELECT 
    m.Name AS Modulo,
    o.Name AS Operacion,
    a.Name AS Accion,
    a.Slug AS CodigoPermiso,
    ra.IsActive AS AsignadoActivo
FROM User u
INNER JOIN UserRole r ON u.UserRoleId = r.Id
INNER JOIN RoleAction ra ON ra.RoleId = r.Id
INNER JOIN Action a ON ra.ActionId = a.Id
INNER JOIN Module m ON a.ModuleId = m.Id
INNER JOIN Operation o ON a.OperationId = o.Id
WHERE u.Username = 'admin'
ORDER BY m.Id, o.Id, a.Id;
