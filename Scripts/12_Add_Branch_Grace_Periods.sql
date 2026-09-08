-- Script 12: Agregar Tiempos de Gracia de Entrada y Salida en Branches
-- Fecha: 2026-09-08
-- Autor: ParkingFlow Team

SET @dbname = DATABASE();
SET @tableName = "Branches";

-- 1. Columna EntryGracePeriodMinutes (Tolerancia de desistimiento para cobro $0 al ingresar)
SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'EntryGracePeriodMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `EntryGracePeriodMinutes` INT NOT NULL DEFAULT 0;"
));
PREPARE stmt1 FROM @sqlCmd; EXECUTE stmt1; DEALLOCATE PREPARE stmt1;

-- 2. Columna ExitGracePeriodMinutes (Tolerancia de salida tras pago para no generar excedente)
SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'ExitGracePeriodMinutes') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `ExitGracePeriodMinutes` INT NOT NULL DEFAULT 0;"
));
PREPARE stmt2 FROM @sqlCmd; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

SELECT 'Columnas EntryGracePeriodMinutes y ExitGracePeriodMinutes verificadas exitosamente en Branches' AS Resultado;
