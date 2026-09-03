-- ==============================================================================
-- MIGRACIÓN: Parámetros Operativos de Empresa, Tipos de Cobro en Sedes y Tarifa Nocturna
-- Base de Datos: MySQL / MariaDB (ParkingApi)
-- ==============================================================================

-- 1. Agregar parámetro RequireInitialCashAmount a la tabla Companies
ALTER TABLE Companies
    ADD COLUMN RequireInitialCashAmount BOOLEAN NOT NULL DEFAULT 1 COMMENT 'Indica si es obligatorio ingresar un monto base inicial al abrir caja';

-- 2. Agregar columnas de Esquemas de Cobro Permitidos a la tabla Branches
ALTER TABLE Branches
    ADD COLUMN AllowChargeByMinute BOOLEAN NOT NULL DEFAULT 1 COMMENT 'Permite liquidación fraccional por minuto',
    ADD COLUMN AllowChargeByHour BOOLEAN NOT NULL DEFAULT 1 COMMENT 'Permite liquidación por hora',
    ADD COLUMN AllowChargeByDay BOOLEAN NOT NULL DEFAULT 1 COMMENT 'Permite cobro de tarifa plena o día',
    ADD COLUMN AllowChargeByNight BOOLEAN NOT NULL DEFAULT 0 COMMENT 'Permite cobro de tarifa nocturna';

-- 3. Agregar columna NightRate a la tabla VehicleRates
ALTER TABLE VehicleRates
    ADD COLUMN NightRate DECIMAL(18, 2) NOT NULL DEFAULT 0.00 COMMENT 'Valor de la tarifa nocturna para este tipo de vehículo';
