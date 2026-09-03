-- =====================================================================================
-- Script: 05_Add_Company_Settings_And_User_Sessions.sql
-- Descripción:
--   1. Agrega campos de parametrización operativa a la tabla Companies.
--   2. Crea la tabla relacional UserSessions para control robusto y escalable de sesiones concurrentes.
--   3. Agrega campo CashRegisterName a la tabla WorkShifts para diferenciar múltiples cajas.
-- =====================================================================================

-- 1. Parametrizaciones Operativas en Companies
ALTER TABLE Companies
    ADD COLUMN AllowMultipleSessions BOOLEAN NOT NULL DEFAULT 0,
    ADD COLUMN MaxActiveSessionsPerUser INT NOT NULL DEFAULT 1,
    ADD COLUMN AllowMultipleOpenShifts BOOLEAN NOT NULL DEFAULT 0,
    ADD COLUMN MaxOpenShiftsPerUser INT NOT NULL DEFAULT 1,
    ADD COLUMN RequireOpenShiftToOperate BOOLEAN NOT NULL DEFAULT 1;

-- 2. Nombre / Identificador de Caja en WorkShifts
ALTER TABLE WorkShifts
    ADD COLUMN CashRegisterName VARCHAR(100) NOT NULL DEFAULT 'Caja Principal';

-- 3. Tabla Relacional UserSessions
CREATE TABLE IF NOT EXISTS UserSessions (
    SessionId CHAR(36) NOT NULL,
    UserId INT NOT NULL,
    Jti VARCHAR(64) NOT NULL,
    DeviceInfo VARCHAR(150) NULL,
    IpAddress VARCHAR(45) NULL,
    CreatedAtUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiresAtUtc DATETIME NOT NULL,
    IsRevoked BOOLEAN NOT NULL DEFAULT 0,
    RevokedAtUtc DATETIME NULL,
    RevokedReason VARCHAR(50) NULL,
    PRIMARY KEY (SessionId),
    INDEX idx_usersessions_user_active (UserId, IsRevoked, ExpiresAtUtc),
    INDEX idx_usersessions_jti (Jti),
    CONSTRAINT fk_usersessions_user FOREIGN KEY (UserId) REFERENCES User (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
