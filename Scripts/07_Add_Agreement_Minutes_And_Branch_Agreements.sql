-- ==============================================================================
-- Script 07: Minutos en Convenios Comerciales y Asignación de Convenios por Sede
-- ==============================================================================

-- 1. Soporte de Minutos Opcionales en Convenios Comerciales
ALTER TABLE `CommercialAgreements`
ADD COLUMN IF NOT EXISTS `MaxMinutesApplicable` INT NULL AFTER `MaxHoursApplicable`;

-- 2. Tabla Relacional de Convenios Comerciales por Sede
CREATE TABLE IF NOT EXISTS `BranchCommercialAgreements` (
    `Id` INT NOT NULL AUTO_INCREMENT,
    `BranchId` INT NOT NULL,
    `AgreementId` CHAR(36) NOT NULL,
    `IsActive` TINYINT(1) NOT NULL DEFAULT 1,
    `CreatedAt` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    `UpdatedAt` DATETIME(6) NULL,
    `ResponsibleUserId` INT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UX_BranchCommercialAgreements_Branch_Agreement` (`BranchId`, `AgreementId`),
    KEY `IX_BranchCommercialAgreements_BranchId` (`BranchId`),
    KEY `IX_BranchCommercialAgreements_AgreementId` (`AgreementId`),
    CONSTRAINT `FK_BranchCommercialAgreements_Branches_BranchId` 
        FOREIGN KEY (`BranchId`) REFERENCES `Branches` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_BranchCommercialAgreements_CommercialAgreements_AgreementId` 
        FOREIGN KEY (`AgreementId`) REFERENCES `CommercialAgreements` (`AgreementId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
