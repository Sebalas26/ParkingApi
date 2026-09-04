
use db_acd7d6_parking;

-- ==================================================================================
-- SCRIPT: 02_Init_RBAC_Seed.sql
-- DESCRIPCIÓN: Script Oficial y Completo de Inicialización de Esquema Multi-Tenant SaaS y RBAC Seed.
-- MOTOR: MySQL 8.x / MariaDB
-- REGLAS DE NEGOCIO SAAS MULTI-TENANT (ARRANQUE LIMPIO):
--   1. Aprovisionamiento seguro DDL (CREATE TABLE IF NOT EXISTS) 100% alineado con Entity Framework Core.
--   2. Soporte completo Multi-Tenant con discriminador CompanyId y aislamiento relacional por Sede (BranchId).
--   3. Inicializa Tipos de Identificación estándar (CC, CE, NIT, PAS, PEP).
--   4. Multi-Tenant Limpio desde CERO: Cero (0) empresas, cero (0) sedes, cero (0) planes predefinidos.
--      El Super Administrador ('admin') crea empresas y planes dinámicamente desde el panel PWA.
--   5. Inicializa ÚNICAMENTE el Rol 'Super Administrador' (Id 1) con acceso al 100% de los módulos y acciones.
--   6. Inicializa ÚNICAMENTE el Usuario 'admin' (SuperAdmin de la Plataforma SaaS con CompanyId NULL).
--   7. Catálogo de los 17 Módulos del sistema (Terminal WPF, Administración PWA y Gestión SaaS/Planes).
--   8. Catálogo de 7 Operaciones estándar del sistema.
--   9. Catálogo completo de 82 Acciones y Slugs canónicos del sistema (incluye companies.*, plans.*, metrics, etc.).
--  10. Asigna el 100% de los 17 Módulos y el 100% de las 82 Acciones exclusivamente al Rol Super Administrador.
--  11. Registro en __EFMigrationsHistory para compatibilidad total con EF Core.
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ==================================================================================
-- FASE 1: DDL SEGURO - CREACIÓN DE TABLAS (SI NO EXISTEN)
-- ==================================================================================

-- 0. Historial de Migraciones EF Core
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) NOT NULL,
    `ProductVersion` VARCHAR(32) NOT NULL,
    PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.0 Planes de Suscripción SaaS (Catálogo Base)
CREATE TABLE IF NOT EXISTS `Plans` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `Description` VARCHAR(500) NULL,
    `PriceCop` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AnnualPriceCop` DECIMAL(18,2) NULL,
    `MaxBranches` INT NOT NULL DEFAULT 1,
    `MaxUsers` INT NOT NULL DEFAULT 5,
    `HasDesktopAccess` BOOLEAN NOT NULL DEFAULT 1,
    `HasWebAccess` BOOLEAN NOT NULL DEFAULT 1,
    `AllowMultipleSessions` BOOLEAN NOT NULL DEFAULT 0,
    `MaxActiveSessionsPerUser` INT NOT NULL DEFAULT 1,
    `IncludedModulesWebJson` LONGTEXT NULL,
    `IncludedModulesDesktopJson` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.1 Empresas / Tenants (Multi-Tenant SaaS)
CREATE TABLE IF NOT EXISTS `Companies` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(150) NOT NULL,
    `LegalName` VARCHAR(150) NULL,
    `Nit` VARCHAR(50) NOT NULL,
    `Email` VARCHAR(100) NOT NULL,
    `Phone` VARCHAR(30) NULL,
    `Address` VARCHAR(200) NULL,
    `City` VARCHAR(50) NULL,
    `PlanType` VARCHAR(50) NOT NULL DEFAULT 'Basic',
    `PlanId` INT NULL,
    `IsCustomPlan` BOOLEAN NOT NULL DEFAULT 0,
    `MaxBranches` INT NOT NULL DEFAULT 1,
    `MaxUsers` INT NOT NULL DEFAULT 5,
    `HasDesktopAccess` BOOLEAN NOT NULL DEFAULT 1,
    `HasWebAccess` BOOLEAN NOT NULL DEFAULT 1,
    `CustomModulesWebJson` LONGTEXT NULL,
    `CustomModulesDesktopJson` LONGTEXT NULL,
    `Logo` LONGTEXT NULL,
    `AllowMultipleSessions` BOOLEAN NOT NULL DEFAULT 0,
    `MaxActiveSessionsPerUser` INT NOT NULL DEFAULT 1,
    `AllowMultipleOpenShifts` BOOLEAN NOT NULL DEFAULT 0,
    `MaxOpenShiftsPerUser` INT NOT NULL DEFAULT 1,
    `RequireOpenShiftToOperate` BOOLEAN NOT NULL DEFAULT 1,
    `RequireInitialCashAmount` BOOLEAN NOT NULL DEFAULT 1,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `SubscriptionExpiresAt` DATETIME NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Companies_Nit` (`Nit`),
    KEY `IX_Companies_PlanId` (`PlanId`),
    CONSTRAINT `FK_Companies_Plans_PlanId` FOREIGN KEY (`PlanId`) REFERENCES `Plans` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.2 Sedes / Parqueaderos (Pertenecen obligatoriamente a una Empresa)
CREATE TABLE IF NOT EXISTS `Branches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NOT NULL,
    `Code` VARCHAR(20) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Address` VARCHAR(200) NOT NULL,
    `Phone` VARCHAR(30) NULL,
    `City` VARCHAR(50) NULL,
    `TotalCapacity` INT NOT NULL DEFAULT 100,
    `PaperWidth` INT NOT NULL DEFAULT 80,
    `DefaultInitialCash` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AllowChargeByMinute` BOOLEAN NOT NULL DEFAULT 1,
    `AllowChargeByHour` BOOLEAN NOT NULL DEFAULT 1,
    `AllowChargeByDay` BOOLEAN NOT NULL DEFAULT 1,
    `AllowChargeByNight` BOOLEAN NOT NULL DEFAULT 0,
    `Notes` VARCHAR(500) NULL,
    `LogoBase64` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Branches_Company_Code` (`CompanyId`, `Code`),
    KEY `IX_Branches_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_Branches_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Migración Defensiva de Columnas para Bases de Datos Existentes
SET @dbname = DATABASE();

-- 1.2a Columnas de Planes y Cuotas en Companies
SET @tableName = "Companies";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'PlanId') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `PlanId` INT NULL AFTER `PlanType`;"
));
PREPARE stmt1a FROM @sqlCmd; EXECUTE stmt1a; DEALLOCATE PREPARE stmt1a;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'IsCustomPlan') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `IsCustomPlan` BOOLEAN NOT NULL DEFAULT 0 AFTER `PlanId`;"
));
PREPARE stmt1b FROM @sqlCmd; EXECUTE stmt1b; DEALLOCATE PREPARE stmt1b;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'MaxUsers') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `MaxUsers` INT NOT NULL DEFAULT 5 AFTER `MaxBranches`;"
));
PREPARE stmt1c FROM @sqlCmd; EXECUTE stmt1c; DEALLOCATE PREPARE stmt1c;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'HasDesktopAccess') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `HasDesktopAccess` BOOLEAN NOT NULL DEFAULT 1 AFTER `MaxUsers`;"
));
PREPARE stmt1d FROM @sqlCmd; EXECUTE stmt1d; DEALLOCATE PREPARE stmt1d;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'HasWebAccess') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `HasWebAccess` BOOLEAN NOT NULL DEFAULT 1 AFTER `HasDesktopAccess`;"
));
PREPARE stmt1e FROM @sqlCmd; EXECUTE stmt1e; DEALLOCATE PREPARE stmt1e;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'CustomModulesWebJson') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `CustomModulesWebJson` LONGTEXT NULL AFTER `HasWebAccess`;"
));
PREPARE stmt1f FROM @sqlCmd; EXECUTE stmt1f; DEALLOCATE PREPARE stmt1f;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'CustomModulesDesktopJson') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `CustomModulesDesktopJson` LONGTEXT NULL AFTER `CustomModulesWebJson`;"
));
PREPARE stmt1g FROM @sqlCmd; EXECUTE stmt1g; DEALLOCATE PREPARE stmt1g;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'Logo') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `Logo` LONGTEXT NULL AFTER `CustomModulesDesktopJson`;"
));
PREPARE stmt1 FROM @sqlCmd; EXECUTE stmt1; DEALLOCATE PREPARE stmt1;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowMultipleSessions') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `AllowMultipleSessions` BOOLEAN NOT NULL DEFAULT 0;"
));
PREPARE stmt2 FROM @sqlCmd; EXECUTE stmt2; DEALLOCATE PREPARE stmt2;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'MaxActiveSessionsPerUser') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `MaxActiveSessionsPerUser` INT NOT NULL DEFAULT 1;"
));
PREPARE stmt3 FROM @sqlCmd; EXECUTE stmt3; DEALLOCATE PREPARE stmt3;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowMultipleOpenShifts') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `AllowMultipleOpenShifts` BOOLEAN NOT NULL DEFAULT 0;"
));
PREPARE stmt4 FROM @sqlCmd; EXECUTE stmt4; DEALLOCATE PREPARE stmt4;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'MaxOpenShiftsPerUser') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `MaxOpenShiftsPerUser` INT NOT NULL DEFAULT 1;"
));
PREPARE stmt5 FROM @sqlCmd; EXECUTE stmt5; DEALLOCATE PREPARE stmt5;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'RequireOpenShiftToOperate') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `RequireOpenShiftToOperate` BOOLEAN NOT NULL DEFAULT 1;"
));
PREPARE stmt6 FROM @sqlCmd; EXECUTE stmt6; DEALLOCATE PREPARE stmt6;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'RequireInitialCashAmount') > 0,
  "SELECT 1",
  "ALTER TABLE `Companies` ADD COLUMN `RequireInitialCashAmount` BOOLEAN NOT NULL DEFAULT 1;"
));
PREPARE stmt7 FROM @sqlCmd; EXECUTE stmt7; DEALLOCATE PREPARE stmt7;

-- 1.2b Columnas Operativas y Esquemas de Cobro en Branches
SET @tableName = "Branches";

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'PaperWidth') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `PaperWidth` INT NOT NULL DEFAULT 80 AFTER `TotalCapacity`;"
));
PREPARE stmt8 FROM @sqlCmd; EXECUTE stmt8; DEALLOCATE PREPARE stmt8;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'DefaultInitialCash') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `DefaultInitialCash` DECIMAL(18,2) NOT NULL DEFAULT 0.00 AFTER `PaperWidth`;"
));
PREPARE stmt9 FROM @sqlCmd; EXECUTE stmt9; DEALLOCATE PREPARE stmt9;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowChargeByMinute') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `AllowChargeByMinute` BOOLEAN NOT NULL DEFAULT 1;"
));
PREPARE stmt10 FROM @sqlCmd; EXECUTE stmt10; DEALLOCATE PREPARE stmt10;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowChargeByHour') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `AllowChargeByHour` BOOLEAN NOT NULL DEFAULT 1;"
));
PREPARE stmt11 FROM @sqlCmd; EXECUTE stmt11; DEALLOCATE PREPARE stmt11;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowChargeByDay') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `AllowChargeByDay` BOOLEAN NOT NULL DEFAULT 1;"
));
PREPARE stmt12 FROM @sqlCmd; EXECUTE stmt12; DEALLOCATE PREPARE stmt12;

SET @sqlCmd = (SELECT IF(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = @tableName AND COLUMN_NAME = 'AllowChargeByNight') > 0,
  "SELECT 1",
  "ALTER TABLE `Branches` ADD COLUMN `AllowChargeByNight` BOOLEAN NOT NULL DEFAULT 0;"
));
PREPARE stmt13 FROM @sqlCmd; EXECUTE stmt13; DEALLOCATE PREPARE stmt13;

-- 1.3 Tipos de Identificación
CREATE TABLE IF NOT EXISTS `IdentificationType` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Identification` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.4 Roles de Usuario (SaaS: CompanyId NULL para roles del sistema / Globales; BranchId para roles de sede)
CREATE TABLE IF NOT EXISTS `UserRole` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_UserRole_CompanyId` (`CompanyId`),
    KEY `IX_UserRole_BranchId` (`BranchId`),
    CONSTRAINT `FK_UserRole_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_UserRole_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.5 Usuarios (SaaS: CompanyId NULL para SuperAdmin de Plataforma)
CREATE TABLE IF NOT EXISTS `User` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NULL,
    `UserRoleId` INT NOT NULL,
    `IdentificationTypeId` INT NOT NULL,
    `IdentificationNumber` VARCHAR(50) NOT NULL,
    `FirstName` VARCHAR(50) NOT NULL DEFAULT '',
    `MiddleName` VARCHAR(50) NULL DEFAULT '',
    `FirstSurname` VARCHAR(50) NOT NULL DEFAULT '',
    `SecondLastName` VARCHAR(50) NULL DEFAULT '',
    `FullName` VARCHAR(150) NOT NULL,
    `Username` VARCHAR(50) NOT NULL,
    `Password` VARCHAR(255) NOT NULL,
    `Email` VARCHAR(100) NOT NULL,
    `Token` LONGTEXT NULL,
    `AssignmentDate` DATETIME(6) NULL,
    `ExpirationDate` DATETIME(6) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `MustChangePassword` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_User_Username` (`Username`),
    KEY `IX_User_CompanyId` (`CompanyId`),
    KEY `IX_User_UserRoleId` (`UserRoleId`),
    KEY `IX_User_IdentificationTypeId` (`IdentificationTypeId`),
    CONSTRAINT `FK_User_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_User_UserRole_UserRoleId` FOREIGN KEY (`UserRoleId`) REFERENCES `UserRole` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_User_IdentificationType_IdentificationTypeId` FOREIGN KEY (`IdentificationTypeId`) REFERENCES `IdentificationType` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.6 Módulos
CREATE TABLE IF NOT EXISTS `Module` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.7 Operaciones
CREATE TABLE IF NOT EXISTS `Operation` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.8 Acciones
CREATE TABLE IF NOT EXISTS `Action` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `ModuleId` INT NOT NULL,
    `OperationId` INT NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Slug` VARCHAR(100) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_Action_ModuleId` (`ModuleId`),
    KEY `IX_Action_OperationId` (`OperationId`),
    CONSTRAINT `FK_Action_Module_ModuleId` FOREIGN KEY (`ModuleId`) REFERENCES `Module` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Action_Operation_OperationId` FOREIGN KEY (`OperationId`) REFERENCES `Operation` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.9 Permisos por Rol (RoleAction)
CREATE TABLE IF NOT EXISTS `RoleAction` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `RoleId` INT NOT NULL,
    `ActionId` INT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_RoleAction_RoleId` (`RoleId`),
    KEY `IX_RoleAction_ActionId` (`ActionId`),
    CONSTRAINT `FK_RoleAction_UserRole_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `UserRole` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_RoleAction_Action_ActionId` FOREIGN KEY (`ActionId`) REFERENCES `Action` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.10 Módulos por Rol (UserRoleModule)
CREATE TABLE IF NOT EXISTS `UserRoleModule` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserRoleId` INT NOT NULL,
    `ModulesRoleId` INT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_UserRoleModule_UserRoleId` (`UserRoleId`),
    KEY `IX_UserRoleModule_ModulesRoleId` (`ModulesRoleId`),
    CONSTRAINT `FK_UserRoleModule_UserRole_UserRoleId` FOREIGN KEY (`UserRoleId`) REFERENCES `UserRole` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserRoleModule_Module_ModulesRoleId` FOREIGN KEY (`ModulesRoleId`) REFERENCES `Module` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.11 Medios de Pago Maestros
CREATE TABLE IF NOT EXISTS `PaymentMethod` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NULL,
    `Name` VARCHAR(50) NOT NULL,
    `Icon` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_PaymentMethod_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_PaymentMethod_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.12 Medios de Pago por Sede
CREATE TABLE IF NOT EXISTS `BranchPaymentMethods` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `PaymentMethodId` INT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchPaymentMethods_Branch_Method` (`BranchId`, `PaymentMethodId`),
    KEY `IX_BranchPaymentMethods_BranchId` (`BranchId`),
    KEY `IX_BranchPaymentMethods_PaymentMethodId` (`PaymentMethodId`),
    CONSTRAINT `FK_BranchPaymentMethods_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_BranchPaymentMethods_PaymentMethod_PaymentMethodId` FOREIGN KEY (`PaymentMethodId`) REFERENCES `PaymentMethod` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.13 Asignación de Usuarios a Sedes
CREATE TABLE IF NOT EXISTS `UserBranches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `BranchId` INT NOT NULL,
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    `ResponsibleUserIdNavigationId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_UserBranches_User_Branch` (`UserId`, `BranchId`),
    KEY `IX_UserBranches_UserId` (`UserId`),
    KEY `IX_UserBranches_BranchId` (`BranchId`),
    CONSTRAINT `FK_UserBranches_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserBranches_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.14 Comercios Aliados
CREATE TABLE IF NOT EXISTS `Stores` (
    `StoreId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `TaxId` VARCHAR(50) NOT NULL,
    `PhoneNumber` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`StoreId`),
    KEY `IX_Stores_BranchId` (`BranchId`),
    KEY `IX_Stores_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_Stores_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Stores_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.15 Convenios Comerciales
CREATE TABLE IF NOT EXISTS `CommercialAgreements` (
    `AgreementId` CHAR(36) NOT NULL,
    `StoreId` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `MinPurchaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DiscountPercentage` DECIMAL(5,2) NULL,
    `DiscountFixedAmount` DECIMAL(18,2) NULL,
    `MaxHoursApplicable` INT NULL,
    `MaxMinutesApplicable` INT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `ImageUrl` LONGTEXT NULL,
    PRIMARY KEY (`AgreementId`),
    KEY `IX_CommercialAgreements_StoreId` (`StoreId`),
    CONSTRAINT `FK_CommercialAgreements_Stores_StoreId` FOREIGN KEY (`StoreId`) REFERENCES `Stores` (`StoreId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.16 Parametrización de Convenios Comerciales por Sede
CREATE TABLE IF NOT EXISTS `BranchCommercialAgreements` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `AgreementId` CHAR(36) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    `ResponsibleUserIdNavigationId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchCommercialAgreements_Branch_Agreement` (`BranchId`, `AgreementId`),
    KEY `IX_BranchCommercialAgreements_BranchId` (`BranchId`),
    KEY `IX_BranchCommercialAgreements_AgreementId` (`AgreementId`),
    CONSTRAINT `FK_BranchCommercialAgreements_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_BranchCommercialAgreements_CommercialAgreements_AgreementId` FOREIGN KEY (`AgreementId`) REFERENCES `CommercialAgreements` (`AgreementId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.17 Resoluciones de Facturación (DIAN / POS)
CREATE TABLE IF NOT EXISTS `BillingResolutions` (
    `ResolutionId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `Name` VARCHAR(150) NOT NULL,
    `DocumentType` VARCHAR(250) NOT NULL,
    `Prefix` VARCHAR(20) NOT NULL,
    `ResolutionNumber` VARCHAR(50) NOT NULL,
    `FromNumber` BIGINT NOT NULL,
    `ToNumber` BIGINT NOT NULL,
    `CurrentNumber` BIGINT NOT NULL DEFAULT 0,
    `ValidFrom` DATETIME(6) NOT NULL,
    `ValidTo` DATETIME(6) NOT NULL,
    `TechnicalKey` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NULL,
    PRIMARY KEY (`ResolutionId`),
    KEY `IX_BillingResolutions_BranchId` (`BranchId`),
    KEY `IX_BillingResolutions_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_BillingResolutions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_BillingResolutions_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.18 Tiquetes de Estacionamiento
CREATE TABLE IF NOT EXISTS `ParkingTickets` (
    `TicketId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `TicketNumber` VARCHAR(50) NOT NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `VehicleType` INT NOT NULL,
    `CustomerPhone` VARCHAR(30) NULL,
    `Notes` VARCHAR(500) NULL,
    `EntryTimeUtc` DATETIME(6) NOT NULL,
    `ExitTimeUtc` DATETIME(6) NULL,
    `TotalDurationMinutes` INT NOT NULL DEFAULT 0,
    `HourlyRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `GrossAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `NetAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AmountPaid` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ChangeGiven` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `PaymentMethod` INT NULL,
    `Status` INT NOT NULL,
    `OperatorName` VARCHAR(100) NOT NULL,
    `IsSynchronized` TINYINT(1) NOT NULL DEFAULT 1,
    `ResolutionId` CHAR(36) NULL,
    `ResolutionName` VARCHAR(150) NULL,
    `InvoiceNumber` VARCHAR(50) NULL,
    `IsElectronicInvoice` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`TicketId`),
    KEY `IX_ParkingTickets_BranchId` (`BranchId`),
    KEY `IX_ParkingTickets_CompanyId` (`CompanyId`),
    KEY `IX_ParkingTickets_PlateNumber` (`PlateNumber`),
    KEY `IX_ParkingTickets_Status` (`Status`),
    CONSTRAINT `FK_ParkingTickets_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ParkingTickets_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.19 Descuentos de Tiquetes por Convenios Comerciales
CREATE TABLE IF NOT EXISTS `TicketDiscounts` (
    `TicketDiscountId` CHAR(36) NOT NULL,
    `TicketId` CHAR(36) NOT NULL,
    `StoreId` CHAR(36) NOT NULL,
    `AgreementId` CHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL,
    `PurchaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AppliedDiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ValidatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `IsSynchronized` TINYINT(1) NOT NULL DEFAULT 1,
    PRIMARY KEY (`TicketDiscountId`),
    KEY `IX_TicketDiscounts_TicketId` (`TicketId`),
    KEY `IX_TicketDiscounts_StoreId` (`StoreId`),
    KEY `IX_TicketDiscounts_AgreementId` (`AgreementId`),
    CONSTRAINT `FK_TicketDiscounts_ParkingTickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `ParkingTickets` (`TicketId`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketDiscounts_Stores_StoreId` FOREIGN KEY (`StoreId`) REFERENCES `Stores` (`StoreId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TicketDiscounts_CommercialAgreements_AgreementId` FOREIGN KEY (`AgreementId`) REFERENCES `CommercialAgreements` (`AgreementId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.20 Turnos Operativos y Arqueos de Caja
CREATE TABLE IF NOT EXISTS `WorkShifts` (
    `ShiftId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `UserId` INT NOT NULL,
    `OperatorName` VARCHAR(100) NOT NULL,
    `CashRegisterName` VARCHAR(100) NOT NULL DEFAULT 'Caja Principal',
    `StartTimeUtc` DATETIME(6) NOT NULL,
    `EndTimeUtc` DATETIME(6) NULL,
    `BaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalCashCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalCardCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalTransferCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalDiscounts` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ExpectedCash` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ActualCashCounted` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `CashDifference` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalTicketsProcessed` INT NOT NULL DEFAULT 0,
    `TotalVehiclesEntered` INT NOT NULL DEFAULT 0,
    `Status` INT NOT NULL DEFAULT 0,
    `Notes` VARCHAR(500) NULL,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `ClosedAtUtc` DATETIME(6) NULL,
    PRIMARY KEY (`ShiftId`),
    KEY `IX_WorkShifts_BranchId` (`BranchId`),
    KEY `IX_WorkShifts_CompanyId` (`CompanyId`),
    KEY `IX_WorkShifts_UserId` (`UserId`),
    KEY `IX_WorkShifts_Status` (`Status`),
    CONSTRAINT `FK_WorkShifts_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_WorkShifts_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_WorkShifts_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.21 Mensualidades y Abonados
CREATE TABLE IF NOT EXISTS `MonthlySubscriptions` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `SubscriptionId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `CustomerName` VARCHAR(150) NOT NULL,
    `CustomerDocument` VARCHAR(50) NOT NULL,
    `CustomerPhone` VARCHAR(30) NOT NULL,
    `CustomerEmail` VARCHAR(100) NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `VehicleType` INT NOT NULL,
    `StartDateUtc` DATETIME(6) NOT NULL,
    `EndDateUtc` DATETIME(6) NOT NULL,
    `MonthlyFee` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AmountPaid` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `PaymentMethod` INT NOT NULL DEFAULT 1,
    `Notes` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    `ResponsibleUserIdNavigationId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_MonthlySubscriptions_BranchId` (`BranchId`),
    KEY `IX_MonthlySubscriptions_CompanyId` (`CompanyId`),
    KEY `IX_MonthlySubscriptions_PlateNumber` (`PlateNumber`),
    CONSTRAINT `FK_MonthlySubscriptions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_MonthlySubscriptions_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.22 Tarifas Vehiculares por Sede
CREATE TABLE IF NOT EXISTS `VehicleRates` (
    `RateId` CHAR(36) NOT NULL,
    `BranchId` INT NULL,
    `CompanyId` INT NULL,
    `VehicleType` INT NOT NULL,
    `DisplayName` VARCHAR(50) NOT NULL,
    `HourRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `MinuteRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `FullDayRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `NightRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `GracePeriodMinutes` INT NOT NULL DEFAULT 15,
    `IconKey` VARCHAR(50) NOT NULL DEFAULT 'IconCar',
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAtUtc` DATETIME(6) NULL,
    PRIMARY KEY (`RateId`),
    KEY `IX_VehicleRates_BranchId` (`BranchId`),
    KEY `IX_VehicleRates_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_VehicleRates_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_VehicleRates_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.23 Novedades e Incidencias Vehiculares
CREATE TABLE IF NOT EXISTS `VehicleIncidents` (
    `IncidentId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `IncidentType` VARCHAR(100) NOT NULL,
    `IsBlocked` TINYINT(1) NOT NULL DEFAULT 0,
    `IsGlobal` TINYINT(1) NOT NULL DEFAULT 1,
    `Description` LONGTEXT NOT NULL,
    `ReportedBy` VARCHAR(100) NOT NULL,
    `ContactPhone` VARCHAR(30) NULL,
    `Status` VARCHAR(30) NOT NULL DEFAULT 'Activa',
    `ResolvedNotes` LONGTEXT NULL,
    `ResolvedAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL,
    `UpdatedAtUtc` DATETIME NULL,
    PRIMARY KEY (`IncidentId`),
    KEY `IX_VehicleIncidents_PlateNumber` (`PlateNumber`),
    KEY `IX_VehicleIncidents_CompanyId` (`CompanyId`),
    KEY `IX_VehicleIncidents_BranchId` (`BranchId`),
    KEY `IX_VehicleIncidents_IsBlocked` (`IsBlocked`),
    KEY `IX_VehicleIncidents_IsGlobal` (`IsGlobal`),
    KEY `IX_VehicleIncidents_Status` (`Status`),
    CONSTRAINT `FK_VehicleIncidents_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_VehicleIncidents_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.24 Sedes Asignadas a Novedades
CREATE TABLE IF NOT EXISTS `VehicleIncidentBranches` (
    `IncidentId` CHAR(36) NOT NULL,
    `BranchId` INT NOT NULL,
    PRIMARY KEY (`IncidentId`, `BranchId`),
    KEY `IX_VehicleIncidentBranches_BranchId` (`BranchId`),
    CONSTRAINT `FK_VehicleIncidentBranches_VehicleIncidents_IncidentId` FOREIGN KEY (`IncidentId`) REFERENCES `VehicleIncidents` (`IncidentId`) ON DELETE CASCADE,
    CONSTRAINT `FK_VehicleIncidentBranches_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.25 Auditoría de Inicios de Sesión
CREATE TABLE IF NOT EXISTS `Login` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `Message` VARCHAR(255) NOT NULL DEFAULT '',
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_Login_UserId` (`UserId`),
    CONSTRAINT `FK_Login_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.26 Tokens de Restablecimiento de Contraseña
CREATE TABLE IF NOT EXISTS `PasswordResetToken` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `Token` VARCHAR(255) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_PasswordResetToken_UserId` (`UserId`),
    CONSTRAINT `FK_PasswordResetToken_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.27 Lotes de Estacionamiento
CREATE TABLE IF NOT EXISTS `ParkingLots` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` LONGTEXT NOT NULL,
    `Description` LONGTEXT NOT NULL,
    `ImageUrl` LONGTEXT NOT NULL,
    `IsMainImage` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.28 Relación Usuario - Lote de Estacionamiento
CREATE TABLE IF NOT EXISTS `UserParkings` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `ParkingLotId` INT NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_UserParkings_UserId` (`UserId`),
    KEY `IX_UserParkings_ParkingLotId` (`ParkingLotId`),
    CONSTRAINT `FK_UserParkings_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserParkings_ParkingLots_ParkingLotId` FOREIGN KEY (`ParkingLotId`) REFERENCES `ParkingLots` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.29 Control de Sesiones Concurrentes (UserSessions)
CREATE TABLE IF NOT EXISTS `UserSessions` (
    `SessionId` CHAR(36) NOT NULL,
    `UserId` INT NOT NULL,
    `Jti` VARCHAR(64) NOT NULL,
    `DeviceInfo` VARCHAR(150) NULL,
    `IpAddress` VARCHAR(45) NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ExpiresAtUtc` DATETIME NOT NULL,
    `IsRevoked` BOOLEAN NOT NULL DEFAULT 0,
    `RevokedAtUtc` DATETIME NULL,
    `RevokedReason` VARCHAR(50) NULL,
    PRIMARY KEY (`SessionId`),
    KEY `IX_UserSessions_UserId_IsRevoked_ExpiresAtUtc` (`UserId`, `IsRevoked`, `ExpiresAtUtc`),
    KEY `IX_UserSessions_Jti` (`Jti`),
    CONSTRAINT `FK_UserSessions_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ==================================================================================
-- FASE 2: INSERCIÓN DE DATOS SEMILLA (SEEDS - ARRANQUE LIMPIO)
-- ==================================================================================

-- ----------------------------------------------------------------------------------
-- 2.0 REGISTRO EN HISTORIAL DE MIGRACIONES EF CORE
-- ----------------------------------------------------------------------------------
INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES 
    ('20260904144753_VersionBase', '9.0.0')
ON DUPLICATE KEY UPDATE `ProductVersion` = VALUES(`ProductVersion`);

-- ----------------------------------------------------------------------------------
-- 2.1 MULTI-TENANT LIMPIO: CERO (0) EMPRESAS, CERO (0) SEDES, CERO (0) PLANES
-- ----------------------------------------------------------------------------------
-- NOTA: No se insertan empresas, sedes ni planes preconfigurados en este seed.
-- La plataforma arranca 100% limpia para que el Super Administrador ('admin')
-- gestione planes en COP y cree clientes corporativos desde el panel de control.

-- ----------------------------------------------------------------------------------
-- 2.2 TIPOS DE IDENTIFICACIÓN (IdentificationType)
-- ----------------------------------------------------------------------------------
INSERT INTO IdentificationType (Id, Identification, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, 'CC',  1, UTC_TIMESTAMP(), NULL),
    (2, 'CE',  1, UTC_TIMESTAMP(), NULL),
    (3, 'NIT', 1, UTC_TIMESTAMP(), NULL),
    (4, 'PAS', 1, UTC_TIMESTAMP(), NULL),
    (5, 'PEP', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Identification = new_row.Identification, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.3 ROL SUPER ADMINISTRADOR GLOBAL SAAS (CompanyId NULL)
-- ----------------------------------------------------------------------------------
INSERT INTO UserRole (Id, CompanyId, BranchId, Role, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, NULL, NULL, 'Super Administrador', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Role = new_row.Role,
    CompanyId = NULL,
    BranchId = NULL,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.4 USUARIO SUPERADMINISTRADOR DE PLATAFORMA (User) - CompanyId NULL
-- Contraseña por defecto: 'Admin2026*' (BCrypt)
-- ----------------------------------------------------------------------------------
INSERT INTO User (
    Id, CompanyId, UserRoleId, IdentificationTypeId, IdentificationNumber, 
    FirstName, MiddleName, FirstSurname, SecondLastName, FullName,
    Username, Password, Email, IsActive, MustChangePassword, CreatedAt
)
VALUES (
    1, NULL, 1, 1, '1000000000', 
    'Super', 'Admin', 'Global', 'SaaS', 'Super Administrador SaaS',
    'admin', 
    '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',
    'admin@parkpoint.local', 
    1, 0, UTC_TIMESTAMP()
)
AS new_row
ON DUPLICATE KEY UPDATE 
    CompanyId = NULL,
    UserRoleId = new_row.UserRoleId,
    IdentificationTypeId = new_row.IdentificationTypeId,
    IdentificationNumber = new_row.IdentificationNumber,
    FullName = new_row.FullName,
    Email = new_row.Email,
    Password = new_row.Password,
    IsActive = 1;

-- ----------------------------------------------------------------------------------
-- 2.5 CATÁLOGO COMPLETO DE 17 MÓDULOS (Module)
-- ----------------------------------------------------------------------------------
INSERT INTO Module (Id, Name, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    -- Módulos Operativos (WPF / PWA)
    (1,  'Ingreso de Vehículos (CheckIn)',      1, UTC_TIMESTAMP(), NULL),
    (2,  'Salida y Cobro (CheckOut)',           1, UTC_TIMESTAMP(), NULL),
    (3,  'Mensualidades y Abonados',            1, UTC_TIMESTAMP(), NULL),
    (4,  'Vehículos en Patio y Monitoreo',      1, UTC_TIMESTAMP(), NULL),
    (5,  'Control de Turnos y Caja',            1, UTC_TIMESTAMP(), NULL),
    (6,  'Analítica, Métricas y Finanzas',      1, UTC_TIMESTAMP(), NULL),

    -- Módulos Administrativos y Configuración (PWA / Panel Central)
    (7,  'Gestión de Sedes y Parqueaderos',     1, UTC_TIMESTAMP(), NULL),
    (8,  'Gestión de Tarifas y Vehículos',      1, UTC_TIMESTAMP(), NULL),
    (9,  'Medios de Pago Maestros',             1, UTC_TIMESTAMP(), NULL),
    (10, 'Convenios y Comercios Aliados',       1, UTC_TIMESTAMP(), NULL),
    (11, 'Seguridad, Usuarios y Roles',         1, UTC_TIMESTAMP(), NULL),
    (12, 'Matriz de Permisos RBAC',             1, UTC_TIMESTAMP(), NULL),
    (13, 'Configuración y Sistema',             1, UTC_TIMESTAMP(), NULL),
    (14, 'Resoluciones de Facturación',         1, UTC_TIMESTAMP(), NULL),
    (15, 'Novedades y Bloqueo de Placas',       1, UTC_TIMESTAMP(), NULL),
    (16, 'Gestión de Empresas SaaS',            1, UTC_TIMESTAMP(), NULL),
    (17, 'Planes y Suscripciones SaaS',         1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.6 OPERACIONES BASE (Operation) - 7 Operaciones Estándar
-- ----------------------------------------------------------------------------------
INSERT INTO Operation (Id, Name, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, 'READ',    1, UTC_TIMESTAMP(), NULL),
    (2, 'CREATE',  1, UTC_TIMESTAMP(), NULL),
    (3, 'UPDATE',  1, UTC_TIMESTAMP(), NULL),
    (4, 'DELETE',  1, UTC_TIMESTAMP(), NULL),
    (5, 'EXECUTE', 1, UTC_TIMESTAMP(), NULL),
    (6, 'PRINT',   1, UTC_TIMESTAMP(), NULL),
    (7, 'ASSIGN',  1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.7 ACCIONES Y SLUGS REALES DEL SISTEMA (Action) - Total: 82 Acciones
-- ----------------------------------------------------------------------------------
INSERT INTO Action (Id, ModuleId, OperationId, Name, Slug, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    -- ==============================================================================
    -- MÓDULO 1: INGRESO DE VEHÍCULOS (CheckIn - WPF & PWA)
    -- ==============================================================================
    (1,  1, 1, 'Ver módulo de ingreso', 'checkin.view', 1, UTC_TIMESTAMP(), NULL),
    (2,  1, 2, 'Generar e imprimir tiquete de ingreso', 'checkin.create', 1, UTC_TIMESTAMP(), NULL),
    (3,  1, 6, 'Reimprimir tiquete de ingreso', 'checkin.reprint', 1, UTC_TIMESTAMP(), NULL),
    (4,  1, 3, 'Editar datos o placa en ingreso', 'checkin.edit_plate', 1, UTC_TIMESTAMP(), NULL),
    (5,  1, 5, 'Abrir barrera de entrada manualmente', 'checkin.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 2: SALIDA Y COBRO (CheckOut - WPF & PWA)
    -- ==============================================================================
    (6,  2, 1, 'Ver módulo de cobro y salida', 'checkout.view', 1, UTC_TIMESTAMP(), NULL),
    (7,  2, 2, 'Liquidar y cobrar tiquete', 'checkout.process_payment', 1, UTC_TIMESTAMP(), NULL),
    (8,  2, 2, 'Aplicar descuentos comerciales o de convenio', 'checkout.apply_discount', 1, UTC_TIMESTAMP(), NULL),
    (9,  2, 3, 'Exonerar o anular cobro de tiquete', 'checkout.waive_fee', 1, UTC_TIMESTAMP(), NULL),
    (10, 2, 6, 'Reimprimir factura o comprobante de salida', 'checkout.reprint_receipt', 1, UTC_TIMESTAMP(), NULL),
    (11, 2, 5, 'Abrir barrera de salida manualmente', 'checkout.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 3: MENSUALIDADES Y ABONADOS (Subscriptions - PWA & WPF)
    -- ==============================================================================
    (12, 3, 1, 'Ver listado de mensualidades y abonados', 'subscriptions.view', 1, UTC_TIMESTAMP(), NULL),
    (13, 3, 2, 'Registrar nuevo abonado / mensualidad', 'subscriptions.create', 1, UTC_TIMESTAMP(), NULL),
    (14, 3, 3, 'Renovar mensualidad o recibir pago', 'subscriptions.renew', 1, UTC_TIMESTAMP(), NULL),
    (15, 3, 3, 'Editar datos de abonado o vehículo', 'subscriptions.edit', 1, UTC_TIMESTAMP(), NULL),
    (16, 3, 4, 'Cancelar o inactivar suscripción', 'subscriptions.cancel', 1, UTC_TIMESTAMP(), NULL),
    (17, 3, 6, 'Imprimir recibo de pago de mensualidad', 'subscriptions.print_receipt', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 4: VEHÍCULOS EN PATIO Y MONITOREO (Live Monitoring - WPF & PWA)
    -- ==============================================================================
    (18, 4, 1, 'Ver mapa de ocupación y vehículos en patio', 'monitoring.view_occupancy', 1, UTC_TIMESTAMP(), NULL),
    (19, 4, 1, 'Consultar historial y detalle de vehículos adentro', 'monitoring.search_vehicles', 1, UTC_TIMESTAMP(), NULL),
    (20, 4, 3, 'Forzar salida manual de vehículo en patio', 'monitoring.force_exit', 1, UTC_TIMESTAMP(), NULL),
    (21, 4, 1, 'Exportar listado de vehículos activos', 'monitoring.export', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 5: CONTROL DE TURNOS Y CAJA (Shifts - WPF & PWA)
    -- ==============================================================================
    (22, 5, 1, 'Ver estado del turno actual y balance de caja', 'shifts.view_current', 1, UTC_TIMESTAMP(), NULL),
    (23, 5, 2, 'Abrir nuevo turno con base de efectivo', 'shifts.open', 1, UTC_TIMESTAMP(), NULL),
    (24, 5, 5, 'Efectuar arqueo ciego o parcial', 'shifts.blind_count', 1, UTC_TIMESTAMP(), NULL),
    (25, 5, 5, 'Cerrar turno y generar corte Z', 'shifts.close', 1, UTC_TIMESTAMP(), NULL),
    (26, 5, 1, 'Consultar historial de turnos y cierres anteriores', 'shifts.view_history', 1, UTC_TIMESTAMP(), NULL),
    (27, 5, 6, 'Reimprimir comprobante de cierre de turno', 'shifts.reprint_closure', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 6: ANALÍTICA, MÉTRICAS Y FINANZAS (Analytics & Reports - PWA)
    -- ==============================================================================
    (28, 6, 1, 'Ver panel ejecutivo y KPIs en tiempo real', 'analytics.view_dashboard', 1, UTC_TIMESTAMP(), NULL),
    (29, 6, 1, 'Consultar reportes de ingresos financieros', 'analytics.income_reports', 1, UTC_TIMESTAMP(), NULL),
    (30, 6, 1, 'Consultar reportes de ocupación y afluencia', 'analytics.occupancy_reports', 1, UTC_TIMESTAMP(), NULL),
    (31, 6, 1, 'Consultar auditoría de anulaciones y descuentos', 'analytics.audit_reports', 1, UTC_TIMESTAMP(), NULL),
    (32, 6, 1, 'Exportar reportes a Excel / PDF', 'analytics.export', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 7: GESTIÓN DE SEDES Y PARQUEADEROS (Branches - PWA)
    -- ==============================================================================
    (33, 7, 1, 'Ver listado de sedes y parqueaderos', 'branches.view', 1, UTC_TIMESTAMP(), NULL),
    (34, 7, 2, 'Crear nueva sede o parqueadero', 'branches.create', 1, UTC_TIMESTAMP(), NULL),
    (35, 7, 3, 'Editar capacidad y datos de la sede', 'branches.edit', 1, UTC_TIMESTAMP(), NULL),
    (36, 7, 4, 'Inactivar sede o parqueadero', 'branches.delete', 1, UTC_TIMESTAMP(), NULL),
    (37, 7, 7, 'Asignar operadores a sede', 'branches.assign_users', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 8: GESTIÓN DE TARIFAS Y VEHÍCULOS (Rates & Vehicle Types - PWA)
    -- ==============================================================================
    (38, 8, 1, 'Ver catálogo de tarifas por tipo de vehículo', 'rates.view', 1, UTC_TIMESTAMP(), NULL),
    (39, 8, 2, 'Crear nueva tarifa de vehículo', 'rates.create', 1, UTC_TIMESTAMP(), NULL),
    (40, 8, 3, 'Editar precios por hora, minuto o día', 'rates.edit', 1, UTC_TIMESTAMP(), NULL),
    (41, 8, 4, 'Inactivar tarifa de vehículo', 'rates.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 9: MEDIOS DE PAGO MAESTROS (Payment Methods - PWA)
    -- ==============================================================================
    (42, 9, 1, 'Ver catálogo de medios de pago', 'payment_methods.view', 1, UTC_TIMESTAMP(), NULL),
    (43, 9, 2, 'Crear nuevo medio de pago', 'payment_methods.create', 1, UTC_TIMESTAMP(), NULL),
    (44, 9, 3, 'Editar medio de pago e ícono', 'payment_methods.edit', 1, UTC_TIMESTAMP(), NULL),
    (45, 9, 4, 'Inactivar medio de pago', 'payment_methods.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 10: CONVENIOS Y COMERCIOS ALIADOS (Agreements & Stores - PWA)
    -- ==============================================================================
    (46, 10, 1, 'Ver comercios y convenios activos', 'agreements.view', 1, UTC_TIMESTAMP(), NULL),
    (47, 10, 2, 'Registrar comercio o convenio comercial', 'agreements.create', 1, UTC_TIMESTAMP(), NULL),
    (48, 10, 3, 'Editar reglas de descuento de convenio', 'agreements.edit', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 11: SEGURIDAD, USUARIOS Y ROLES (Users & Roles - PWA)
    -- ==============================================================================
    (49, 11, 1, 'Ver usuarios del sistema', 'users.view', 1, UTC_TIMESTAMP(), NULL),
    (50, 11, 2, 'Crear nuevo usuario operador / administrador', 'users.create', 1, UTC_TIMESTAMP(), NULL),
    (51, 11, 3, 'Editar datos de usuario y restablecer contraseñas', 'users.edit', 1, UTC_TIMESTAMP(), NULL),
    (52, 11, 4, 'Inactivar usuario del sistema', 'users.delete', 1, UTC_TIMESTAMP(), NULL),
    (53, 11, 1, 'Ver catálogo de roles de usuario', 'roles.view', 1, UTC_TIMESTAMP(), NULL),
    (54, 11, 2, 'Crear nuevo rol de usuario', 'roles.create', 1, UTC_TIMESTAMP(), NULL),
    (55, 11, 3, 'Editar nombre y estado de rol', 'roles.edit', 1, UTC_TIMESTAMP(), NULL),
    (56, 11, 4, 'Inactivar rol de usuario', 'roles.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 12: MATRIZ DE PERMISOS RBAC (Permissions - PWA)
    -- ==============================================================================
    (57, 12, 1, 'Ver matriz de permisos por rol', 'permissions.view', 1, UTC_TIMESTAMP(), NULL),
    (58, 12, 7, 'Asignar y revocar acciones y módulos a roles', 'permissions.assign', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 13: CONFIGURACIÓN Y SISTEMA (System - WPF & PWA)
    -- ==============================================================================
    (59, 13, 5, 'Ejecutar sincronización manual en caliente', 'system.sync', 1, UTC_TIMESTAMP(), NULL),
    (60, 13, 5, 'Limpiar caché local y forzar resincronización', 'system.clean_cache', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 14: RESOLUCIONES DE FACTURACIÓN (Resolutions - DIAN / POS - PWA)
    -- ==============================================================================
    (61, 14, 1, 'Ver catálogo de resoluciones de facturación', 'resolutions.view', 1, UTC_TIMESTAMP(), NULL),
    (62, 14, 2, 'Crear nueva resolución de facturación DIAN / POS', 'resolutions.create', 1, UTC_TIMESTAMP(), NULL),
    (63, 14, 3, 'Editar rangos, prefijos y vigencias de resolución', 'resolutions.edit', 1, UTC_TIMESTAMP(), NULL),
    (64, 14, 4, 'Inactivar resolución de facturación', 'resolutions.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 15: NOVEDADES Y BLOQUEO DE PLACAS (Incidents & Blacklist - PWA & WPF)
    -- ==============================================================================
    (65, 15, 1, 'Ver novedades e incidencias de vehículos', 'novedades.view', 1, UTC_TIMESTAMP(), NULL),
    (66, 15, 2, 'Registrar novedad o bloqueo restrictivo de placa', 'novedades.create', 1, UTC_TIMESTAMP(), NULL),
    (67, 15, 3, 'Editar observaciones y contacto de novedad', 'novedades.edit', 1, UTC_TIMESTAMP(), NULL),
    (68, 15, 5, 'Resolver novedad y levantar restricción de placa', 'novedades.resolve', 1, UTC_TIMESTAMP(), NULL),
    (69, 15, 4, 'Eliminar registro de novedad', 'novedades.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 16: GESTIÓN DE EMPRESAS SAAS (Multi-Tenant SaaS - SuperAdmin)
    -- ==============================================================================
    (70, 16, 1, 'Ver empresas clientes registradas en la plataforma', 'companies.view', 1, UTC_TIMESTAMP(), NULL),
    (71, 16, 2, 'Crear nueva empresa cliente y su administrador inicial', 'companies.create', 1, UTC_TIMESTAMP(), NULL),
    (72, 16, 3, 'Editar datos, sedes máximas y planes de empresa', 'companies.edit', 1, UTC_TIMESTAMP(), NULL),
    (73, 16, 5, 'Suspender o reactivar empresa por suscripción', 'companies.suspend', 1, UTC_TIMESTAMP(), NULL),
    (74, 16, 4, 'Eliminar o dar de baja empresa del sistema', 'companies.delete', 1, UTC_TIMESTAMP(), NULL),
    (75, 6,  1, 'Consultar métricas operativas y gráficas de afluencia', 'analytics.metrics', 1, UTC_TIMESTAMP(), NULL),
    (76, 10, 4, 'Inactivar o eliminar convenio comercial', 'agreements.delete', 1, UTC_TIMESTAMP(), NULL),
    (77, 16, 7, 'Asignar límites de sedes y capacidad SaaS', 'companies.assign_limits', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULO 17: PLANES Y SUSCRIPCIONES SAAS (SaaS Plans & Billing - SuperAdmin)
    -- ==============================================================================
    (78, 17, 1, 'Ver catálogo de planes SaaS', 'plans.view', 1, UTC_TIMESTAMP(), NULL),
    (79, 17, 2, 'Crear nuevo plan SaaS en COP', 'plans.create', 1, UTC_TIMESTAMP(), NULL),
    (80, 17, 3, 'Editar precios, cuotas y módulos de plan', 'plans.edit', 1, UTC_TIMESTAMP(), NULL),
    (81, 17, 5, 'Activar o inactivar plan de suscripción', 'plans.toggle_status', 1, UTC_TIMESTAMP(), NULL),
    (82, 17, 4, 'Eliminar plan de suscripción', 'plans.delete', 1, UTC_TIMESTAMP(), NULL),

    -- ==============================================================================
    -- MÓDULOS DE TERMINAL POS GARITA (WPF Dedicado e Independiente)
    -- ==============================================================================
    -- MÓDULO 1: INGRESO DE VEHÍCULOS (CheckIn WPF POS)
    (83,  1, 1, 'Ver terminal de ingreso WPF', 'wpf.checkin.view', 1, UTC_TIMESTAMP(), NULL),
    (84,  1, 2, 'Generar e imprimir tiquete en terminal WPF', 'wpf.checkin.create', 1, UTC_TIMESTAMP(), NULL),
    (85,  1, 6, 'Reimprimir tiquete de ingreso en terminal WPF', 'wpf.checkin.reprint', 1, UTC_TIMESTAMP(), NULL),
    (86,  1, 3, 'Editar datos o placa en ingreso WPF', 'wpf.checkin.edit_plate', 1, UTC_TIMESTAMP(), NULL),
    (87,  1, 5, 'Abrir barrera de entrada manualmente en terminal WPF', 'wpf.checkin.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- MÓDULO 2: SALIDA Y COBRO (CheckOut WPF POS)
    (88,  2, 1, 'Ver terminal de cobro y salida WPF', 'wpf.checkout.view', 1, UTC_TIMESTAMP(), NULL),
    (89,  2, 2, 'Liquidar y cobrar tiquete en terminal WPF', 'wpf.checkout.process_payment', 1, UTC_TIMESTAMP(), NULL),
    (90,  2, 2, 'Aplicar convenios y descuentos en terminal WPF', 'wpf.checkout.apply_discount', 1, UTC_TIMESTAMP(), NULL),
    (91,  2, 3, 'Exonerar cobro de tiquete en terminal WPF', 'wpf.checkout.waive_fee', 1, UTC_TIMESTAMP(), NULL),
    (92,  2, 6, 'Reimprimir factura o comprobante en terminal WPF', 'wpf.checkout.reprint_receipt', 1, UTC_TIMESTAMP(), NULL),
    (93,  2, 5, 'Abrir barrera de salida manualmente en terminal WPF', 'wpf.checkout.manual_barrier', 1, UTC_TIMESTAMP(), NULL),

    -- MÓDULO 3: MENSUALIDADES Y ABONADOS (WPF POS)
    (94,  3, 1, 'Ver catálogo de mensualidades en terminal WPF', 'wpf.subscriptions.view', 1, UTC_TIMESTAMP(), NULL),
    (95,  3, 2, 'Registrar nuevo abonado en terminal WPF', 'wpf.subscriptions.create', 1, UTC_TIMESTAMP(), NULL),
    (96,  3, 3, 'Renovar mensualidad en terminal WPF', 'wpf.subscriptions.renew', 1, UTC_TIMESTAMP(), NULL),
    (97,  3, 4, 'Cancelar suscripción en terminal WPF', 'wpf.subscriptions.cancel', 1, UTC_TIMESTAMP(), NULL),

    -- MÓDULO 4: MONITOREO DE PATIO (WPF POS)
    (98,  4, 1, 'Ver mapa de ocupación en terminal WPF', 'wpf.monitoring.view_occupancy', 1, UTC_TIMESTAMP(), NULL),
    (99,  4, 1, 'Buscar vehículos adentro en terminal WPF', 'wpf.monitoring.search_vehicles', 1, UTC_TIMESTAMP(), NULL),
    (100, 4, 3, 'Forzar salida manual en terminal WPF', 'wpf.monitoring.force_exit', 1, UTC_TIMESTAMP(), NULL),
    (101, 4, 1, 'Exportar vehículos activos desde terminal WPF', 'wpf.monitoring.export', 1, UTC_TIMESTAMP(), NULL),

    -- MÓDULO 5: CONTROL DE TURNOS Y CAJA (WPF POS)
    (102, 5, 1, 'Ver turno actual y balance en terminal WPF', 'wpf.shifts.view_current', 1, UTC_TIMESTAMP(), NULL),
    (103, 5, 2, 'Abrir turno con base en terminal WPF', 'wpf.shifts.open', 1, UTC_TIMESTAMP(), NULL),
    (104, 5, 5, 'Arqueo ciego / retiro de efectivo en terminal WPF', 'wpf.shifts.blind_count', 1, UTC_TIMESTAMP(), NULL),
    (105, 5, 5, 'Cerrar turno de caja y corte Z en terminal WPF', 'wpf.shifts.close', 1, UTC_TIMESTAMP(), NULL),
    (106, 5, 1, 'Ver historial de turnos en terminal WPF', 'wpf.shifts.view_history', 1, UTC_TIMESTAMP(), NULL),
    (107, 5, 6, 'Reimprimir comprobante de cierre en terminal WPF', 'wpf.shifts.reprint_closure', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    ModuleId = new_row.ModuleId,
    OperationId = new_row.OperationId,
    Name = new_row.Name,
    Slug = new_row.Slug,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.8 MATRIZ DE MÓDULOS POR ROL (UserRoleModule)
-- Asignación del 100% de los 17 Módulos ÚNICAMENTE al Rol 1 (Super Administrador)
-- ----------------------------------------------------------------------------------
DELETE FROM `UserRoleModule` WHERE `UserRoleId` = 1;

INSERT INTO `UserRoleModule` (`UserRoleId`, `ModulesRoleId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Module`;

-- ----------------------------------------------------------------------------------
-- 2.9 MATRIZ DE PERMISOS: ROL ACCIONES (RoleAction)
-- Asignación del 100% de las 82 Acciones ÚNICAMENTE al Rol 1 (Super Administrador) - FULL ACCESS
-- ----------------------------------------------------------------------------------
DELETE FROM `RoleAction` WHERE `RoleId` = 1;

INSERT INTO `RoleAction` (`RoleId`, `ActionId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Action`;

SET FOREIGN_KEY_CHECKS = 1;

-- ==================================================================================
-- FASE 3: VERIFICACIÓN Y AUDITORÍA FINAL
-- ==================================================================================
SELECT 
    u.Id AS UsuarioId,
    u.Username AS Usuario,
    u.FullName AS NombreCompleto,
    COALESCE(c.Name, 'Sin Empresa (Global SaaS)') AS EmpresaAsignada,
    r.Role AS RolAsignado,
    COUNT(DISTINCT urm.ModulesRoleId) AS TotalModulosAsignados,
    COUNT(DISTINCT ra.ActionId) AS TotalAccionesAsignadas,
    (SELECT COUNT(*) FROM Companies) AS TotalEmpresasEnSistema,
    (SELECT COUNT(*) FROM Branches) AS TotalSedesEnSistema,
    (SELECT COUNT(*) FROM Plans) AS TotalPlanesEnSistema
FROM User u
LEFT JOIN Companies c ON u.CompanyId = c.Id
INNER JOIN UserRole r ON u.UserRoleId = r.Id
LEFT JOIN UserRoleModule urm ON urm.UserRoleId = r.Id AND urm.IsActive = 1
LEFT JOIN RoleAction ra ON ra.RoleId = r.Id AND ra.IsActive = 1
WHERE u.Username = 'admin'
GROUP BY u.Id, u.Username, u.FullName, c.Name, r.Role;
