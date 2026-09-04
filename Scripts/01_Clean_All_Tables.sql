
use db_acd7d6_parking;

-- ==================================================================================
-- SCRIPT: 01_Clean_All_Tables.sql
-- DESCRIPCIÓN: Limpieza segura y completa de todas las tablas para MySQL / MariaDB.
-- NOTA: No ejecuta DROP DATABASE para preservar la base de datos y usuario del hosting.
-- FECHA ACTUALIZACIÓN: 2026-09-03
-- ==================================================================================

-- 1. Desactivar validación de claves foráneas temporalmente
SET FOREIGN_KEY_CHECKS = 0;

-- 2. Eliminar tablas de relaciones, intermedias y transacciones operativas
DROP TABLE IF EXISTS `BranchCommercialAgreements`;
DROP TABLE IF EXISTS `BranchPaymentMethods`;
DROP TABLE IF EXISTS `UserBranches`;
DROP TABLE IF EXISTS `VehicleIncidentBranches`;
DROP TABLE IF EXISTS `userparkings`;
DROP TABLE IF EXISTS `UserParkings`;
DROP TABLE IF EXISTS `UserParking`;
DROP TABLE IF EXISTS `TicketDiscounts`;
DROP TABLE IF EXISTS `CommercialAgreements`;
DROP TABLE IF EXISTS `Stores`;
DROP TABLE IF EXISTS `ParkingTickets`;
DROP TABLE IF EXISTS `WorkShifts`;
DROP TABLE IF EXISTS `BillingResolutions`;
DROP TABLE IF EXISTS `VehicleIncidents`;
DROP TABLE IF EXISTS `MonthlySubscriptions`;
DROP TABLE IF EXISTS `VehicleRates`;
DROP TABLE IF EXISTS `parkinglots`;
DROP TABLE IF EXISTS `ParkingLots`;
DROP TABLE IF EXISTS `ParkingLot`;

-- 3. Eliminar tablas maestras, seguridad, sesiones, sedes, planes y empresas
DROP TABLE IF EXISTS `UserSessions`;
DROP TABLE IF EXISTS `Branches`;
DROP TABLE IF EXISTS `Companies`;
DROP TABLE IF EXISTS `Company`;
DROP TABLE IF EXISTS `Plans`;
DROP TABLE IF EXISTS `PasswordResetToken`;
DROP TABLE IF EXISTS `Login`;
DROP TABLE IF EXISTS `RoleAction`;
DROP TABLE IF EXISTS `UserRoleModule`;
DROP TABLE IF EXISTS `Action`;
DROP TABLE IF EXISTS `Operation`;
DROP TABLE IF EXISTS `Module`;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS `UserRole`;
DROP TABLE IF EXISTS `IdentificationType`;
DROP TABLE IF EXISTS `PaymentMethod`;

-- 4. Eliminar historial de migraciones de Entity Framework para reseteo completo
DROP TABLE IF EXISTS `__EFMigrationsHistory`;

-- 5. Reactivar validación de claves foráneas
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Todas las tablas han sido eliminadas exitosamente. La base de datos está lista para la migración o creación DDL limpia.' AS Resultado;
