-- ==================================================================================
-- SCRIPT: 02_Init_RBAC_Seed.sql
-- DESCRIPCIÓN: Script Oficial y Completo de Inicialización de Esquema Multi-Tenant SaaS y RBAC Seed.
-- MOTOR: MySQL 8.x / MariaDB
-- REGLAS DE NEGOCIO SAAS MULTI-TENANT:
--   1. Aprovisionamiento seguro DDL (CREATE TABLE IF NOT EXISTS) para base de datos vacía.
--   2. Soporte completo Multi-Tenant con discriminador CompanyId y aislamiento relacional.
--   3. Inicializa Tipos de Identificación estándar (CC, CE, NIT, PAS, PEP).
--   4. Inicializa la Empresa Matriz Global SaaS (Id 1) para el SuperAdmin.
--   5. Inicializa ÚNICAMENTE el Rol 'Administrador' (Id 1) con acceso al 100% de los módulos y acciones.
--   6. Inicializa el Usuario 'admin' (SuperAdmin de la Plataforma SaaS).
--   7. Catálogo de los 16 Módulos del sistema (Terminal WPF, Administración PWA y Gestión SaaS).
--   8. Catálogo de 7 Operaciones estándar del sistema.
--   9. Catálogo completo de 74 Acciones y Slugs reales del sistema (incluye companies.*).
--  10. Asigna el 100% de los 16 Módulos y el 100% de las 74 Acciones al Rol Administrador.
-- ==================================================================================

SET FOREIGN_KEY_CHECKS = 0;

-- ==================================================================================
-- FASE 1: DDL SEGURO - CREACIÓN DE TABLAS (SI NO EXISTEN)
-- ==================================================================================

-- 1.0 Empresas / Tenants (Multi-Tenant SaaS)
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
    `MaxBranches` INT NOT NULL DEFAULT 1,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `SubscriptionExpiresAt` DATETIME NULL,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Companies_Nit` (`Nit`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.1 Tipos de Identificación
CREATE TABLE IF NOT EXISTS `IdentificationType` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Identification` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.2 Roles de Usuario (SaaS: CompanyId NULL para roles del sistema / Globales)
CREATE TABLE IF NOT EXISTS `UserRole` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NULL,
    `Role` VARCHAR(50) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_UserRole_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_UserRole_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.3 Usuarios (SaaS: CompanyId NULL para SuperAdmin de Plataforma)
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
    CONSTRAINT `FK_User_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.4 Módulos
CREATE TABLE IF NOT EXISTS `Module` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.5 Operaciones
CREATE TABLE IF NOT EXISTS `Operation` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(100) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.6 Acciones y Permisos
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
    UNIQUE KEY `UX_Action_Slug` (`Slug`),
    KEY `IX_Action_ModuleId` (`ModuleId`),
    KEY `IX_Action_OperationId` (`OperationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.7 Relación Rol - Acciones (Permisos)
CREATE TABLE IF NOT EXISTS `RoleAction` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `RoleId` INT NOT NULL,
    `ActionId` INT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_RoleAction_Role_Action` (`RoleId`, `ActionId`),
    KEY `IX_RoleAction_ActionId` (`ActionId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.8 Relación Rol - Módulos
CREATE TABLE IF NOT EXISTS `UserRoleModule` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserRoleId` INT NOT NULL,
    `ModulesRoleId` INT NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_UserRoleModule_Role_Module` (`UserRoleId`, `ModulesRoleId`),
    KEY `IX_UserRoleModule_ModulesRoleId` (`ModulesRoleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.9 Medios de Pago Maestros
CREATE TABLE IF NOT EXISTS `PaymentMethod` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(50) NOT NULL,
    `Icon` VARCHAR(50) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.10 Sedes / Parqueaderos (Pertenecen obligatoriamente a una Empresa)
CREATE TABLE IF NOT EXISTS `Branches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `CompanyId` INT NOT NULL,
    `Code` VARCHAR(20) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `Address` VARCHAR(200) NOT NULL,
    `Phone` VARCHAR(30) NULL,
    `City` VARCHAR(50) NULL,
    `TotalCapacity` INT NOT NULL DEFAULT 100,
    `Notes` VARCHAR(500) NULL,
    `LogoBase64` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_Branches_Code` (`Code`),
    KEY `IX_Branches_CompanyId` (`CompanyId`),
    CONSTRAINT `FK_Branches_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.11 Asignación Usuario - Sedes
CREATE TABLE IF NOT EXISTS `UserBranches` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `UserId` INT NOT NULL,
    `BranchId` INT NOT NULL,
    `IsDefault` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_UserBranches_User_Branch` (`UserId`, `BranchId`),
    KEY `IX_UserBranches_BranchId` (`BranchId`),
    CONSTRAINT `FK_UserBranches_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserBranches_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.12 Medios de Pago por Sede
CREATE TABLE IF NOT EXISTS `BranchPaymentMethods` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `PaymentMethodId` INT NOT NULL,
    `RequiresCashTender` TINYINT(1) NOT NULL DEFAULT 0,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchPaymentMethods_Branch_PaymentMethod` (`BranchId`, `PaymentMethodId`),
    KEY `IX_BranchPaymentMethods_PaymentMethodId` (`PaymentMethodId`),
    CONSTRAINT `FK_BranchPaymentMethods_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_BranchPaymentMethods_PaymentMethod_PaymentMethodId` FOREIGN KEY (`PaymentMethodId`) REFERENCES `PaymentMethod` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.13 Tarifas de Vehículos
CREATE TABLE IF NOT EXISTS `VehicleRates` (
    `RateId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `DisplayName` VARCHAR(50) NOT NULL,
    `IconKey` VARCHAR(50) NULL,
    `HourRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `MinuteRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `FullDayRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `GracePeriodMinutes` INT NOT NULL DEFAULT 15,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`RateId`),
    KEY `IX_VehicleRates_CompanyId` (`CompanyId`),
    KEY `IX_VehicleRates_BranchId` (`BranchId`),
    CONSTRAINT `FK_VehicleRates_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_VehicleRates_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.14 Comercios Aliados
CREATE TABLE IF NOT EXISTS `Stores` (
    `StoreId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `TaxId` VARCHAR(50) NULL,
    `Phone` VARCHAR(30) NULL,
    `ContactPerson` VARCHAR(100) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`StoreId`),
    KEY `IX_Stores_CompanyId` (`CompanyId`),
    KEY `IX_Stores_BranchId` (`BranchId`),
    CONSTRAINT `FK_Stores_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Stores_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.15 Convenios Comerciales
CREATE TABLE IF NOT EXISTS `CommercialAgreements` (
    `AgreementId` CHAR(36) NOT NULL,
    `StoreId` CHAR(36) NOT NULL,
    `Name` VARCHAR(100) NOT NULL,
    `MinPurchaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DiscountPercentage` DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    `DiscountFixedAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DiscountType` VARCHAR(50) NOT NULL DEFAULT 'Percentage',
    `MaxHours` INT NULL,
    `ImageUrl` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`AgreementId`),
    KEY `IX_CommercialAgreements_StoreId` (`StoreId`),
    CONSTRAINT `FK_CommercialAgreements_Stores_StoreId` FOREIGN KEY (`StoreId`) REFERENCES `Stores` (`StoreId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.16 Tiquetes de Estacionamiento
CREATE TABLE IF NOT EXISTS `ParkingTickets` (
    `TicketId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `TicketNumber` VARCHAR(50) NOT NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `VehicleType` VARCHAR(50) NOT NULL,
    `CustomerPhone` VARCHAR(30) NULL,
    `Notes` VARCHAR(500) NULL,
    `EntryTime` DATETIME(6) NOT NULL,
    `ExitTime` DATETIME(6) NULL,
    `OperatorName` VARCHAR(100) NOT NULL,
    `HourlyRate` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `GrossAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `DiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `NetAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AmountPaid` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ChangeGiven` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `PaymentMethod` VARCHAR(50) NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Active',
    `IsPrepaid` TINYINT(1) NOT NULL DEFAULT 0,
    `ResolutionId` CHAR(36) NULL,
    `ResolutionName` VARCHAR(150) NULL,
    `InvoiceNumber` VARCHAR(50) NULL,
    `IsElectronicInvoice` TINYINT(1) NOT NULL DEFAULT 0,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`TicketId`),
    UNIQUE KEY `UX_ParkingTickets_TicketNumber` (`TicketNumber`),
    KEY `IX_ParkingTickets_PlateNumber` (`PlateNumber`),
    KEY `IX_ParkingTickets_CompanyId` (`CompanyId`),
    KEY `IX_ParkingTickets_BranchId` (`BranchId`),
    KEY `IX_ParkingTickets_ResolutionId` (`ResolutionId`),
    CONSTRAINT `FK_ParkingTickets_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_ParkingTickets_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.17 Descuentos de Tiquetes
CREATE TABLE IF NOT EXISTS `TicketDiscounts` (
    `TicketDiscountId` CHAR(36) NOT NULL,
    `TicketId` CHAR(36) NOT NULL,
    `StoreId` CHAR(36) NOT NULL,
    `AgreementId` CHAR(36) NOT NULL,
    `InvoiceNumber` VARCHAR(50) NOT NULL,
    `PurchaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AppliedDiscountAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    PRIMARY KEY (`TicketDiscountId`),
    KEY `IX_TicketDiscounts_TicketId` (`TicketId`),
    KEY `IX_TicketDiscounts_StoreId` (`StoreId`),
    KEY `IX_TicketDiscounts_AgreementId` (`AgreementId`),
    CONSTRAINT `FK_TicketDiscounts_ParkingTickets_TicketId` FOREIGN KEY (`TicketId`) REFERENCES `ParkingTickets` (`TicketId`) ON DELETE CASCADE,
    CONSTRAINT `FK_TicketDiscounts_Stores_StoreId` FOREIGN KEY (`StoreId`) REFERENCES `Stores` (`StoreId`) ON DELETE RESTRICT,
    CONSTRAINT `FK_TicketDiscounts_CommercialAgreements_AgreementId` FOREIGN KEY (`AgreementId`) REFERENCES `CommercialAgreements` (`AgreementId`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.18 Turnos de Trabajo (Caja)
CREATE TABLE IF NOT EXISTS `WorkShifts` (
    `ShiftId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `UserId` INT NOT NULL,
    `OperatorName` VARCHAR(100) NOT NULL,
    `StartTime` DATETIME(6) NOT NULL,
    `EndTime` DATETIME(6) NULL,
    `BaseAmount` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalCashCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalCardCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalTransferCollected` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `TotalDiscounts` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ExpectedCash` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `ActualCashCounted` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `CashDifference` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `Notes` VARCHAR(500) NULL,
    `Status` VARCHAR(50) NOT NULL DEFAULT 'Open',
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`ShiftId`),
    KEY `IX_WorkShifts_CompanyId` (`CompanyId`),
    KEY `IX_WorkShifts_BranchId` (`BranchId`),
    KEY `IX_WorkShifts_UserId` (`UserId`),
    KEY `IX_WorkShifts_Status` (`Status`),
    CONSTRAINT `FK_WorkShifts_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_WorkShifts_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_WorkShifts_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.19 Mensualidades y Suscripciones
CREATE TABLE IF NOT EXISTS `MonthlySubscriptions` (
    `Id` CHAR(36) NOT NULL,
    `SubscriptionId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `CustomerName` VARCHAR(150) NOT NULL,
    `CustomerDocument` VARCHAR(50) NOT NULL,
    `CustomerPhone` VARCHAR(30) NOT NULL,
    `CustomerEmail` VARCHAR(100) NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `VehicleType` VARCHAR(50) NOT NULL,
    `StartDate` DATETIME(6) NOT NULL,
    `EndDate` DATETIME(6) NOT NULL,
    `MonthlyFee` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `AmountPaid` DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    `PaymentMethod` VARCHAR(50) NULL,
    `Notes` VARCHAR(500) NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_MonthlySubscriptions_SubscriptionId` (`SubscriptionId`),
    KEY `IX_MonthlySubscriptions_PlateNumber` (`PlateNumber`),
    KEY `IX_MonthlySubscriptions_CompanyId` (`CompanyId`),
    KEY `IX_MonthlySubscriptions_BranchId` (`BranchId`),
    CONSTRAINT `FK_MonthlySubscriptions_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_MonthlySubscriptions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.20 Resoluciones de Facturación (DIAN / POS)
CREATE TABLE IF NOT EXISTS `BillingResolutions` (
    `ResolutionId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `BranchId` INT NULL,
    `Name` VARCHAR(150) NOT NULL,
    `DocumentType` VARCHAR(250) NOT NULL,
    `Prefix` VARCHAR(20) NOT NULL,
    `ResolutionNumber` VARCHAR(50) NOT NULL,
    `FromNumber` BIGINT NOT NULL DEFAULT 1,
    `ToNumber` BIGINT NOT NULL DEFAULT 999999,
    `CurrentNumber` BIGINT NOT NULL DEFAULT 0,
    `ValidFrom` DATETIME NOT NULL,
    `ValidTo` DATETIME NOT NULL,
    `TechnicalKey` LONGTEXT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdatedAtUtc` DATETIME NULL,
    PRIMARY KEY (`ResolutionId`),
    KEY `IX_BillingResolutions_CompanyId` (`CompanyId`),
    KEY `IX_BillingResolutions_BranchId` (`BranchId`),
    KEY `IX_BillingResolutions_ResolutionNumber` (`ResolutionNumber`),
    KEY `IX_BillingResolutions_Prefix` (`Prefix`),
    KEY `IX_BillingResolutions_IsActive` (`IsActive`),
    CONSTRAINT `FK_BillingResolutions_Companies_CompanyId` FOREIGN KEY (`CompanyId`) REFERENCES `Companies` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_BillingResolutions_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 1.21 Novedades y Bloqueo de Placas (Incidencias)
CREATE TABLE IF NOT EXISTS `VehicleIncidents` (
    `IncidentId` CHAR(36) NOT NULL,
    `CompanyId` INT NULL,
    `PlateNumber` VARCHAR(20) NOT NULL,
    `BranchId` INT NULL,
    `IncidentType` VARCHAR(100) NOT NULL,
    `IsBlocked` TINYINT(1) NOT NULL DEFAULT 0,
    `IsGlobal` TINYINT(1) NOT NULL DEFAULT 0,
    `Description` LONGTEXT NOT NULL,
    `ReportedBy` VARCHAR(100) NOT NULL,
    `ContactPhone` VARCHAR(30) NULL,
    `Status` VARCHAR(30) NOT NULL DEFAULT 'Activa',
    `ResolvedNotes` LONGTEXT NULL,
    `ResolvedAtUtc` DATETIME NULL,
    `CreatedAtUtc` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
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

-- 1.22 Sedes Asignadas a Novedades (Multi-Sede Relacional)
CREATE TABLE IF NOT EXISTS `VehicleIncidentBranches` (
    `IncidentId` CHAR(36) NOT NULL,
    `BranchId` INT NOT NULL,
    PRIMARY KEY (`IncidentId`, `BranchId`),
    KEY `IX_VehicleIncidentBranches_BranchId` (`BranchId`),
    CONSTRAINT `FK_VehicleIncidentBranches_VehicleIncidents_IncidentId` FOREIGN KEY (`IncidentId`) REFERENCES `VehicleIncidents` (`IncidentId`) ON DELETE CASCADE,
    CONSTRAINT `FK_VehicleIncidentBranches_Branches_BranchId` FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ==================================================================================
-- FASE 2: INSERCIÓN DE DATOS SEMILLA (SEEDS)
-- ==================================================================================

-- ----------------------------------------------------------------------------------
-- 2.0 EMPRESA MATRIZ GLOBAL (Companies) - Plataforma SaaS
-- ----------------------------------------------------------------------------------
INSERT INTO Companies (Id, Name, LegalName, Nit, Email, Phone, Address, City, PlanType, MaxBranches, IsActive, CreatedAt)
VALUES
    (1, 'ParkPoint Global SaaS', 'ParkPoint Solutions S.A.S', '900000000-1', 'soporte@parkpoint.local', '+57 300 000 0000', 'Calle Principal # 1-00', 'Bogotá', 'Enterprise', 99, 1, UTC_TIMESTAMP())
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name,
    Nit = new_row.Nit,
    PlanType = new_row.PlanType,
    MaxBranches = new_row.MaxBranches,
    IsActive = 1;

-- ----------------------------------------------------------------------------------
-- 2.1 TIPOS DE IDENTIFICACIÓN (IdentificationType)
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
-- 2.2 ROLES DE USUARIO (UserRole) - ROL ADMINISTRADOR GLOBAL (CompanyId NULL)
-- ----------------------------------------------------------------------------------
INSERT INTO UserRole (Id, CompanyId, Role, IsActive, CreatedAt, ResponsibleUserId)
VALUES
    (1, NULL, 'Administrador', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Role = new_row.Role,
    CompanyId = NULL,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.3 USUARIO SUPERADMINISTRADOR DE PLATAFORMA (User) - CompanyId NULL
-- Contraseña por defecto: 'Admin2026*' (BCrypt)
-- ----------------------------------------------------------------------------------
INSERT INTO User (
    Id, CompanyId, UserRoleId, IdentificationTypeId, IdentificationNumber, 
    FirstName, MiddleName, FirstSurname, SecondLastName, FullName,
    Username, Password, Email, IsActive, MustChangePassword, CreatedAt
)
VALUES (
    1, NULL, 1, 1, '1000000000', 
    'Administrador', '', 'Principal', '', 'Super Administrador SaaS',
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
-- 2.4 CATÁLOGO COMPLETO DE 16 MÓDULOS (Module)
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
    (16, 'Gestión de Empresas SaaS',            1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    Name = new_row.Name, 
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.5 OPERACIONES BASE (Operation) - 7 Operaciones Estándar
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
-- 2.6 ACCIONES Y SLUGS REALES DEL SISTEMA (Action) - Total: 74 Acciones
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
    (74, 16, 4, 'Eliminar o dar de baja empresa del sistema', 'companies.delete', 1, UTC_TIMESTAMP(), NULL)
AS new_row
ON DUPLICATE KEY UPDATE 
    ModuleId = new_row.ModuleId,
    OperationId = new_row.OperationId,
    Name = new_row.Name,
    Slug = new_row.Slug,
    IsActive = new_row.IsActive;

-- ----------------------------------------------------------------------------------
-- 2.7 MATRIZ DE MÓDULOS POR ROL (UserRoleModule)
-- Asignación del 100% de los 16 Módulos ÚNICAMENTE al Rol 1 (Administrador)
-- ----------------------------------------------------------------------------------
DELETE FROM `UserRoleModule` WHERE `UserRoleId` = 1;

INSERT INTO `UserRoleModule` (`UserRoleId`, `ModulesRoleId`, `IsActive`, `CreatedAt`, `ResponsibleUserId`)
SELECT 1, `Id`, 1, UTC_TIMESTAMP(), 1 FROM `Module`;

-- ----------------------------------------------------------------------------------
-- 2.8 MATRIZ DE PERMISOS: ROL ACCIONES (RoleAction)
-- Asignación del 100% de las 74 Acciones ÚNICAMENTE al Rol 1 (Administrador) - FULL ACCESS
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
    c.Name AS EmpresaAsignada,
    r.Role AS RolAsignado,
    COUNT(DISTINCT urm.ModulesRoleId) AS TotalModulosAsignados,
    COUNT(DISTINCT ra.ActionId) AS TotalAccionesAsignadas
FROM User u
LEFT JOIN Companies c ON u.CompanyId = c.Id
INNER JOIN UserRole r ON u.UserRoleId = r.Id
LEFT JOIN UserRoleModule urm ON urm.UserRoleId = r.Id AND urm.IsActive = 1
LEFT JOIN RoleAction ra ON ra.RoleId = r.Id AND ra.IsActive = 1
WHERE u.Username = 'admin'
GROUP BY u.Id, u.Username, u.FullName, c.Name, r.Role;
