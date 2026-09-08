use db_acd7d6_parking;

-- ==================================================================================
-- SCRIPT: 03_Reset_Operational_Data_Keep_SuperAdmin.sql
-- DESCRIPCIÓN: Limpieza profunda y segura de toda la data transaccional, operativa y
--              empresas SaaS, preservando intacta la estructura DDL, el catálogo RBAC
--              (Módulos, Operaciones, Acciones), el Rol 'Super Administrador' (Id 1)
--              y el Usuario 'admin' (Id 1, CompanyId NULL).
-- MOTOR: MySQL 8.x / MariaDB
-- CASO DE USO: Arrancar el sistema SaaS en estado limpio desde cero sin necesidad de
--              hacer DROP TABLES ni reejecutar migraciones.
-- FECHA: 2026-09-08
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- 1. Limpieza de notificaciones y sesiones
DELETE FROM `PushSubscriptions`;
DELETE FROM `UserNotificationPreferences` WHERE `UserId` NOT IN (1);
DELETE FROM `UserSessions`;

-- 2. Limpieza de horarios, medios de pago y convenios específicos de sedes
DELETE FROM `BranchOperatingHours`;
DELETE FROM `BranchCommercialAgreements`;
DELETE FROM `BranchPaymentMethods`;
DELETE FROM `UserBranches`;
DELETE FROM `VehicleIncidentBranches`;
DELETE FROM `UserParkings`;
DELETE FROM `userparkings` WHERE 1=1;

-- 3. Limpieza de transacciones operativas: tickets, turnos, resoluciones, siniestros y mensualidades
DELETE FROM `TicketDiscounts`;
DELETE FROM `CommercialAgreements`;
DELETE FROM `Stores`;
DELETE FROM `ParkingTickets`;
DELETE FROM `WorkShifts`;
DELETE FROM `BillingResolutions`;
DELETE FROM `VehicleIncidents`;
DELETE FROM `MonthlySubscriptions`;
DELETE FROM `VehicleRates`;
DELETE FROM `ParkingLots`;
DELETE FROM `parkinglots` WHERE 1=1;

-- 4. Limpieza de Sedes y Empresas
DELETE FROM `Branches`;
DELETE FROM `Companies`;
DELETE FROM `Company` WHERE 1=1;

-- 5. Limpieza de Planes de suscripción dinámicos
DELETE FROM `Plans`;

-- 6. Limpieza de credenciales y tokens de usuarios de empresas (Preservando SuperAdmin Id = 1)
DELETE FROM `PasswordResetToken` WHERE `UserId` NOT IN (1);
DELETE FROM `Login` WHERE `UserId` NOT IN (1);

-- 7. Limpieza de roles y permisos personalizados de empresas
-- Se eliminan vínculos en RoleAction y UserRoleModule para roles que no sean el Super Administrador (Id 1)
DELETE FROM `RoleAction` WHERE `RoleId` NOT IN (1);
DELETE FROM `UserRoleModule` WHERE `UserRoleId` NOT IN (1);
DELETE FROM `UserRole` WHERE `Id` NOT IN (1) OR `CompanyId` IS NOT NULL;

-- 8. Limpieza de usuarios (Preservando exclusivamente al Super Administrador Id = 1, CompanyId NULL)
DELETE FROM `User` WHERE `Id` NOT IN (1) OR `CompanyId` IS NOT NULL;

-- 9. Asegurar que el usuario 'admin' mantenga su rol Super Administrador (Id 1) y CompanyId en NULL
UPDATE `User` 
SET `CompanyId` = NULL, 
    `UserRoleId` = 1, 
    `IsActive` = 1,
    `UpdatedAt` = NOW()
WHERE `Id` = 1;

-- 10. Reiniciar contadores AUTO_INCREMENT en tablas operativas y maestros de tenant
ALTER TABLE `PushSubscriptions` AUTO_INCREMENT = 1;
ALTER TABLE `UserNotificationPreferences` AUTO_INCREMENT = 2;
ALTER TABLE `UserSessions` AUTO_INCREMENT = 1;
ALTER TABLE `BranchOperatingHours` AUTO_INCREMENT = 1;
ALTER TABLE `BranchCommercialAgreements` AUTO_INCREMENT = 1;
ALTER TABLE `BranchPaymentMethods` AUTO_INCREMENT = 1;
ALTER TABLE `UserBranches` AUTO_INCREMENT = 1;
ALTER TABLE `TicketDiscounts` AUTO_INCREMENT = 1;
ALTER TABLE `CommercialAgreements` AUTO_INCREMENT = 1;
ALTER TABLE `Stores` AUTO_INCREMENT = 1;
ALTER TABLE `WorkShifts` AUTO_INCREMENT = 1;
ALTER TABLE `BillingResolutions` AUTO_INCREMENT = 1;
ALTER TABLE `VehicleRates` AUTO_INCREMENT = 1;
ALTER TABLE `ParkingLots` AUTO_INCREMENT = 1;
ALTER TABLE `Branches` AUTO_INCREMENT = 1;
ALTER TABLE `Companies` AUTO_INCREMENT = 1;
ALTER TABLE `Plans` AUTO_INCREMENT = 1;
ALTER TABLE `UserRole` AUTO_INCREMENT = 2;
ALTER TABLE `User` AUTO_INCREMENT = 2;

SET FOREIGN_KEY_CHECKS = 1;

-- ==================================================================================
-- VERIFICACIÓN FINAL DE ESTADO LIMPIO
-- ==================================================================================
SELECT 
    (SELECT COUNT(*) FROM `Companies`) AS TotalEmpresas,
    (SELECT COUNT(*) FROM `Branches`) AS TotalSedes,
    (SELECT COUNT(*) FROM `ParkingTickets`) AS TotalTickets,
    (SELECT COUNT(*) FROM `WorkShifts`) AS TotalTurnos,
    (SELECT COUNT(*) FROM `User` WHERE `Id` = 1 AND `CompanyId` IS NULL) AS SuperAdminActivo,
    (SELECT COUNT(*) FROM `User` WHERE `Id` > 1) AS UsuariosEmpresasRestantes,
    (SELECT COUNT(*) FROM `UserRole` WHERE `Id` = 1) AS RolSuperAdminActivo,
    (SELECT COUNT(*) FROM `Action`) AS TotalAccionesRBAC,
    (SELECT COUNT(*) FROM `Module`) AS TotalModulosRBAC,
    'Limpieza completada exitosamente. La BD está en estado inicial con solo SuperAdministrador.' AS Mensaje;
