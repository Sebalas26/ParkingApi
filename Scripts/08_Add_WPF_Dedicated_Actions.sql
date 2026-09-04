-- ==================================================================================
-- Script: 08_Add_WPF_Dedicated_Actions.sql
-- Propósito: Desacoplar e independizar los permisos de terminal WPF frente a PWA
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

INSERT INTO `Action` (`Id`, `ModuleId`, `OperationId`, `Name`, `Slug`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
VALUES
    -- Módulo 1: Ingreso de Vehículos (CheckIn WPF POS)
    (83,  1, 1, 'Ver terminal de ingreso WPF', 'wpf.checkin.view', 1, UTC_TIMESTAMP(), NULL),
    (84,  1, 2, 'Generar e imprimir tiquete en terminal WPF', 'wpf.checkin.create', 1, UTC_TIMESTAMP(), NULL),
    (85,  1, 6, 'Reimprimir tiquete de ingreso en terminal WPF', 'wpf.checkin.reprint', 1, UTC_TIMESTAMP(), NULL),
    (86,  1, 3, 'Editar datos o placa en ingreso WPF', 'wpf.checkin.edit_plate', 1, UTC_TIMESTAMP(), NULL),
    (87,  1, 5, 'Abrir barrera de entrada manualmente en terminal WPF', 'wpf.checkin.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- Módulo 2: Salida y Cobro (CheckOut WPF POS)
    (88,  2, 1, 'Ver terminal de cobro y salida WPF', 'wpf.checkout.view', 1, UTC_TIMESTAMP(), NULL),
    (89,  2, 2, 'Liquidar y cobrar tiquete en terminal WPF', 'wpf.checkout.process_payment', 1, UTC_TIMESTAMP(), NULL),
    (90,  2, 2, 'Aplicar convenios y descuentos en terminal WPF', 'wpf.checkout.apply_discount', 1, UTC_TIMESTAMP(), NULL),
    (91,  2, 3, 'Exonerar cobro de tiquete en terminal WPF', 'wpf.checkout.waive_fee', 1, UTC_TIMESTAMP(), NULL),
    (92,  2, 6, 'Reimprimir factura o comprobante en terminal WPF', 'wpf.checkout.reprint_receipt', 1, UTC_TIMESTAMP(), NULL),
    (93,  2, 5, 'Abrir barrera de salida manualmente en terminal WPF', 'wpf.checkout.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- Módulo 3: Mensualidades y Abonados (WPF POS)
    (94,  3, 1, 'Ver catálogo de mensualidades en terminal WPF', 'wpf.subscriptions.view', 1, UTC_TIMESTAMP(), NULL),
    (95,  3, 2, 'Registrar nuevo abonado en terminal WPF', 'wpf.subscriptions.create', 1, UTC_TIMESTAMP(), NULL),
    (96,  3, 3, 'Renovar mensualidad en terminal WPF', 'wpf.subscriptions.renew', 1, UTC_TIMESTAMP(), NULL),
    (97,  3, 4, 'Cancelar suscripción en terminal WPF', 'wpf.subscriptions.cancel', 1, UTC_TIMESTAMP(), NULL),

    -- Módulo 4: Monitoreo de Patio (WPF POS)
    (98,  4, 1, 'Ver mapa de ocupación en terminal WPF', 'wpf.monitoring.view_occupancy', 1, UTC_TIMESTAMP(), NULL),
    (99,  4, 1, 'Buscar vehículos adentro en terminal WPF', 'wpf.monitoring.search_vehicles', 1, UTC_TIMESTAMP(), NULL),
    (100, 4, 3, 'Forzar salida manual en terminal WPF', 'wpf.monitoring.force_exit', 1, UTC_TIMESTAMP(), NULL),
    (101, 4, 1, 'Exportar vehículos activos desde terminal WPF', 'wpf.monitoring.export', 1, UTC_TIMESTAMP(), NULL),

    -- Módulo 5: Control de Turnos y Caja (WPF POS)
    (102, 5, 1, 'Ver turno actual y balance en terminal WPF', 'wpf.shifts.view_current', 1, UTC_TIMESTAMP(), NULL),
    (103, 5, 2, 'Abrir turno con base en terminal WPF', 'wpf.shifts.open', 1, UTC_TIMESTAMP(), NULL),
    (104, 5, 5, 'Arqueo ciego / retiro de efectivo en terminal WPF', 'wpf.shifts.blind_count', 1, UTC_TIMESTAMP(), NULL),
    (105, 5, 5, 'Cerrar turno de caja y corte Z en terminal WPF', 'wpf.shifts.close', 1, UTC_TIMESTAMP(), NULL),
    (106, 5, 1, 'Ver historial de turnos en terminal WPF', 'wpf.shifts.view_history', 1, UTC_TIMESTAMP(), NULL),
    (107, 5, 6, 'Reimprimir comprobante de cierre en terminal WPF', 'wpf.shifts.reprint_closure', 1, UTC_TIMESTAMP(), NULL)
ON DUPLICATE KEY UPDATE 
    ModuleId = VALUES(ModuleId),
    OperationId = VALUES(OperationId),
    Name = VALUES(Name),
    Slug = VALUES(Slug),
    IsActive = VALUES(IsActive);

-- Asignar las nuevas acciones wpf.* al Rol 1 (Super Administrador)
INSERT IGNORE INTO `RoleAction` (`RoleId`, `ActionId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1
FROM `Action`
WHERE `Slug` LIKE 'wpf.%';

SET FOREIGN_KEY_CHECKS = 1;
