-- =========================================================================
-- Script: 04_Add_Branch_LogoBase64.sql
-- Propósito: Agregar la columna LogoBase64 a la tabla `branches` para
--            almacenar el logo corporativo de cada parqueadero en Base64.
-- =========================================================================

-- Verificar si la columna ya existe antes de agregarla
SET @dbname = DATABASE();
SET @tablename = "branches";
SET @columnname = "LogoBase64";
SET @preparedStatement = (SELECT IF(
  (
    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
    WHERE
      (TABLE_SCHEMA = @dbname)
      AND (TABLE_NAME = @tablename)
      AND (COLUMN_NAME = @columnname)
  ) > 0,
  "SELECT 1",
  "ALTER TABLE `branches` ADD COLUMN `LogoBase64` LONGTEXT NULL AFTER `Notes`;"
));
PREPARE alterIfNotExists FROM @preparedStatement;
EXECUTE alterIfNotExists;
DEALLOCATE PREPARE alterIfNotExists;
