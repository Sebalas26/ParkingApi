use db_acd7d6_parking;

-- ==================================================================================
-- SCRIPT: 09_Add_Hours_Rates_Agreements_And_Push.sql
-- DESCRIPCIÓN: Migración segura y DDL para Horarios con Gabela, Tarifas por Día Semanal,
--             Tarifa Plena Avanzada, Tiquete Perdido, Convenios por Tiempo y WebPush VAPID.
-- FECHA: 2026-09-06
-- ==================================================================================

SET @dbname = DATABASE();

-- 1. Columnas en Companies para Notificaciones Push
SET @tableName = "Companies";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'HasPushNotificationsEnabled') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `HasPushNotificationsEnabled` BOOLEAN NOT NULL DEFAULT 0;"
));
PREPARE stmt1 FROM @sqlCmd; EXECUTE stmt1; DEALLOCATE PREPARE stmt1;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowedPushTypesJson') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `AllowedPushTypesJson` LONGTEXT NULL;"
));
PREPARE stmt2 FROM @sqlCmd; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

-- 2. Columnas en Branches para Tiquete Perdido, Plena y Nocturna
SET @tableName = "Branches";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'LostTicketFee') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `LostTicketFee` DECIMAL(18,2) NOT NULL DEFAULT 0.00;"
));
PREPARE stmt3 FROM @sqlCmd; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayThresholdMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `FullDayThresholdMinutes` INT NULL DEFAULT 180;"
));
PREPARE stmt4 FROM @sqlCmd; EXECUTE stmt4; DEALLOCATE PREPARE stmt4;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayApplicableDays') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `FullDayApplicableDays` VARCHAR(50) NULL DEFAULT '1,2,3,4,5,6,0';"
));
PREPARE stmt5 FROM @sqlCmd; EXECUTE stmt5; DEALLOCATE PREPARE stmt5;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayStartTime') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `FullDayStartTime` TIME NULL;"
));
PREPARE stmt6 FROM @sqlCmd; EXECUTE stmt6; DEALLOCATE PREPARE stmt6;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayEndTime') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `FullDayEndTime` TIME NULL;"
));
PREPARE stmt7 FROM @sqlCmd; EXECUTE stmt7; DEALLOCATE PREPARE stmt7;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightStartTime') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `NightStartTime` TIME NULL DEFAULT '18:00:00';"
));
PREPARE stmt8 FROM @sqlCmd; EXECUTE stmt8; DEALLOCATE PREPARE stmt8;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightEndTime') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `NightEndTime` TIME NULL DEFAULT '06:00:00';"
));
PREPARE stmt9 FROM @sqlCmd; EXECUTE stmt9; DEALLOCATE PREPARE stmt9;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightStayMinMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `NightStayMinMinutes` INT NULL DEFAULT 240;"
));
PREPARE stmt10 FROM @sqlCmd; EXECUTE stmt10; DEALLOCATE PREPARE stmt10;

-- 3. Columna DayOfWeek en VehicleRates
SET @tableName = "VehicleRates";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'DayOfWeek') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `DayOfWeek` INT NULL;"
));
PREPARE stmt11 FROM @sqlCmd; EXECUTE stmt11; DEALLOCATE PREPARE stmt11;

-- 4. Columnas en CommercialAgreements (Multi-Tenant y Modalidad Tiempo)
SET @tableName = "CommercialAgreements";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'CompanyId') > 0,
  "SELECT 1",
  "ALTER TABLE `CommercialAgreements` ADD COLUMN `CompanyId` INT NULL AFTER `AgreementId`;"
));
PREPARE stmt12 FROM @sqlCmd; EXECUTE stmt12; DEALLOCATE PREPARE stmt12;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'DiscountType') > 0,
  "SELECT 1",
  "ALTER TABLE `CommercialAgreements` ADD COLUMN `DiscountType` INT NOT NULL DEFAULT 0 AFTER `MinPurchaseAmount`;"
));
PREPARE stmt13 FROM @sqlCmd; EXECUTE stmt13; DEALLOCATE PREPARE stmt13;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FreeMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `CommercialAgreements` ADD COLUMN `FreeMinutes` INT NULL AFTER `DiscountFixedAmount`;"
));
PREPARE stmt14 FROM @sqlCmd; EXECUTE stmt14; DEALLOCATE PREPARE stmt14;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FreeHours') > 0,
  "SELECT 1",
  "ALTER TABLE `CommercialAgreements` ADD COLUMN `FreeHours` INT NULL AFTER `FreeMinutes`;"
));
PREPARE stmt15 FROM @sqlCmd; EXECUTE stmt15; DEALLOCATE PREPARE stmt15;

-- 5. Columnas en ParkingTickets para Tiquete Perdido
SET @tableName = "ParkingTickets";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'IsLostTicket') > 0,
  "SELECT 1",
  "ALTER TABLE `ParkingTickets` ADD COLUMN `IsLostTicket` BOOLEAN NOT NULL DEFAULT 0;"
));
PREPARE stmt16 FROM @sqlCmd; EXECUTE stmt16; DEALLOCATE PREPARE stmt16;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'LostTicketFee') > 0,
  "SELECT 1",
  "ALTER TABLE `ParkingTickets` ADD COLUMN `LostTicketFee` DECIMAL(18,2) NOT NULL DEFAULT 0.00;"
));
PREPARE stmt17 FROM @sqlCmd; EXECUTE stmt17; DEALLOCATE PREPARE stmt17;

-- 6. Crear Tabla de Horarios de Sede con Gabela
CREATE TABLE IF NOT EXISTS `BranchOperatingHours` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `DayOfWeek` INT NOT NULL,
    `IsOpen` BOOLEAN NOT NULL DEFAULT 1,
    `OpeningTime` TIME NOT NULL DEFAULT '08:00:00',
    `ClosingTime` TIME NOT NULL DEFAULT '22:00:00',
    `BufferMinutesBefore` INT NOT NULL DEFAULT 30,
    `BufferMinutesAfter` INT NOT NULL DEFAULT 30,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchOperatingHours_Branch_Day` (`BranchId`, `DayOfWeek`),
    KEY `IX_BranchOperatingHours_BranchId` (`BranchId`),
    CONSTRAINT `FK_BranchOperatingHours_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 7. Crear Tabla de Dispositivos Suscritos a WebPush VAPID
CREATE TABLE IF NOT EXISTS `PushSubscriptions` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `CompanyId` INT NOT NULL,
    `BranchId` INT NULL,
    `Endpoint` TEXT NOT NULL,
    `P256dh` VARCHAR(500) NOT NULL,
    `Auth` VARCHAR(500) NOT NULL,
    `DeviceName` VARCHAR(200) NULL,
    `UserAgent` VARCHAR(500) NULL,
    `IsActive` BOOLEAN NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `LastSentAtUtc` DATETIME(6) NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_PushSubscriptions_User_Branch` (`UserId`, `BranchId`),
    KEY `IX_PushSubscriptions_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_PushSubscriptions_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_PushSubscriptions_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_PushSubscriptions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Crear Tabla de Preferencias de Notificaciones por Usuario
CREATE TABLE IF NOT EXISTS `UserNotificationPreferences` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `NotifyAppUpdates` BOOLEAN NOT NULL DEFAULT 1,
    `NotifyShiftOpen` BOOLEAN NOT NULL DEFAULT 0,
    `NotifyShiftClose` BOOLEAN NOT NULL DEFAULT 1,
    `NotifyCashDiscrepancy` BOOLEAN NOT NULL DEFAULT 1,
    `NotifyVehicleIncidents` BOOLEAN NOT NULL DEFAULT 1,
    `NotifyOverdueVehicles` BOOLEAN NOT NULL DEFAULT 0,
    `NotifyCancelledTickets` BOOLEAN NOT NULL DEFAULT 1,
    `UpdatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_UserNotificationPreferences_UserId` (`UserId`),
    CONSTRAINT `FK_UserNotificationPreferences_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

SELECT 'Migración 09 aplicada exitosamente: Horarios, Tarifas Semanales, Plena, Tiquete Perdido, Convenios Tiempo y Push Subscriptions.' AS Resultado;
