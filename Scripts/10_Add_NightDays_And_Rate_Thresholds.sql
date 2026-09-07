use db_acd7d6_parking;

-- ==================================================================================
-- SCRIPT: 10_Add_NightDays_And_Rate_Thresholds.sql
-- DESCRIPCIÓN: Migración segura y DDL para soporte de días aplicables en tarifa nocturna,
--             horarios de ventana y umbrales de activación fija en tarifas vehiculares (VehicleRates).
-- FECHA: 2026-09-07
-- ==================================================================================

SET @dbname = DATABASE();

-- 1. Columna NightApplicableDays en Branches
SET @tableName = "Branches";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightApplicableDays') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `NightApplicableDays` VARCHAR(50) NULL DEFAULT '1,2,3,4,5,6,0' AFTER `FullDayApplicableDays`;"
));
PREPARE stmt1 FROM @sqlCmd; EXECUTE stmt1; DEALLOCATE PREPARE stmt1;

-- 2. Columnas en VehicleRates para Horario de Plena, Umbral de Activación y Pernocta por Vehículo
SET @tableName = "VehicleRates";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayStartTime') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `FullDayStartTime` TIME NULL AFTER `FullDayRate`;"
));
PREPARE stmt2 FROM @sqlCmd; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayEndTime') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `FullDayEndTime` TIME NULL AFTER `FullDayStartTime`;"
));
PREPARE stmt3 FROM @sqlCmd; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayThresholdMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `FullDayThresholdMinutes` INT NULL AFTER `FullDayEndTime`;"
));
PREPARE stmt4 FROM @sqlCmd; EXECUTE stmt4; DEALLOCATE PREPARE stmt4;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightStartTime') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `NightStartTime` TIME NULL AFTER `NightRate`;"
));
PREPARE stmt5 FROM @sqlCmd; EXECUTE stmt5; DEALLOCATE PREPARE stmt5;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightEndTime') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `NightEndTime` TIME NULL AFTER `NightStartTime`;"
));
PREPARE stmt6 FROM @sqlCmd; EXECUTE stmt6; DEALLOCATE PREPARE stmt6;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'NightStayMinMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `NightStayMinMinutes` INT NULL AFTER `NightEndTime`;"
));
PREPARE stmt7 FROM @sqlCmd; EXECUTE stmt7; DEALLOCATE PREPARE stmt7;
