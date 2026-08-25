-- ==================================================================================
-- SCRIPT: 01_Clean_All_Tables.sql
-- DESCRIPCIÓN: Limpieza segura de todas las tablas existentes para el servidor MySQL.
-- NOTA: No ejecuta DROP DATABASE para preservar la base de datos y usuario del hosting.
-- FECHA: 2026-08-25
-- ==================================================================================

-- 1. Desactivar validación de claves foráneas temporalmente
SET FOREIGN_KEY_CHECKS = 0;

-- 2. Eliminar tablas de relaciones y transacciones operativas
DROP TABLE IF EXISTS `BranchPaymentMethods`;
DROP TABLE IF EXISTS `UserBranches`;
DROP TABLE IF EXISTS `TicketDiscounts`;
DROP TABLE IF EXISTS `CommercialAgreements`;
DROP TABLE IF EXISTS `Stores`;
DROP TABLE IF EXISTS `ParkingTickets`;
DROP TABLE IF EXISTS `WorkShifts`;
DROP TABLE IF EXISTS `MonthlySubscriptions`;
DROP TABLE IF EXISTS `VehicleRates`;

-- 3. Eliminar tablas maestras, seguridad y sedes
DROP TABLE IF EXISTS `Branches`;
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

SELECT 'Todas las tablas han sido eliminadas exitosamente. La base de datos está lista para la migración o creación DDL.' AS Resultado;
