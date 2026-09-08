-- ============================================================================
-- SCRIPT: 11_Add_FullDayRules_And_Coverage.sql
-- DESCRIPCIÓN: Migración defensiva de columnas FullDayRulesJson en Branches
--              y FullDayCoverageMinutes en VehicleRates.
-- FECHA: 2026-09-07
-- ============================================================================

SET @dbname = DATABASE();

-- 1. Columna FullDayRulesJson en Branches
SET @tableName = "Branches";
SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayRulesJson') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `FullDayRulesJson` LONGTEXT NULL AFTER `FullDayEndTime`;"
));
PREPARE stmt11a FROM @sqlCmd; EXECUTE stmt11a; DEALLOCATE PREPARE stmt11a;

-- 2. Columna FullDayCoverageMinutes en VehicleRates
SET @tableName = "VehicleRates";
SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayCoverageMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `FullDayCoverageMinutes` INT NULL DEFAULT NULL AFTER `FullDayThresholdMinutes`;"
));
PREPARE stmt11b FROM @sqlCmd; EXECUTE stmt11b; DEALLOCATE PREPARE stmt11b;
