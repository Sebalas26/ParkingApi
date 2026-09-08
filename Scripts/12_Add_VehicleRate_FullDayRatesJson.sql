-- ============================================================================
-- SCRIPT: 12_Add_VehicleRate_FullDayRatesJson.sql
-- DESCRIPCIÓN: Migración defensiva de columna FullDayRatesJson en VehicleRates
--              para soportar tarifas plenas diferenciadas por bloques de días.
-- FECHA: 2026-09-08
-- ============================================================================

SET @dbname = DATABASE();

-- 1. Columna FullDayRatesJson en VehicleRates
SET @tableName = "VehicleRates";
SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'FullDayRatesJson') > 0,
  "SELECT 1",
  "ALTER TABLE `VehicleRates` ADD COLUMN `FullDayRatesJson` LONGTEXT NULL DEFAULT NULL AFTER `FullDayCoverageMinutes`;"
));
PREPARE stmt12a FROM @sqlCmd; EXECUTE stmt12a; DEALLOCATE PREPARE stmt12a;
