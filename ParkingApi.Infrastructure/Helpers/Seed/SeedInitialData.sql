-- ==========================================================
-- ParkFlow Database Master Seed Script for MySQL / MariaDB
-- ==========================================================

USE parkflow_db;

-- 1. Roles
INSERT IGNORE INTO Roles (RoleId, Name, Description, IsActive, CreatedAtUtc)
VALUES 
('11111111-1111-1111-1111-111111111111', 'Administrador', 'Control total de configuración y tarifas', 1, UTC_TIMESTAMP()),
('22222222-2222-2222-2222-222222222222', 'Operador', 'Operación de terminal POS (Ingreso, Cobro y Turno)', 1, UTC_TIMESTAMP());

-- 2. Usuarios Base (Password 'admin123' / 'operador123')
INSERT IGNORE INTO Users (UserId, Username, PasswordHash, FullName, Email, RoleId, IsActive, CreatedAtUtc)
VALUES 
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'admin', '.K5vO0Zl8wPz.8c4rV6M7Qe5D2Iu8YQ9pMhU5Zf4OQfe', 'Administrador Principal', 'admin@parkflow.com', '11111111-1111-1111-1111-111111111111', 1, UTC_TIMESTAMP()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'operador', '.K5vO0Zl8wPz.8c4rV6M7Qe5D2Iu8YQ9pMhU5Zf4OQfe', 'Operador de Turno', 'operador@parkflow.com', '22222222-2222-2222-2222-222222222222', 1, UTC_TIMESTAMP());

-- 3. Tarifas Base por Categoría
INSERT IGNORE INTO VehicleRates (RateId, VehicleType, DisplayName, HourRate, MinuteRate, FullDayRate, GracePeriodMinutes, IconKey, IsActive, CreatedAtUtc)
VALUES 
('33333333-3333-3333-3333-333333333331', 0, 'Automóvil / Sedán', 4000.00, 70.00, 28000.00, 15, 'IconCar', 1, UTC_TIMESTAMP()),
('33333333-3333-3333-3333-333333333332', 1, 'Motocicleta', 2000.00, 35.00, 14000.00, 15, 'IconMotorcycle', 1, UTC_TIMESTAMP()),
('33333333-3333-3333-3333-333333333333', 5, 'Camioneta / SUV', 5000.00, 85.00, 35000.00, 15, 'IconSuv', 1, UTC_TIMESTAMP()),
('33333333-3333-3333-3333-333333333334', 3, 'Furgón / Minibús', 6000.00, 100.00, 42000.00, 15, 'IconVan', 1, UTC_TIMESTAMP()),
('33333333-3333-3333-3333-333333333335', 2, 'Vehículo Pesado / Camión', 10000.00, 170.00, 70000.00, 15, 'IconTruck', 1, UTC_TIMESTAMP());
