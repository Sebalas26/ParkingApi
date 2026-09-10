/**
 * SCRIPT GENERADOR DE DATOS REALISTAS DE PRODUCCIÓN (> 1 MES DE OPERACIONES)
 * Ecosistema: ParkFlow Multi-Tenant SaaS (ParkingApi / PWA / WPF)
 * Autor: Antigravity AI Assistant
 */

const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const outputFile = path.join(__dirname, '13_Seed_Realistic_Production_Simulation.sql');
const stream = fs.createWriteStream(outputFile, { encoding: 'utf8' });

function write(str) {
  stream.write(str + '\n');
}

function guid() {
  return crypto.randomUUID();
}

console.log('Iniciando generación de datos de simulación realista de producción...');

write(`-- ==================================================================================`);
write(`-- SCRIPT: 13_Seed_Realistic_Production_Simulation.sql`);
write(`-- DESCRIPCIÓN: Inyección masiva y determinista de datos de producción realista (> 35 días)`);
write(`--              10 Empresas SaaS, 44 Sedes, RBAC completo, Tarifas, Resoluciones DIAN,`);
write(`--              Cientos de turnos de caja, miles de tiquetes cobrados y ocupación en vivo.`);
write(`-- MOTOR: MySQL 8.x / MariaDB`);
write(`-- FECHA: ${new Date().toISOString()}`);
write(`-- ==================================================================================\n`);

write(`USE db_acd7d6_parking;\n`);
write(`SET FOREIGN_KEY_CHECKS = 0;`);
write(`SET SQL_SAFE_UPDATES = 0;`);
write(`SET SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO';`);
write(`SET time_zone = '+00:00';\n`);

// 1. Limpieza de datos de simulación previos preservando SuperAdmin
write(`-- 1. LIMPIEZA PREVIA DE TENANTS (PRESERVANDO SUPERADMIN ID=1)`);
write(`DELETE FROM \`PushSubscriptions\`;`);
write(`DELETE FROM \`UserNotificationPreferences\` WHERE \`UserId\` NOT IN (1);`);
write(`DELETE FROM \`UserSessions\`;`);
write(`DELETE FROM \`BranchOperatingHours\`;`);
write(`DELETE FROM \`BranchCommercialAgreements\`;`);
write(`DELETE FROM \`BranchPaymentMethods\`;`);
write(`DELETE FROM \`UserBranches\`;`);
write(`DELETE FROM \`VehicleIncidentBranches\`;`);
write(`DELETE FROM \`TicketDiscounts\`;`);
write(`DELETE FROM \`CommercialAgreements\`;`);
write(`DELETE FROM \`Stores\`;`);
write(`DELETE FROM \`ParkingTickets\`;`);
write(`DELETE FROM \`WorkShifts\`;`);
write(`DELETE FROM \`BillingResolutions\`;`);
write(`DELETE FROM \`VehicleIncidents\`;`);
write(`DELETE FROM \`MonthlySubscriptions\`;`);
write(`DELETE FROM \`VehicleRates\`;`);
write(`DELETE FROM \`Branches\`;`);
write(`DELETE FROM \`Companies\`;`);
write(`DELETE FROM \`Plans\`;`);
write(`DELETE FROM \`Login\` WHERE \`UserId\` NOT IN (1);`);
write(`DELETE FROM \`PasswordResetToken\` WHERE \`UserId\` NOT IN (1);`);
write(`DELETE FROM \`RoleAction\` WHERE \`RoleId\` NOT IN (1);`);
write(`DELETE FROM \`UserRoleModule\` WHERE \`UserRoleId\` NOT IN (1);`);
write(`DELETE FROM \`UserRole\` WHERE \`Id\` NOT IN (1);`);
write(`DELETE FROM \`User\` WHERE \`Id\` NOT IN (1);`);
write(`DELETE FROM \`PaymentMethod\`;\n`);

write(`ALTER TABLE \`Plans\` AUTO_INCREMENT = 1;`);
write(`ALTER TABLE \`Companies\` AUTO_INCREMENT = 1;`);
write(`ALTER TABLE \`Branches\` AUTO_INCREMENT = 1;`);
write(`ALTER TABLE \`UserRole\` AUTO_INCREMENT = 2;`);
write(`ALTER TABLE \`User\` AUTO_INCREMENT = 2;`);
write(`ALTER TABLE \`PaymentMethod\` AUTO_INCREMENT = 1;\n`);

// 2. Planes SaaS
write(`-- 2. PLANES SAAS`);
write(`INSERT INTO \`Plans\` (\`Id\`, \`Name\`, \`Description\`, \`PriceCop\`, \`AnnualPriceCop\`, \`MaxBranches\`, \`MaxUsers\`, \`UsersPerBranch\`, \`HasDesktopAccess\`, \`HasWebAccess\`, \`AllowMultipleSessions\`, \`MaxActiveSessionsPerUser\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
write(`(1, 'Plan Básico Pyme', 'Ideal para parqueaderos de 1 a 2 sedes', 120000.00, 1200000.00, 2, 6, 3, 1, 1, 0, 1, 1, NOW()),`);
write(`(2, 'Plan Profesional Pro', 'Para empresas de 3 a 5 sedes con auditoría avanzada', 280000.00, 2800000.00, 5, 20, 4, 1, 1, 1, 2, 1, NOW()),`);
write(`(3, 'Plan Enterprise Corporativo', 'Ilimitado para grandes cadenas y terminales', 550000.00, 5500000.00, 10, 50, 5, 1, 1, 1, 5, 1, NOW());\n`);

// 3. Medios de Pago Globales
write(`-- 3. MEDIOS DE PAGO GLOBALES`);
write(`INSERT INTO \`PaymentMethod\` (\`Id\`, \`CompanyId\`, \`Name\`, \`Icon\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
write(`(1, NULL, 'Efectivo', '💵', 1, NOW()),`);
write(`(2, NULL, 'Tarjeta Débito / Crédito', '💳', 1, NOW()),`);
write(`(3, NULL, 'Transferencia / QR Nequi - Daviplata', '📱', 1, NOW()),`);
write(`(4, NULL, 'Convenio Comercial / Bono', '🎟️', 1, NOW());\n`);

// Definición de las 10 empresas y sus sedes
const companiesDef = [
  {
    id: 1,
    name: 'Gran Plaza Centro Comercial S.A.S.',
    legalName: 'Inversiones Gran Plaza S.A.S.',
    nit: '900542110-1',
    email: 'administracion@granplazacc.com',
    phone: '6013456789',
    address: 'Av. Carrera 68 # 45-20',
    city: 'Bogotá',
    planId: 3,
    planType: 'Enterprise',
    maxBranches: 10,
    maxUsers: 50,
    prefix: 'granplaza',
    branches: [
      { code: 'SEDE-GP-01', name: 'Plaza Central Piso 1', address: 'Av. Cra 68 # 45-20 Piso 1', city: 'Bogotá', capacity: 300, grace: 15, base: 200000 },
      { code: 'SEDE-GP-02', name: 'Plaza Sótano 1', address: 'Av. Cra 68 # 45-20 Sótano 1', city: 'Bogotá', capacity: 250, grace: 15, base: 200000 },
      { code: 'SEDE-GP-03', name: 'Plaza Bahía Exterior', address: 'Av. Cra 68 # 45-20 Exterior', city: 'Bogotá', capacity: 80, grace: 10, base: 100000 },
      { code: 'SEDE-GP-04', name: 'Gran Plaza Norte Chía', address: 'Km 2 Vía Chía - Cajicá', city: 'Chía', capacity: 150, grace: 15, base: 150000 },
      { code: 'SEDE-GP-05', name: 'Plaza Torre Médica', address: 'Calle 46 # 67B-10', city: 'Bogotá', capacity: 120, grace: 20, base: 150000 }
    ]
  },
  {
    id: 2,
    name: 'Cadena Park & Go Colombia S.A.S.',
    legalName: 'Park and Go Servicios de Estacionamiento S.A.S.',
    nit: '901223450-4',
    email: 'contacto@parkandgo.com.co',
    phone: '6044448899',
    address: 'Carrera 43A # 1Sur-220',
    city: 'Medellín',
    planId: 3,
    planType: 'Enterprise',
    maxBranches: 10,
    maxUsers: 50,
    prefix: 'parkgo',
    branches: [
      { code: 'SEDE-PKG-01', name: 'Park & Go Calle 72', address: 'Calle 72 # 10-34', city: 'Bogotá', capacity: 120, grace: 10, base: 150000 },
      { code: 'SEDE-PKG-02', name: 'Park & Go El Poblado', address: 'Cra 43A # 7-50', city: 'Medellín', capacity: 180, grace: 10, base: 200000 },
      { code: 'SEDE-PKG-03', name: 'Park & Go Laureles', address: 'Circular 4 # 73-12', city: 'Medellín', capacity: 90, grace: 10, base: 100000 },
      { code: 'SEDE-PKG-04', name: 'Park & Go Granada', address: 'Av 9N # 14-20', city: 'Cali', capacity: 110, grace: 10, base: 150000 },
      { code: 'SEDE-PKG-05', name: 'Park & Go Chipichape', address: 'Calle 38N # 6N-45', city: 'Cali', capacity: 140, grace: 15, base: 150000 },
      { code: 'SEDE-PKG-06', name: 'Park & Go Bocagrande', address: 'Cra 2 # 8-30', city: 'Cartagena', capacity: 160, grace: 10, base: 200000 },
      { code: 'SEDE-PKG-07', name: 'Park & Go Cabecera', address: 'Cra 33 # 48-110', city: 'Bucaramanga', capacity: 100, grace: 10, base: 100000 }
    ]
  },
  {
    id: 3,
    name: 'Inversiones Metropolitan Parking Ltda.',
    legalName: 'Inversiones Metropolitan Parking Limitada',
    nit: '830119870-2',
    email: 'gerencia@metropolitanparking.co',
    phone: '6017894561',
    address: 'Calle 100 # 19-61',
    city: 'Bogotá',
    planId: 2,
    planType: 'Pro',
    maxBranches: 5,
    maxUsers: 20,
    prefix: 'metropolitan',
    branches: [
      { code: 'SEDE-MET-01', name: 'Metropolitan Centro Financiero', address: 'Cra 7 # 71-52', city: 'Bogotá', capacity: 200, grace: 15, base: 250000 },
      { code: 'SEDE-MET-02', name: 'Metropolitan Calle 100', address: 'Calle 100 # 19-61', city: 'Bogotá', capacity: 150, grace: 15, base: 200000 },
      { code: 'SEDE-MET-03', name: 'Metropolitan Chapinero', address: 'Cra 13 # 58-30', city: 'Bogotá', capacity: 85, grace: 10, base: 100000 },
      { code: 'SEDE-MET-04', name: 'Metropolitan Salitre', address: 'Calle 26 # 69D-91', city: 'Bogotá', capacity: 130, grace: 15, base: 150000 }
    ]
  },
  {
    id: 4,
    name: 'Clínica & Parking San Rafael S.A.S.',
    legalName: 'Parqueaderos y Soluciones San Rafael S.A.S.',
    nit: '900887330-9',
    email: 'contacto@parkingsanrafael.com',
    phone: '6015551234',
    address: 'Carrera 8 # 17-45 Sur',
    city: 'Bogotá',
    planId: 2,
    planType: 'Pro',
    maxBranches: 5,
    maxUsers: 20,
    prefix: 'sanrafael',
    branches: [
      { code: 'SEDE-SR-01', name: 'San Rafael Sede Principal', address: 'Cra 8 # 17-45 Sur', city: 'Bogotá', capacity: 140, grace: 20, base: 200000 },
      { code: 'SEDE-SR-02', name: 'San Rafael Urgencias 24H', address: 'Calle 17A Sur # 8-10', city: 'Bogotá', capacity: 60, grace: 30, base: 150000 },
      { code: 'SEDE-SR-03', name: 'San Rafael Especialistas', address: 'Cra 9 # 18-02 Sur', city: 'Bogotá', capacity: 90, grace: 15, base: 100000 },
      { code: 'SEDE-SR-04', name: 'San Rafael Centro Diagnóstico', address: 'Calle 19 Sur # 8-60', city: 'Bogotá', capacity: 75, grace: 15, base: 100000 }
    ]
  },
  {
    id: 5,
    name: 'Terminal & Aeropark Service S.A.S.',
    legalName: 'Terminal y Aeropark Service de Colombia S.A.S.',
    nit: '901445660-8',
    email: 'operaciones@aeroparkservice.com',
    phone: '6014147000',
    address: 'Av. El Dorado # 103-09',
    city: 'Bogotá',
    planId: 3,
    planType: 'Enterprise',
    maxBranches: 10,
    maxUsers: 50,
    prefix: 'aeropark',
    branches: [
      { code: 'SEDE-AERO-01', name: 'Aeropark El Dorado T1', address: 'Av El Dorado # 103-09', city: 'Bogotá', capacity: 350, grace: 15, base: 300000 },
      { code: 'SEDE-AERO-02', name: 'Aeropark Puente Aéreo T2', address: 'Av El Dorado # 106-20', city: 'Bogotá', capacity: 180, grace: 15, base: 200000 },
      { code: 'SEDE-AERO-03', name: 'Aeropark Rionegro JMC', address: 'Aeropuerto José María Córdova', city: 'Rionegro', capacity: 280, grace: 15, base: 250000 },
      { code: 'SEDE-AERO-04', name: 'Aeropark Palmira Bonilla', address: 'Aeropuerto Alfonso Bonilla Aragón', city: 'Palmira', capacity: 220, grace: 15, base: 200000 },
      { code: 'SEDE-AERO-05', name: 'Aeropark Barranquilla Cortissoz', address: 'Aeropuerto Ernesto Cortissoz', city: 'Soledad', capacity: 150, grace: 15, base: 150000 },
      { code: 'SEDE-TERM-01', name: 'Terminal Salitre Módulo 1', address: 'Diag 23 # 69-60 Módulo 1', city: 'Bogotá', capacity: 200, grace: 15, base: 200000 },
      { code: 'SEDE-TERM-02', name: 'Terminal Salitre Módulo 2', address: 'Diag 23 # 69-60 Módulo 2', city: 'Bogotá', capacity: 170, grace: 15, base: 200000 }
    ]
  },
  {
    id: 6,
    name: 'Hoteles & Estacionamientos del Valle S.A.',
    legalName: 'Hoteles y Estacionamientos del Valle S.A.',
    nit: '890301220-3',
    email: 'reservas@valleparking.com.co',
    phone: '6028881234',
    address: 'Av. Colombia # 1-40',
    city: 'Cali',
    planId: 2,
    planType: 'Pro',
    maxBranches: 5,
    maxUsers: 20,
    prefix: 'valle',
    branches: [
      { code: 'SEDE-VALLE-01', name: 'Hotel & Park Dann Cali', address: 'Av Colombia # 1-40', city: 'Cali', capacity: 110, grace: 15, base: 150000 },
      { code: 'SEDE-VALLE-02', name: 'Estacionamiento San Antonio', address: 'Cra 10 # 2-30', city: 'Cali', capacity: 80, grace: 10, base: 100000 },
      { code: 'SEDE-VALLE-03', name: 'Estacionamiento El Peñón', address: 'Cra 3 # 1-15 Oeste', city: 'Cali', capacity: 70, grace: 10, base: 100000 },
      { code: 'SEDE-VALLE-04', name: 'Hotel Inter Parking', address: 'Av Colombia # 2-72', city: 'Cali', capacity: 95, grace: 15, base: 150000 },
      { code: 'SEDE-VALLE-05', name: 'Parking Unicentro Cali', address: 'Cra 100 # 5-169', city: 'Cali', capacity: 160, grace: 15, base: 200000 }
    ]
  },
  {
    id: 7,
    name: 'Smart Parking Solutions S.A.S.',
    legalName: 'Smart Parking Solutions S.A.S.',
    nit: '901778990-1',
    email: 'info@smartparkingsolutions.co',
    phone: '6043129000',
    address: 'Cra 48 # 32B Sur-139',
    city: 'Envigado',
    planId: 2,
    planType: 'Pro',
    maxBranches: 5,
    maxUsers: 20,
    prefix: 'smartpark',
    branches: [
      { code: 'SEDE-SMT-01', name: 'SmartPark Envigado Viva', address: 'Cra 48 # 32B Sur-139', city: 'Envigado', capacity: 130, grace: 15, base: 150000 },
      { code: 'SEDE-SMT-02', name: 'SmartPark Sabaneta Mayorca', address: 'Calle 51 Sur # 48-57', city: 'Sabaneta', capacity: 140, grace: 15, base: 150000 },
      { code: 'SEDE-SMT-03', name: 'SmartPark Belén Los Molinos', address: 'Calle 30A # 82A-26', city: 'Medellín', capacity: 90, grace: 10, base: 100000 },
      { code: 'SEDE-SMT-04', name: 'SmartPark Itagüí Central', address: 'Cra 50 # 51-20', city: 'Itagüí', capacity: 80, grace: 10, base: 100000 }
    ]
  },
  {
    id: 8,
    name: 'Parqueaderos El Centro 24 Horas',
    legalName: 'Inversiones El Centro 24H Ltda.',
    nit: '800654321-0',
    email: 'contacto@elcentro24h.com',
    phone: '6012849900',
    address: 'Carrera 7 # 12-45',
    city: 'Bogotá',
    planId: 1,
    planType: 'Basic',
    maxBranches: 2,
    maxUsers: 6,
    prefix: 'elcentro',
    branches: [
      { code: 'SEDE-CENT-01', name: 'El Centro Plaza Bolívar', address: 'Cra 7 # 12-45', city: 'Bogotá', capacity: 75, grace: 10, base: 100000 },
      { code: 'SEDE-CENT-02', name: 'El Centro San Victorino', address: 'Calle 10 # 12-18', city: 'Bogotá', capacity: 60, grace: 10, base: 100000 }
    ]
  },
  {
    id: 9,
    name: 'Logística & Bahías del Norte S.A.S.',
    legalName: 'Logística y Bahías del Norte S.A.S.',
    nit: '900334556-7',
    email: 'operaciones@bahiasdelnorte.com',
    phone: '6016781200',
    address: 'Autopista Norte # 170-40',
    city: 'Bogotá',
    planId: 1,
    planType: 'Basic',
    maxBranches: 2,
    maxUsers: 6,
    prefix: 'logistica',
    branches: [
      { code: 'SEDE-NORTE-01', name: 'Bahía Norte Autopista 170', address: 'Autopista Norte # 170-40', city: 'Bogotá', capacity: 90, grace: 10, base: 100000 },
      { code: 'SEDE-NORTE-02', name: 'Bahía Norte Calle 134', address: 'Calle 134 # 19-30', city: 'Bogotá', capacity: 70, grace: 10, base: 100000 }
    ]
  },
  {
    id: 10,
    name: 'EcoParking Urbano S.A.S.',
    legalName: 'EcoParking Urbano S.A.S.',
    nit: '901556778-5',
    email: 'info@ecoparkingurbano.com',
    phone: '6013108844',
    address: 'Calle 85 # 13-05',
    city: 'Bogotá',
    planId: 2,
    planType: 'Pro',
    maxBranches: 5,
    maxUsers: 20,
    prefix: 'ecoparking',
    branches: [
      { code: 'SEDE-ECO-01', name: 'EcoPark Zona Rosa Zona T', address: 'Calle 85 # 13-05', city: 'Bogotá', capacity: 110, grace: 15, base: 150000 },
      { code: 'SEDE-ECO-02', name: 'EcoPark Parque de la 93', address: 'Cra 11A # 93-42', city: 'Bogotá', capacity: 130, grace: 15, base: 200000 },
      { code: 'SEDE-ECO-03', name: 'EcoPark Usaquén Colonial', address: 'Cra 6 # 119-14', city: 'Bogotá', capacity: 95, grace: 10, base: 150000 },
      { code: 'SEDE-ECO-04', name: 'EcoPark Cedritos 140', address: 'Calle 140 # 12-25', city: 'Bogotá', capacity: 85, grace: 10, base: 100000 }
    ]
  }
];

// 4. Inserción de Empresas
write(`-- 4. INSERCIÓN DE 10 EMPRESAS SAAS`);
write(`INSERT INTO \`Companies\` (\`Id\`, \`Name\`, \`LegalName\`, \`Nit\`, \`Email\`, \`Phone\`, \`Address\`, \`City\`, \`PlanType\`, \`PlanId\`, \`IsCustomPlan\`, \`MaxBranches\`, \`MaxUsers\`, \`HasDesktopAccess\`, \`HasWebAccess\`, \`AllowMultipleSessions\`, \`MaxActiveSessionsPerUser\`, \`AllowMultipleOpenShifts\`, \`MaxOpenShiftsPerUser\`, \`RequireOpenShiftToOperate\`, \`RequireInitialCashAmount\`, \`HasPushNotificationsEnabled\`, \`IsActive\`, \`SubscriptionExpiresAt\`, \`CreatedAt\`) VALUES`);

const compSql = companiesDef.map(c => {
  return `(${c.id}, '${c.name}', '${c.legalName}', '${c.nit}', '${c.email}', '${c.phone}', '${c.address}', '${c.city}', '${c.planType}', ${c.planId}, 0, ${c.maxBranches}, ${c.maxUsers}, 1, 1, 1, 3, 0, 1, 1, 1, 0, 1, DATE_ADD(NOW(), INTERVAL 365 DAY), NOW())`;
}).join(',\n');
write(compSql + ';\n');

// 5. Inserción de Sedes
write(`-- 5. INSERCIÓN DE 44 SEDES`);
write(`INSERT INTO \`Branches\` (\`Id\`, \`CompanyId\`, \`Code\`, \`Name\`, \`Address\`, \`Phone\`, \`City\`, \`TotalCapacity\`, \`PaperWidth\`, \`DefaultInitialCash\`, \`AllowChargeByMinute\`, \`AllowChargeByHour\`, \`AllowChargeByDay\`, \`AllowChargeByNight\`, \`LostTicketFee\`, \`FullDayThresholdMinutes\`, \`FullDayApplicableDays\`, \`NightApplicableDays\`, \`EntryGracePeriodMinutes\`, \`ExitGracePeriodMinutes\`, \`IsActive\`, \`CreatedAt\`) VALUES`);

let branchGlobalId = 1;
const allBranches = [];

companiesDef.forEach(c => {
  c.branches.forEach(b => {
    b.globalId = branchGlobalId++;
    b.companyId = c.id;
    allBranches.push(b);
  });
});

const branchSql = allBranches.map(b => {
  return `(${b.globalId}, ${b.companyId}, '${b.code}', '${b.name}', '${b.address}', '6013000000', '${b.city}', ${b.capacity}, 80, ${b.base.toFixed(2)}, 1, 1, 1, 1, 20000.00, 180, '1,2,3,4,5,6,0', '1,2,3,4,5,6,0', ${b.grace}, 15, 1, NOW())`;
}).join(',\n');
write(branchSql + ';\n');

// 6. Configuración de Sedes (Medios de Pago, Horarios)
write(`-- 6. MEDIOS DE PAGO Y HORARIOS POR SEDE`);
write(`INSERT INTO \`BranchPaymentMethods\` (\`BranchId\`, \`PaymentMethodId\`, \`RequiresCashTender\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
const bpmSql = [];
allBranches.forEach(b => {
  bpmSql.push(`(${b.globalId}, 1, 1, 1, NOW())`); // Efectivo
  bpmSql.push(`(${b.globalId}, 2, 0, 1, NOW())`); // Tarjetas
  bpmSql.push(`(${b.globalId}, 3, 0, 1, NOW())`); // Transferencias
});
write(bpmSql.join(',\n') + ';\n');

write(`INSERT INTO \`BranchOperatingHours\` (\`BranchId\`, \`DayOfWeek\`, \`IsOpen\`, \`OpeningTime\`, \`ClosingTime\`, \`BufferMinutesBefore\`, \`BufferMinutesAfter\`) VALUES`);
const bohSql = [];
allBranches.forEach(b => {
  for (let d = 0; d < 7; d++) {
    bohSql.push(`(${b.globalId}, ${d}, 1, '06:00:00', '22:00:00', 30, 30)`);
  }
});
write(bohSql.join(',\n') + ';\n');

// 7. Tarifas Vehiculares por Sede
write(`-- 7. TARIFAS VEHICULARES POR SEDE (Car=0, Motorcycle=1, Suv=5, Truck=2)`);
write(`INSERT INTO \`VehicleRates\` (\`RateId\`, \`BranchId\`, \`CompanyId\`, \`VehicleType\`, \`DayOfWeek\`, \`DisplayName\`, \`HourRate\`, \`MinuteRate\`, \`FullDayRate\`, \`NightRate\`, \`GracePeriodMinutes\`, \`IconKey\`, \`IsActive\`, \`CreatedAtUtc\`) VALUES`);
const vrSql = [];
allBranches.forEach(b => {
  // Car
  vrSql.push(`('${guid()}', ${b.globalId}, ${b.companyId}, 0, NULL, 'Automóvil', 4500.00, 75.00, 35000.00, 18000.00, ${b.grace}, 'IconCar', 1, NOW())`);
  // Motorcycle
  vrSql.push(`('${guid()}', ${b.globalId}, ${b.companyId}, 1, NULL, 'Motocicleta', 2500.00, 42.00, 18000.00, 10000.00, ${b.grace}, 'IconMotorcycle', 1, NOW())`);
  // Suv
  vrSql.push(`('${guid()}', ${b.globalId}, ${b.companyId}, 5, NULL, 'Camioneta / SUV', 5500.00, 92.00, 42000.00, 22000.00, ${b.grace}, 'IconSuv', 1, NOW())`);
  // Truck
  vrSql.push(`('${guid()}', ${b.globalId}, ${b.companyId}, 2, NULL, 'Vehículo Pesado', 8500.00, 142.00, 65000.00, 35000.00, ${b.grace}, 'IconTruck', 1, NOW())`);
});
write(vrSql.join(',\n') + ';\n');

// 8. Resoluciones DIAN por Sede
write(`-- 8. RESOLUCIONES DIAN POR SEDE`);
write(`INSERT INTO \`BillingResolutions\` (\`ResolutionId\`, \`CompanyId\`, \`BranchId\`, \`Name\`, \`DocumentType\`, \`Prefix\`, \`ResolutionNumber\`, \`FromNumber\`, \`ToNumber\`, \`CurrentNumber\`, \`ValidFrom\`, \`ValidTo\`, \`TechnicalKey\`, \`IsActive\`, \`CreatedAtUtc\`) VALUES`);
const brSql = [];
const branchResolutions = {}; // branchId -> { posId, posName, feId, feName }
allBranches.forEach(b => {
  const feId = guid();
  const posId = guid();
  const tqId = guid();
  branchResolutions[b.globalId] = {
    posId,
    posName: `POS-${b.code}`,
    feId,
    feName: `FE-${b.code}`,
    tqId,
    tqName: `TQ-${b.code}`
  };

  brSql.push(`('${posId}', ${b.companyId}, ${b.globalId}, 'Factura Electrónica POS', 'POS', 'POS', '18764000100${b.globalId}', 1, 100000, 4500, '2026-01-01', '2027-12-31', 'tk_live_pos_key_2026', 1, NOW())`);
  brSql.push(`('${feId}', ${b.companyId}, ${b.globalId}, 'Factura Electrónica de Venta', 'FEV', 'FE', '18765000200${b.globalId}', 1, 50000, 1200, '2026-01-01', '2027-12-31', 'tk_live_fe_key_2026', 1, NOW())`);
  brSql.push(`('${tqId}', ${b.companyId}, ${b.globalId}, 'Tiquete de Parqueadero', 'TIQ', 'TQ', '18766000300${b.globalId}', 1, 500000, 8900, '2026-01-01', '2027-12-31', NULL, 1, NOW())`);
});
write(brSql.join(',\n') + ';\n');

// 9. Roles RBAC y Permisos por Empresa
write(`-- 9. ROLES RBAC POR EMPRESA`);
write(`INSERT INTO \`UserRole\` (\`Id\`, \`CompanyId\`, \`BranchId\`, \`Role\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
let roleIdCounter = 2;
const companyRoles = {}; // companyId -> { adminRoleId, superRoleId, operatorRoleId }

companiesDef.forEach(c => {
  const aId = roleIdCounter++;
  const sId = roleIdCounter++;
  const oId = roleIdCounter++;
  companyRoles[c.id] = { adminRoleId: aId, superRoleId: sId, operatorRoleId: oId };
  write(`(${aId}, ${c.id}, NULL, 'Administrador Empresa', 1, NOW()),`);
  write(`(${sId}, ${c.id}, NULL, 'Supervisor de Patio', 1, NOW()),`);
  write(`(${oId}, ${c.id}, NULL, 'Operador de Garita / Caja', 1, NOW())${c.id === 10 ? ';' : ','}`);
});
write('');

// Módulos para los roles
write(`-- Vinculación de Módulos a Roles (1 a 18)`);
write(`INSERT INTO \`UserRoleModule\` (\`UserRoleId\`, \`ModulesRoleId\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
const urmSql = [];
companiesDef.forEach(c => {
  const roles = companyRoles[c.id];
  for (let m = 1; m <= 18; m++) {
    urmSql.push(`(${roles.adminRoleId}, ${m}, 1, NOW())`);
  }
  [1, 2, 3, 4, 5, 6, 8, 9, 13, 14, 18].forEach(m => {
    urmSql.push(`(${roles.superRoleId}, ${m}, 1, NOW())`);
  });
  [1, 2, 3, 4, 5, 6].forEach(m => {
    urmSql.push(`(${roles.operatorRoleId}, ${m}, 1, NOW())`);
  });
});
write(urmSql.join(',\n') + ';\n');

// Acciones a Roles (Asignar todas las acciones existentes al admin de la empresa, y operativas al cajero)
write(`-- Vinculación de Acciones a Roles`);
write(`INSERT INTO \`RoleAction\` (\`RoleId\`, \`ActionId\`, \`IsActive\`, \`CreatedAt\`)`);
write(`SELECT r.Id, a.Id, 1, NOW() FROM \`UserRole\` r CROSS JOIN \`Action\` a WHERE r.Role = 'Administrador Empresa';`);

write(`INSERT INTO \`RoleAction\` (\`RoleId\`, \`ActionId\`, \`IsActive\`, \`CreatedAt\`)`);
write(`SELECT r.Id, a.Id, 1, NOW() FROM \`UserRole\` r CROSS JOIN \`Action\` a WHERE r.Role = 'Operador de Garita / Caja' AND (a.Slug LIKE 'wpf.%' OR a.Slug LIKE 'vehicles.%' OR a.Slug LIKE 'shifts.%');\n`);

// 10. Usuarios por Empresa (Admins, Supervisores y Operadores de Garita)
write(`-- 10. USUARIOS POR EMPRESA (Credenciales para pruebas)`);
// Hash BCrypt canónico para 'admin123'
const passwordHash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy';

write(`INSERT INTO \`User\` (\`Id\`, \`CompanyId\`, \`UserRoleId\`, \`IdentificationTypeId\`, \`IdentificationNumber\`, \`FirstName\`, \`MiddleName\`, \`FirstSurname\`, \`SecondLastName\`, \`FullName\`, \`Username\`, \`Password\`, \`Email\`, \`IsActive\`, \`MustChangePassword\`, \`CreatedAt\`) VALUES`);

let userIdCounter = 2;
const companyUsers = {}; // companyId -> { adminUser, superUser, operators: [] }
const allUserBranches = [];
const userSql = [];

companiesDef.forEach(c => {
  const roles = companyRoles[c.id];
  companyUsers[c.id] = { operators: [] };

  // Admin
  const adminId = userIdCounter++;
  const adminUser = {
    id: adminId,
    username: `admin.${c.prefix}`,
    name: `Administrador ${c.name.split(' ')[0]}`,
    companyId: c.id
  };
  companyUsers[c.id].adminUser = adminUser;
  userSql.push(`(${adminId}, ${c.id}, ${roles.adminRoleId}, 1, '100${adminId}000', 'Admin', '', '${c.name.split(' ')[0]}', '', '${adminUser.name}', '${adminUser.username}', '${passwordHash}', 'admin@${c.prefix}.com', 1, 0, NOW())`);

  // Supervisor
  const superId = userIdCounter++;
  userSql.push(`(${superId}, ${c.id}, ${roles.superRoleId}, 1, '100${superId}000', 'Supervisor', '', '${c.name.split(' ')[0]}', '', 'Supervisor ${c.name.split(' ')[0]}', 'super.${c.prefix}', '${passwordHash}', 'supervisor@${c.prefix}.com', 1, 0, NOW())`);

  // Operadores de cada sede
  c.branches.forEach((b, idx) => {
    const opId = userIdCounter++;
    const opUser = {
      id: opId,
      username: `cajero.${b.code.toLowerCase().replace('-', '')}`,
      name: `Cajero ${b.code}`,
      companyId: c.id,
      branchId: b.globalId
    };
    companyUsers[c.id].operators.push(opUser);
    userSql.push(`(${opId}, ${c.id}, ${roles.operatorRoleId}, 1, '100${opId}000', 'Cajero', '', '${b.code}', '', '${opUser.name}', '${opUser.username}', '${passwordHash}', '${opUser.username}@${c.prefix}.com', 1, 0, NOW())`);

    allUserBranches.push(`(${opId}, ${b.globalId}, 1, 1, NOW())`);
    allUserBranches.push(`(${adminId}, ${b.globalId}, ${idx === 0 ? 1 : 0}, 1, NOW())`); // Admin tiene acceso a todas las sedes de su empresa
    allUserBranches.push(`(${superId}, ${b.globalId}, ${idx === 0 ? 1 : 0}, 1, NOW())`); // Supervisor también
  });
});

write(userSql.join(',\n') + ';\n');

write(`-- Asignaciones de Usuarios a Sedes (UserBranches)`);
write(`INSERT INTO \`UserBranches\` (\`UserId\`, \`BranchId\`, \`IsDefault\`, \`IsActive\`, \`CreatedAt\`) VALUES`);
write(allUserBranches.join(',\n') + ';\n');

// 11. Mensualidades y Abonados
write(`-- 11. MENSUALIDADES Y ABONADOS (4 por sede)`);
write(`INSERT INTO \`MonthlySubscriptions\` (\`SubscriptionId\`, \`CompanyId\`, \`BranchId\`, \`CustomerName\`, \`CustomerDocument\`, \`CustomerPhone\`, \`CustomerEmail\`, \`PlateNumber\`, \`VehicleType\`, \`StartDateUtc\`, \`EndDateUtc\`, \`MonthlyFee\`, \`AmountPaid\`, \`PaymentMethod\`, \`IsActive\`, \`CreatedAt\`) VALUES`);

const subsSql = [];
const customerNames = ['Carlos Alberto Restrepo', 'María Fernanda Gómez', 'Andrés Felipe Morales', 'Diana Carolina Pardo', 'Javier Eduardo Castillo', 'Laura Marcela Ortiz'];
allBranches.forEach((b, idx) => {
  for (let s = 0; s < 4; s++) {
    const cName = customerNames[(idx + s) % customerNames.length];
    const plate = `MS${String.fromCharCode(65 + (s % 26))}${idx % 10}${s + 1}${idx % 9}`;
    const fee = s % 2 === 0 ? 180000.00 : 90000.00;
    const isAct = s < 3 ? 1 : 0;
    const endDays = isAct ? 25 : -5;
    subsSql.push(`('${guid()}', ${b.companyId}, ${b.globalId}, '${cName}', '102030${b.globalId}${s}', '3109876543', 'cliente${s}@correo.com', '${plate}', ${s % 2}, DATE_SUB(NOW(), INTERVAL 30 DAY), DATE_ADD(NOW(), INTERVAL ${endDays} DAY), ${fee}, ${fee}, 1, ${isAct}, NOW())`);
  }
});
write(subsSql.join(',\n') + ';\n');

// 12. Simulación de Operaciones Históricas: Turnos y Tiquetes (Últimos 35 Días)
write(`-- 12. GENERACIÓN DE TURNOS OPERATIVOS HISTÓRICOS Y TIQUETES (> 35 DÍAS)`);

const DAYS_HISTORY = 35;
const allShifts = [];
const allTickets = [];

const vehicleTypes = [
  { type: 0, name: 'Automóvil', rate: 4500, ratio: 0.65 },
  { type: 1, name: 'Motocicleta', rate: 2500, ratio: 0.25 },
  { type: 5, name: 'Camioneta / SUV', rate: 5500, ratio: 0.10 }
];

const platesPoolLetters = ['ABC', 'DFG', 'HJK', 'LMN', 'PQR', 'STV', 'WXY', 'KLT', 'MWZ', 'JGH'];

console.log(`Generando simulación histórica de ${DAYS_HISTORY} días para las 44 sedes...`);

allBranches.forEach(b => {
  const operators = companyUsers[b.companyId].operators.filter(op => op.branchId === b.globalId);
  const operator = operators[0] || companyUsers[b.companyId].adminUser;
  const res = branchResolutions[b.globalId];

  let ticketCounter = 1;

  for (let day = DAYS_HISTORY; day >= 0; day--) {
    const isToday = (day === 0);

    // Turno 1: Mañana (06:00 - 14:00)
    const shift1Id = guid();
    const shift1Status = isToday ? 0 : 1; // Si es hoy, queda ABIERTO
    const shift1Start = `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '06:00:00' HOUR_SECOND)`;
    const shift1End = isToday ? 'NULL' : `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '14:00:00' HOUR_SECOND)`;

    // Generar tiquetes para el turno 1
    // Si es hoy: generar vehículos activos en patio (ocupación en vivo) y vehículos que ya salieron en la mañana
    const numTickets = isToday ? Math.floor(b.capacity * 0.45) : (12 + (b.globalId % 15));
    let t1Cash = 0, t1Card = 0, t1Transfer = 0;
    let t1Processed = 0;

    for (let t = 0; t < numTickets; t++) {
      const vChoice = vehicleTypes[t % vehicleTypes.length];
      const pLetters = platesPoolLetters[(b.globalId + t) % platesPoolLetters.length];
      const pNum = String(100 + ((b.globalId * 17 + t * 23) % 899));
      const plate = vChoice.type === 1 ? `${pLetters}${pNum.substring(0, 2)}${String.fromCharCode(65 + (t % 26))}` : `${pLetters}${pNum}`;

      const entryHour = 6 + Math.floor((t / numTickets) * 7);
      const entryMin = (t * 7) % 60;
      const durationMin = 30 + ((t * 29) % 240); // Entre 30 min y 4.5 horas

      const isCurrentActive = isToday && (t >= Math.floor(numTickets * 0.4)); // 60% de los de hoy están adentro en patio

      const entryTimeSql = `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '${String(entryHour).padStart(2, '0')}:${String(entryMin).padStart(2, '0')}:00' HOUR_SECOND)`;
      const exitTimeSql = isCurrentActive ? 'NULL' : `DATE_ADD(${entryTimeSql}, INTERVAL ${durationMin} MINUTE)`;

      // Cálculo de tarifa
      const hours = Math.max(1, Math.ceil(durationMin / 60));
      const gross = isCurrentActive ? 0 : hours * vChoice.rate;
      const disc = (t % 7 === 0 && !isCurrentActive) ? (vChoice.rate * 0.5) : 0;
      const net = Math.max(0, gross - disc);

      const payMethodChoice = t % 10;
      let pmEnum = 0, pmId = 1; // Efectivo
      if (payMethodChoice >= 6 && payMethodChoice <= 8) {
        pmEnum = 1; pmId = 2; // Tarjeta
      } else if (payMethodChoice === 9) {
        pmEnum = 3; pmId = 3; // Transferencia
      }

      if (!isCurrentActive) {
        t1Processed++;
        if (pmId === 1) t1Cash += net;
        else if (pmId === 2) t1Card += net;
        else if (pmId === 3) t1Transfer += net;
      }

      const tktId = guid();
      const tktNum = `TK-${b.code}-${String(ticketCounter++).padStart(6, '0')}`;
      const invNum = isCurrentActive ? 'NULL' : `'${res.posName}-${String(ticketCounter).padStart(5, '0')}'`;

      allTickets.push({
        ticketId: tktId,
        companyId: b.companyId,
        branchId: b.globalId,
        ticketNumber: tktNum,
        plateNumber: plate,
        vehicleType: vChoice.type,
        entryTimeSql,
        exitTimeSql,
        durationMin: isCurrentActive ? 0 : durationMin,
        hourlyRate: vChoice.rate,
        grossAmount: gross,
        discountAmount: disc,
        netAmount: net,
        amountPaid: net,
        changeGiven: 0,
        paymentMethod: isCurrentActive ? 'NULL' : pmEnum,
        paymentMethodId: isCurrentActive ? 'NULL' : pmId,
        status: isCurrentActive ? 0 : 1, // 0=Active, 1=Completed
        operatorName: operator.name,
        resolutionId: isCurrentActive ? 'NULL' : `'${res.posId}'`,
        resolutionName: isCurrentActive ? 'NULL' : `'${res.posName}'`,
        invoiceNumber: invNum,
        createdAtSql: entryTimeSql
      });
    }

    // Simulación de sobrantes / faltantes leves para probar métricas de arqueo
    let t1Diff = 0;
    if (!isToday) {
      if (day % 13 === 0) t1Diff = 2000;
      else if (day % 19 === 0) t1Diff = -3000;
    }
    const t1Expected = b.base + t1Cash;
    const t1Actual = isToday ? 0 : (t1Expected + t1Diff);
    allShifts.push({
      shiftId: shift1Id,
      companyId: b.companyId,
      branchId: b.globalId,
      userId: operator.id,
      operatorName: operator.name,
      cashRegisterName: 'Caja Garita Principal',
      startTimeSql: shift1Start,
      endTimeSql: shift1End,
      baseAmount: b.base,
      cashCollected: t1Cash,
      cardCollected: t1Card,
      transferCollected: t1Transfer,
      discounts: 0,
      expectedCash: t1Expected,
      actualCash: t1Actual,
      difference: t1Diff,
      processed: t1Processed,
      entered: numTickets,
      status: shift1Status,
      createdAtSql: shift1Start,
      closedAtSql: shift1End
    });

    // Turno 2: Tarde (14:00 - 22:00) para días históricos
    if (!isToday) {
      const shift2Id = guid();
      const shift2Start = `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '14:00:00' HOUR_SECOND)`;
      const shift2End = `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '22:00:00' HOUR_SECOND)`;

      const numTicketsT2 = (15 + (b.globalId % 18));
      let t2Cash = 0, t2Card = 0, t2Transfer = 0;
      let t2Processed = 0;

      for (let t = 0; t < numTicketsT2; t++) {
        const vChoice = vehicleTypes[(t + 1) % vehicleTypes.length];
        const pLetters = platesPoolLetters[(b.globalId + t + 3) % platesPoolLetters.length];
        const pNum = String(100 + ((b.globalId * 19 + t * 31) % 899));
        const plate = vChoice.type === 1 ? `${pLetters}${pNum.substring(0, 2)}${String.fromCharCode(65 + (t % 26))}` : `${pLetters}${pNum}`;

        const entryHour = 14 + Math.floor((t / numTicketsT2) * 7);
        const entryMin = (t * 11) % 60;
        const durationMin = 25 + ((t * 37) % 210);

        const entryTimeSql = `DATE_ADD(DATE_SUB(CURDATE(), INTERVAL ${day} DAY), INTERVAL '${String(entryHour).padStart(2, '0')}:${String(entryMin).padStart(2, '0')}:00' HOUR_SECOND)`;
        const exitTimeSql = `DATE_ADD(${entryTimeSql}, INTERVAL ${durationMin} MINUTE)`;

        const hours = Math.max(1, Math.ceil(durationMin / 60));
        const gross = hours * vChoice.rate;
        const disc = (t % 8 === 0) ? (vChoice.rate * 0.5) : 0;
        const net = Math.max(0, gross - disc);

        const payMethodChoice = (t + 2) % 10;
        let pmEnum = 0, pmId = 1;
        if (payMethodChoice >= 6 && payMethodChoice <= 8) {
          pmEnum = 1; pmId = 2;
        } else if (payMethodChoice === 9) {
          pmEnum = 3; pmId = 3;
        }

        t2Processed++;
        if (pmId === 1) t2Cash += net;
        else if (pmId === 2) t2Card += net;
        else if (pmId === 3) t2Transfer += net;

        const tktId = guid();
        const tktNum = `TK-${b.code}-${String(ticketCounter++).padStart(6, '0')}`;
        const invNum = `'${res.posName}-${String(ticketCounter).padStart(5, '0')}'`;

        allTickets.push({
          ticketId: tktId,
          companyId: b.companyId,
          branchId: b.globalId,
          ticketNumber: tktNum,
          plateNumber: plate,
          vehicleType: vChoice.type,
          entryTimeSql,
          exitTimeSql,
          durationMin,
          hourlyRate: vChoice.rate,
          grossAmount: gross,
          discountAmount: disc,
          netAmount: net,
          amountPaid: net,
          changeGiven: 0,
          paymentMethod: pmEnum,
          paymentMethodId: pmId,
          status: 1,
          operatorName: operator.name,
          resolutionId: `'${res.posId}'`,
          resolutionName: `'${res.posName}'`,
          invoiceNumber: invNum,
          createdAtSql: entryTimeSql
        });
      }

      let t2Diff = 0;
      if (day % 17 === 0) t2Diff = 5000;
      else if (day % 23 === 0) t2Diff = -4000;
      const t2Expected = b.base + t2Cash;
      const t2Actual = t2Expected + t2Diff;
      allShifts.push({
        shiftId: shift2Id,
        companyId: b.companyId,
        branchId: b.globalId,
        userId: operator.id,
        operatorName: operator.name,
        cashRegisterName: 'Caja Garita Principal',
        startTimeSql: shift2Start,
        endTimeSql: shift2End,
        baseAmount: b.base,
        cashCollected: t2Cash,
        cardCollected: t2Card,
        transferCollected: t2Transfer,
        discounts: 0,
        expectedCash: t2Expected,
        actualCash: t2Actual,
        difference: t2Diff,
        processed: t2Processed,
        entered: numTicketsT2,
        status: 1,
        createdAtSql: shift2Start,
        closedAtSql: shift2End
      });
    }
  }
});

console.log(`Total de turnos generados: ${allShifts.length}`);
console.log(`Total de tiquetes generados: ${allTickets.length}`);

// Escribir Turnos en bloques de 100
write(`-- Insertando ${allShifts.length} Turnos Operativos`);
const SHIFT_CHUNK_SIZE = 100;
for (let i = 0; i < allShifts.length; i += SHIFT_CHUNK_SIZE) {
  const chunk = allShifts.slice(i, i + SHIFT_CHUNK_SIZE);
  write(`INSERT INTO \`WorkShifts\` (\`ShiftId\`, \`CompanyId\`, \`BranchId\`, \`UserId\`, \`OperatorName\`, \`CashRegisterName\`, \`StartTimeUtc\`, \`EndTimeUtc\`, \`BaseAmount\`, \`TotalCashCollected\`, \`TotalCardCollected\`, \`TotalTransferCollected\`, \`TotalDiscounts\`, \`ExpectedCash\`, \`ActualCashCounted\`, \`CashDifference\`, \`TotalTicketsProcessed\`, \`TotalVehiclesEntered\`, \`Status\`, \`CreatedAtUtc\`, \`ClosedAtUtc\`) VALUES`);
  const values = chunk.map(s => {
    return `('${s.shiftId}', ${s.companyId}, ${s.branchId}, ${s.userId}, '${s.operatorName}', '${s.cashRegisterName}', ${s.startTimeSql}, ${s.endTimeSql}, ${s.baseAmount.toFixed(2)}, ${s.cashCollected.toFixed(2)}, ${s.cardCollected.toFixed(2)}, ${s.transferCollected.toFixed(2)}, ${s.discounts.toFixed(2)}, ${s.expectedCash.toFixed(2)}, ${s.actualCash.toFixed(2)}, ${s.difference.toFixed(2)}, ${s.processed}, ${s.entered}, ${s.status}, ${s.createdAtSql}, ${s.closedAtSql})`;
  }).join(',\n');
  write(values + ';\n');
}

// Escribir Tiquetes en bloques de 200
write(`-- Insertando ${allTickets.length} Tiquetes de Parqueadero`);
const TICKET_CHUNK_SIZE = 200;
for (let i = 0; i < allTickets.length; i += TICKET_CHUNK_SIZE) {
  const chunk = allTickets.slice(i, i + TICKET_CHUNK_SIZE);
  write(`INSERT INTO \`ParkingTickets\` (\`TicketId\`, \`CompanyId\`, \`BranchId\`, \`TicketNumber\`, \`PlateNumber\`, \`VehicleType\`, \`EntryTimeUtc\`, \`ExitTimeUtc\`, \`TotalDurationMinutes\`, \`HourlyRate\`, \`GrossAmount\`, \`DiscountAmount\`, \`NetAmount\`, \`AmountPaid\`, \`ChangeGiven\`, \`IsLostTicket\`, \`LostTicketFee\`, \`PaymentMethod\`, \`PaymentMethodId\`, \`Status\`, \`OperatorName\`, \`IsSynchronized\`, \`ResolutionId\`, \`ResolutionName\`, \`InvoiceNumber\`, \`IsElectronicInvoice\`, \`CreatedAtUtc\`) VALUES`);
  const values = chunk.map(t => {
    return `('${t.ticketId}', ${t.companyId}, ${t.branchId}, '${t.ticketNumber}', '${t.plateNumber}', ${t.vehicleType}, ${t.entryTimeSql}, ${t.exitTimeSql}, ${t.durationMin}, ${t.hourlyRate.toFixed(2)}, ${t.grossAmount.toFixed(2)}, ${t.discountAmount.toFixed(2)}, ${t.netAmount.toFixed(2)}, ${t.amountPaid.toFixed(2)}, ${t.changeGiven.toFixed(2)}, 0, 0.00, ${t.paymentMethod}, ${t.paymentMethodId}, ${t.status}, '${t.operatorName}', 1, ${t.resolutionId}, ${t.resolutionName}, ${t.invoiceNumber}, 0, ${t.createdAtSql})`;
  }).join(',\n');
  write(values + ';\n');
}

// 13. Novedades e Incidentes
write(`-- 13. NOVEDADES E INCIDENCIAS VEHICULARES (2 por sede)`);
write(`INSERT INTO \`VehicleIncidents\` (\`IncidentId\`, \`CompanyId\`, \`BranchId\`, \`PlateNumber\`, \`IncidentType\`, \`IsBlocked\`, \`IsGlobal\`, \`Description\`, \`ReportedBy\`, \`ContactPhone\`, \`Status\`, \`CreatedAtUtc\`) VALUES`);
const incSql = [];
allBranches.forEach((b, idx) => {
  incSql.push(`('${guid()}', ${b.companyId}, ${b.globalId}, 'INC${idx}01', 'Rayón previo en puerta', 0, 0, 'Vehículo ingresa con rayón lateral en puerta izquierda registrado al ingresar.', 'Cajero Garita', '3001234567', 'Activa', DATE_SUB(NOW(), INTERVAL ${idx % 20} DAY))`);
  incSql.push(`('${guid()}', ${b.companyId}, ${b.globalId}, 'INC${idx}02', 'Golpe en parachoque', 0, 0, 'Descuadre y golpe en bomper trasero notificado por el conductor.', 'Supervisor Patio', '3001234568', 'Resuelta', DATE_SUB(NOW(), INTERVAL ${(idx % 15) + 5} DAY))`);
});
write(incSql.join(',\n') + ';\n');

// Reactivar claves foráneas y modo seguro
write(`SET SQL_SAFE_UPDATES = 1;`);
write(`SET FOREIGN_KEY_CHECKS = 1;\n`);

write(`-- ==================================================================================`);
write(`-- RESUMEN DE LA SIMULACIÓN DE DATOS INYECTADA`);
write(`-- ==================================================================================`);
write(`SELECT `);
write(`    (SELECT COUNT(*) FROM \`Companies\`) AS TotalEmpresas,`);
write(`    (SELECT COUNT(*) FROM \`Branches\`) AS TotalSedes,`);
write(`    (SELECT COUNT(*) FROM \`User\` WHERE \`Id\` > 1) AS TotalUsuariosOperativos,`);
write(`    (SELECT COUNT(*) FROM \`WorkShifts\` WHERE \`Status\` = 1) AS TurnosCerradosHistoricos,`);
write(`    (SELECT COUNT(*) FROM \`WorkShifts\` WHERE \`Status\` = 0) AS TurnosAbiertosActivosHoy,`);
write(`    (SELECT COUNT(*) FROM \`ParkingTickets\` WHERE \`Status\` = 1) AS TiquetesFacturadosHistoricos,`);
write(`    (SELECT COUNT(*) FROM \`ParkingTickets\` WHERE \`Status\` = 0) AS VehiculosActivosEnPatio,`);
write(`    (SELECT COUNT(*) FROM \`MonthlySubscriptions\`) AS TotalMensualidades,`);
write(`    (SELECT COUNT(*) FROM \`VehicleIncidents\`) AS TotalNovedades,`);
write(`    'Simulación de producción generada exitosamente. El sistema cuenta con > 35 días de datos reales.' AS Mensaje;\n`);

stream.end(() => {
  console.log(`\n¡Éxito! Archivo SQL generado correctamente en: ${outputFile}`);
});
