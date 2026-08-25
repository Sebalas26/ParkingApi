-- ==================================================================================
-- SCRIPT: 02_Create_MultiBranch_Tables.sql
-- DESCRIPCIÓN: Script DDL para creación manual de tablas Multi-Sede y actualización de FKs en MySQL.
-- FECHA: 2026-08-25
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- 1. Tabla Branches (Sedes / Parqueaderos)
CREATE TABLE IF NOT EXISTS `Branches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Code` VARCHAR(20) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Address` VARCHAR(200) NOT NULL,
    `Phone` VARCHAR(30) NULL,
    `City` VARCHAR(50) NULL,
    `TotalCapacity` INT NOT NULL DEFAULT 100,
    `Notes` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Branches_Code` (`Code`),
    CONSTRAINT `FK_Branches_User_ResponsibleUserId` FOREIGN KEY (`ResponsibleUserId`) REFERENCES `User` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Tabla UserBranches (Relación N:N Usuarios - Sedes)
CREATE TABLE IF NOT EXISTS `UserBranches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `BranchId` INT NOT NULL,
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_UserBranches_User_Branch` (`UserId`, `BranchId`),
    CONSTRAINT `FK_UserBranches_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserBranches_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Tabla BranchPaymentMethods (Parametrización de Medios de Pago por Sede)
CREATE TABLE IF NOT EXISTS `BranchPaymentMethods` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `PaymentMethodId` INT NOT NULL,
    `RequiresCashTender` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchPaymentMethods_Branch_PaymentMethod` (`BranchId`, `PaymentMethodId`),
    CONSTRAINT `FK_BranchPaymentMethods_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_BranchPaymentMethods_PaymentMethod_PaymentMethodId` FOREIGN KEY (`PaymentMethodId`) REFERENCES `PaymentMethod` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Asegurar columnas BranchId en tablas de negocio si ya existen
SET @dbname = DATABASE();

-- VehicleRates.BranchId
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbname AND table_name = 'VehicleRates');
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @dbname AND table_name = 'VehicleRates' AND column_name = 'BranchId');
SET @stmt = IF(@table_exists > 0 AND @col_exists = 0, 'ALTER TABLE `VehicleRates` ADD COLUMN `BranchId` INT NULL, ADD INDEX `IX_VehicleRates_BranchId` (`BranchId`), ADD CONSTRAINT `FK_VehicleRates_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT;', 'SELECT 1');
PREPARE st FROM @stmt; EXECUTE st; DEALLOCATE PREPARE st;

-- Stores.BranchId
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbname AND table_name = 'Stores');
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @dbname AND table_name = 'Stores' AND column_name = 'BranchId');
SET @stmt = IF(@table_exists > 0 AND @col_exists = 0, 'ALTER TABLE `Stores` ADD COLUMN `BranchId` INT NULL, ADD INDEX `IX_Stores_BranchId` (`BranchId`), ADD CONSTRAINT `FK_Stores_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT;', 'SELECT 1');
PREPARE st FROM @stmt; EXECUTE st; DEALLOCATE PREPARE st;

-- ParkingTickets.BranchId
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbname AND table_name = 'ParkingTickets');
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @dbname AND table_name = 'ParkingTickets' AND column_name = 'BranchId');
SET @stmt = IF(@table_exists > 0 AND @col_exists = 0, 'ALTER TABLE `ParkingTickets` ADD COLUMN `BranchId` INT NULL, ADD INDEX `IX_ParkingTickets_BranchId` (`BranchId`), ADD CONSTRAINT `FK_ParkingTickets_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT;', 'SELECT 1');
PREPARE st FROM @stmt; EXECUTE st; DEALLOCATE PREPARE st;

-- WorkShifts.BranchId
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbname AND table_name = 'WorkShifts');
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @dbname AND table_name = 'WorkShifts' AND column_name = 'BranchId');
SET @stmt = IF(@table_exists > 0 AND @col_exists = 0, 'ALTER TABLE `WorkShifts` ADD COLUMN `BranchId` INT NULL, ADD INDEX `IX_WorkShifts_BranchId` (`BranchId`), ADD CONSTRAINT `FK_WorkShifts_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT;', 'SELECT 1');
PREPARE st FROM @stmt; EXECUTE st; DEALLOCATE PREPARE st;

-- MonthlySubscriptions.BranchId
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbname AND table_name = 'MonthlySubscriptions');
SET @col_exists = (SELECT COUNT(*) FROM information_schema.columns WHERE table_schema = @dbname AND table_name = 'MonthlySubscriptions' AND column_name = 'BranchId');
SET @stmt = IF(@table_exists > 0 AND @col_exists = 0, 'ALTER TABLE `MonthlySubscriptions` ADD COLUMN `BranchId` INT NULL, ADD INDEX `IX_MonthlySubscriptions_BranchId` (`BranchId`), ADD CONSTRAINT `FK_MonthlySubscriptions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT;', 'SELECT 1');
PREPARE st FROM @stmt; EXECUTE st; DEALLOCATE PREPARE st;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'Tablas Multi-Sede y claves foráneas creadas / verificadas exitosamente.' AS Resultado;
