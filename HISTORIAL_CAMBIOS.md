# 📜 HISTORIAL DE CAMBIOS Y CONTEXTO TÉCNICO MULTI-PC

Este archivo registra de forma acumulativa y cronológica todos los requerimientos, decisiones arquitectónicas, cambios en DTOs/entidades y estado de compilación del ecosistema Parking.

## 📌 Entrada: Restricción Estricta de Módulos 'Empresas SaaS' y 'Sedes' para Super Administrador
- **`💬 Prompt Original del Usuario`**:
  > *"estos dos modulos no deberían aparecerle a nadie que no sea el superadministrador del todo el software por que mira que estaba configurando los permisos para un rol de la compañia desde el usuario administrador de la compañia, pero enserio no es posible imaginate darle el controla otra compañia de mi software eso es un error fatal si me explico ? analiza eso y dame el plan"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Aislamiento de Módulos SuperAdmin (`ModuleRepository.cs` & `ActionRepository.cs`)**:
    - Se restringieron los módulos 16 (`Gestión de Empresas SaaS`) y 7 (`Gestión de Sedes y Parqueaderos`) y todas sus acciones asociadas (`companies.*` y `branches.*`) en `GetModules`, `GetActions` y `GetActionsActive` para que no se retornen a usuarios que no tengan `_currentUser.IsSuperAdmin == true`.
  - **Sanitización de Permisos Asignados (`RoleActionRepository.cs`)**:
    - En `AssignRolePermissionsAsync`, si el usuario autenticado no es SuperAdmin, se filtran y descartan automáticamente las acciones de `ModuleId == 16` y `ModuleId == 7` antes de persistirlas.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Infrastructure/Data/Repositories/Modules/ModuleRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Actions/ActionRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/RoleActions/RoleActionRepository.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Columna Logo en Companies y Soporte de Imagen Base64
- **`💬 Prompt Original del Usuario`**:
  > *"Necesito que agregues una columna en la tabla companies que sea logo eso se guardara en base 64 pero que sea opcional, compañia que no tenga el logo usuara el de nosotros el de nuestro software vale el que si lo tenga configurado se usara para la impresión eso lo vamos mirando después pero entonces para que lo tengas muy presente. lo que toca hacer, eso implica modificar la modal de creación de la compañia para que permita opcional cargar la imagen pero no quiero el boton convecional algo mas pro unico que se le pueda dar click y poder ver la imagen pero ten presente que no dañe nada del responsi completo que se tienen en el sistema, e iogual al editar la compañias ya creadas listo . analiza y dame el plan completo."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Mapeo de Columna `Logo` en EF Core**:
    - Se habilitó la propiedad `public string? Logo { get; set; }` en `Company.cs`.
    - En `EntityConfigurations.cs`, se configuró `builder.Property(c => c.Logo).HasColumnType("longtext").IsRequired(false);`.
  - **DTOs y Lógica de Negocio**:
    - Se incluyó `Logo` opcional en `CompanyDto`, `CreateCompanyDto` y `UpdateCompanyDto`.
    - En `CompanyService.cs`, se adaptaron `CreateCompanyAsync`, `UpdateCompanyAsync` y `MapToDto` para persistir y retornar la cadena Base64 del logo de la compañía.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Models/Company.cs`
  - `ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`
  - `ParkingApi.Domain/Dtos/Companies/CompanyDtos.cs`
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Control Estricto de MaxBranches y Código de Sede Autoincremental
- **`💬 Prompt Original del Usuario`**:
  > *"Listo, ya se creo bien las compañias deja crear bien las sedes, pero encontramos otro error se supone que cuando se crea la compañia se le coloca el limite de sedes que puede tener le colocamos 3 y dejo pasar dejor crear 4 sedes eso no debe ser posible debería salir un mensaje de alerta con el mismo diseño del sistema donde diga ya alcanzaste tu limite comprado si necesitas mas sedes contacta al administrador si me explico. otro error que encontrado el codigo de sede necesito que le crees un codigo dinamico si pero que no se repita en la bd osea oculta ese campo del fronted pero que si guarde un codigo dinamico osea consecutivo entonces va a coger el nombre de la compañia y le va a sumar el 01 despues el02 si me explico así autoincremental pero se debe quitar de la visual del usuario tanto al crear como al editar la sedeme explico analiza el plan para revisarlo."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Control de Límite de Sedes (`BranchService.cs`)**:
    - Inyección de `ICompanyRepository` en `BranchService`.
    - En `CreateAsync`, se consulta la empresa y se evalúa `if (company.MaxBranches > 0 && existingBranches.Count >= company.MaxBranches)` lanzando `InvalidOperationException` descriptiva para bloquear creaciones que superen el cupo contratado.
    - Se mapeó `MaxBranches` en `AuthResponseDto` en `LoginStandardAsync` para su consumo reactivo en la sesión del cliente.
  - **Generación Consecutiva de Código de Sede**:
    - Si el código no viene en el payload (`string.IsNullOrWhiteSpace(dto.Code)`), se calcula un prefijo a partir del nombre de la empresa y se busca el siguiente consecutivo libre (`EMPRESA-01`, `EMPRESA-02`, etc.) garantizando unicidad por empresa sin colisiones.
    - `Code` se marcó como opcional en `CreateBranchDto` y `UpdateBranchDto`.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Dtos/Branches/BranchDtos.cs`
  - `ParkingApi.Domain/Dtos/Auth/AuthResponseDto.cs`
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `ParkingApi/Controllers/BranchesController.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Compatibilidad de Transacciones con MySqlRetryingExecutionStrategy
- **`💬 Prompt Original del Usuario`**:
  > *"genero este error no dejo crearlo arrojo este conflicto pero esta vez ni lo creo en la bd"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Soporte de Estrategia de Reintentos (`CreateExecutionStrategy`) en `CompanyService.cs`**:
    - Se encapsuló la ejecución de las transacciones manuales en `CreateCompanyAsync` y `DeleteCompanyAsync` mediante `_context.Database.CreateExecutionStrategy().ExecuteAsync(async () => { ... })`.
    - Esto resuelve la incompatibilidad de EF Core con `MySqlRetryingExecutionStrategy` cuando se invocan transacciones manuales, permitiendo que la creación y aprovisionamiento de nuevas empresas se ejecute como una unidad retriable 100% atómica.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Transaccionalidad Atómica y Código Unívoco en Aprovisionamiento de Nuevas Empresas
- **`💬 Prompt Original del Usuario`**:
  > *"Mira que intento desde el super admin crear una compañia y arroja este error pero al parecer si la crea por que si guarda en la bd ya revise pero generar error entonces algo esta mal por que si la crea pero genera el error eso no esta bien valida y genera el plan para la solución ."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Transacción Atómica Integral (`CompanyService.cs`)**:
    - Se envolvió todo el pipeline de aprovisionamiento de `CreateCompanyAsync` dentro de `using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken)`. Si cualquier paso falla (roles, acciones, sedes, usuario o tarifas), se ejecuta un `RollbackAsync` inmediato, garantizando que nunca queden registros huérfanos o empresas corruptas en la base de datos.
  - **Código de Sede Unívoco y No Colisionante**:
    - La sede inicial obligatoria se genera con el código unívoco `Code = $"SEDE-{company.Id:D2}"` (ej. `SEDE-04`), evitando colisiones de clave única en base de datos.
  - **Inicialización Completa de Entidades**:
    - Se asignaron todas las propiedades requeridas de `User` (`FirstName`, `FirstSurname`, `IdentificationTypeId`, etc.) y se garantizó la generación explícita de `Guid.NewGuid()` para `VehicleRates` y `BillingResolutions`.
  - **Desempaquetado de Excepciones Detalladas (`CompaniesController.cs`)**:
    - Se modificaron los bloques `catch` de `Create` y `Update` para extraer `ex.InnerException?.Message`, entregando mensajes exactos en caso de errores de BD en lugar del mensaje genérico de EF Core.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`
  - `ParkingApi/Controllers/CompaniesController.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Herencia Automática de Sedes en Login para Usuarios de Empresa
- **`💬 Prompt Original del Usuario`**:
  > *"Listo perfecto, pero tengo otro error sucede que le listo cree el usuario en la otra compañia super bien le di permisos super bien le di todos los permisos super bien pero me loguee y de una me mando a crear sede pero si ya existe una sede en esa compañia por que me saco esa ventana eso no deberia ser así deberia existir algo antes. analiza eso y dame el plan"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Herencia Automática de Sedes (`AuthService.cs`)**:
    - En `LoginStandardAsync` y `LoginAsync`, si el usuario autenticado pertenece a una empresa (`user.CompanyId.HasValue`):
      - Se consulta si tiene sedes asignadas en `UserBranches`. Si existen asignaciones explícitas (y no es administrador global de la empresa), se retornan dichas sedes.
      - Si `UserBranches` está vacío o el usuario es administrador de la empresa, **hereda automáticamente todas las sedes activas de su empresa** (`_branchRepository.GetBranchesByCompanyIdAsync`), evitando retornar una lista vacía de sedes (`[]`) que provocaría el disparo erróneo del modal de primera sede en el frontend.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Corrección de Índice Único Multi-Tenant en Sedes y Visibilidad de Usuarios por Empresa
- **`💬 Prompt Original del Usuario`**:
  > *"Intente crear una sede y se revento, segundo estoy como superadministrador controlando una sede pero no me carga los usuarios de esa sede y reviso en la bd y si esta creado los usuarios yo los cree pero no los esta mostrando ni filtrando, revisa eso que esta pasando llega null algo esat m al por que filtra por sede los usuartios si soy super administrador o igual soy administrador como va a filtrar por sede el usuario no entiendo ese filtro entiendo lo de la compañia nada mas es lo correcto. si me explico. analiza ese proceso y dame el plan"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Índice Multi-Tenant de Sedes (`EntityConfigurations.cs`)**:
    - Se modificó la restricción única en `Branches` pasando de `builder.HasIndex(b => b.Code).IsUnique()` (global erróneo) a `builder.HasIndex(b => new { b.CompanyId, b.Code }).IsUnique()` (único por empresa). Esto permite que diferentes empresas utilicen los mismos códigos de sede (ej. `SEDE-01`) sin colisiones de base de datos.
  - **Validación y Mensajes Claros (`BranchesController.cs`)**:
    - Se agregó validación previa de código dentro de la misma empresa antes del insert.
    - Se implementó captura detallada de excepciones internas (`ex.InnerException?.Message`) retornando mensajes descriptivos para la UI en vez de 500 genéricos.
  - **Inclusión Permanente de Administradores (`UserRepository.cs`)**:
    - En `GetUsers`, cuando se recibe `branchId`, se asegura que los administradores de la empresa (`Role == "Administrador" || Role == "Admin" || Role == "Super Administrador" || Role == "Super Admin"`) no sean excluidos por no tener una fila fija en `UserBranches`.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`
  - `ParkingApi/Controllers/BranchesController.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/Branches/IBranchRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Branches/BranchRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Users/UserRepository.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---

## 📌 Entrada: Blindaje y Soporte de Contexto Multi-Organización vía Header X-Company-Id para SuperAdmin
- **`💬 Prompt Original del Usuario`**:
  > *"AUDITORÍA Y BLINDAJE: GESTIÓN DE ROLES/PERMISOS MULTI-ORGANIZACIÓN PARA SUPERADMIN (PWA & API). Verificar y validar exhaustivamente que la experiencia de administración multi-tenant en la ParkingPwa y el backend ParkingApi mantenga aislamiento estricto por organización cuando opera un usuario con rol SuperAdmin."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Soporte de Contexto Header `X-Company-Id` (`CurrentUserService.cs`)**:
    - `GetEffectiveCompanyId` ahora valida si `requestedCompanyId` viene por query parameter o en el header HTTP `X-Company-Id`.
    - Si el usuario es `SuperAdmin`, se adopta el ID de la organización objetivo; si el usuario es un administrador de tenant regular, el backend fuerza de forma intransferible el `CompanyId` de su claim JWT, ignorando cualquier intento de manipulación por headers o parámetros.
  - **Aislamiento en `UserRoleRepository` y `RoleActionRepository`**:
    - Consultas filtran estrictamente por `x.CompanyId == targetCompanyId`.
    - La asignación de permisos `AssignRolePermissionsAsync` afecta única y exclusivamente las filas vinculadas a la clave primaria `roleId` de esa organización específica.
  - **Cero Errores de Compilación**:
    - `dotnet build` ejecutado exitosamente (**0 Errores**).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Infrastructure/Security/CurrentUserService.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` (**0 Errores**).

---
- **`💬 Prompt Original del Usuario`**:
  > *"AUDITORÍA TÉCNICA EXHAUSTIVA: SISTEMA DE PERMISOS (PWA/API/WPF) Y MULTI-TENANCY SaaS. Diagnóstico del flujo de permisos (PWA -> API -> WPF), blindaje de aislamiento multi-tenant SaaS (Organizaciones y Sedes), cero errores de compilación y registro estricto en HISTORIAL_CAMBIOS.md."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Servicio de Contexto de Usuario (`ICurrentUserService` / `CurrentUserService`)**:
    - Se agregaron las propiedades y métodos `UserId`, `ParsedUserId`, `CompanyId`, `IsSuperAdmin`, `RoleId`, `RoleName`, `GetEffectiveCompanyId(int? requestedCompanyId)` y `CanAccessCompany(int targetCompanyId)`.
    - Blindaje de acceso: Si un usuario no es SuperAdmin, el backend ignora cualquier `companyId` enviado por query string o payload y fuerza estrictamente el `CompanyId` de su claim JWT.
  - **Blindaje Multi-Tenant en Capa de Repositorios y Servicios**:
    - `IBranchRepository` / `BranchRepository`: `GetActiveAsync(int? companyId = null)` aísla sedes por empresa.
    - `IMonthlySubscriptionRepository` / `MonthlySubscriptionRepository`: `GetAllAsync`, `GetActiveAsync`, `GetActiveByPlateAsync` filtran por `companyId` y `branchId`.
    - `IParkingTicketRepository` / `ParkingTicketRepository`: `GetActiveTicketsAsync`, `GetTodayCompletedTicketsAsync`, `GetHistoryAsync`, `GetAllAsync`, `CountActiveAsync`, `CountTodayCompletedAsync`, `CountTodayTotalAsync`, `GetTodayRevenueAsync` reciben `branchId` y `companyId` para total aislamiento por sede y empresa.
    - `ISyncService` / `SyncService`: `GetBootstrapDataAsync(int? branchId)` sincroniza de forma segura `UserRoles` y `RoleActions` filtrados por la empresa/sede activa, además de sedes, tarifas, comercios, convenios, turnos, mensualidades, novedades y tiquetes.
  - **Blindaje Multi-Tenant en Controladores**:
    - `BranchesController.cs`, `MonthlySubscriptionsController.cs`, `TicketsController.cs`, `UsersController.cs`, `UserRoleController.cs`, `VehicleRatesController.cs`, `PaymentMethodController.cs`, `StoresController.cs`, `AgreementsController.cs`, `ResolutionsController.cs` inyectan `ICurrentUserService` y aplican `_currentUser.GetEffectiveCompanyId` en todas las consultas y validaciones de creación/edición.
  - **Extensión de DTOs de Sincronización (`SyncDtos.cs`)**:
    - Se definieron `UserRoleSyncDto` y `RoleActionSyncDto`, incorporándolos a `BootstrapSyncDto` para garantizar la sincronización offline completa de roles y permisos hacia clientes WPF.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Interfaces/Services/ICurrentUserService.cs`
  - `ParkingApi.Infrastructure/Security/CurrentUserService.cs`
  - `ParkingApi.Domain/Dtos/Sync/SyncDtos.cs`
  - `ParkingApi.Domain/Dtos/MonthlySubscriptions/MonthlySubscriptionDtos.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/Branches/IBranchRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Branches/BranchRepository.cs`
  - `ParkingApi.Domain/Interfaces/Services/Branches/IBranchService.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `ParkingApi/Controllers/BranchesController.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/MonthlySubscriptions/IMonthlySubscriptionRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/MonthlySubscriptions/MonthlySubscriptionRepository.cs`
  - `ParkingApi.Domain/Interfaces/Services/MonthlySubscriptions/IMonthlySubscriptionService.cs`
  - `ParkingApi.Core/Services/MonthlySubscriptions/MonthlySubscriptionService.cs`
  - `ParkingApi/Controllers/MonthlySubscriptionsController.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/Tickets/IParkingTicketRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Tickets/ParkingTicketRepository.cs`
  - `ParkingApi.Domain/Interfaces/Services/Tickets/IParkingTicketService.cs`
  - `ParkingApi.Core/Services/Tickets/ParkingTicketService.cs`
  - `ParkingApi/Controllers/TicketsController.cs`
  - `ParkingApi/Controllers/PublicTicketsController.cs`
  - `ParkingApi.Core/Services/Sync/SyncService.cs`
  - `ParkingApi.Core/Services/Analytics/AnalyticsService.cs`
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi/Controllers/UsersController.cs`
  - `ParkingApi/Controllers/UserRoleController.cs`
  - `ParkingApi/Controllers/VehicleRatesController.cs`
  - `ParkingApi/Controllers/PaymentMethodController.cs`
  - `ParkingApi/Controllers/StoresController.cs`
  - `ParkingApi/Controllers/AgreementsController.cs`
  - `ParkingApi/Controllers/ResolutionsController.cs`
  - `Scripts/02_Init_RBAC_Seed.sql`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` ejecutado en `c:\Users\migue\source\repos\ParkingApi` con resultado exitoso (**0 Errores**).

---
- **`💬 Prompt Original del Usuario`**:
  > *"no, esta mal, cuando como super administrador ingreso administrar un parqueadero me deben salir toda la infomacion de ese parqueadero, dashboard, caja, activos, reportes, novedades y confguraciones (sedes, usuarios, roles) ... valida porque en BD y las apis deben retornar los usuarios y roles de cada parqueadero cuando ingreso a dichos modulos"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Aislamiento por `BranchId` en `UserRole` y API**:
    - Se incluyó `BranchId` en la entidad `UserRole` y `GetUserRoleDto`.
    - Se agregó el soporte opcional `branchId` en `IUserRoleRepository.GetUserRoles`, filtrando por sede cuando el contexto lo indique, permitiendo que la PWA segmente la lista de roles al seleccionar un parqueadero/sede.
  - **Aislamiento por `BranchId` en Usuarios (`UserRepository`)**:
    - Se actualizó el método `GetUsers` en `UserRepository.cs` para filtrar por `BranchId` a través de la entidad puente `UserBranches` cuando el parámetro `branchId` está presente.
  - **Migración de Base de Datos**:
    - Se creó la migración EF Core `AddBranchIdToUserRole` para la columna `BranchId` con clave foránea hacia la tabla `Branches`.
  - **Endpoint Controllers (`UserRoleController.cs`, `UsersController.cs`)**:
    - Se añadieron parámetros `[FromQuery] int? branchId` en los endpoints `GetUsersRoles` y `GetUsers` para propagar la solicitud de filtrado hacia la capa de persistencia.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain\Models\UserRole.cs`
  - `ParkingApi.Domain\Dtos\UserRoles\GetUserRoleDto.cs`
  - `ParkingApi.Infrastructure\Data\Configurations\EntityConfigurations.cs`
  - `ParkingApi.Infrastructure\Data\Repositories\UserRoles\UserRoleRepository.cs`
  - `ParkingApi.Infrastructure\Data\Repositories\Users\UserRepository.cs`
  - `ParkingApi.Core\Services\UserRoles\UserRoleService.cs`
  - `ParkingApi.Core\Services\Users\UserService.cs`
  - `ParkingApi\Controllers\UserRoleController.cs`
  - `ParkingApi\Controllers\UsersController.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` ejecutado en `ParkingApi.slnx` con resultado exitoso (**0 Errores**).

---

## 📌 Entrada: Aislamiento Estricto de Roles y Usuarios por Parqueadero / Empresa (Multi-Tenant SaaS)
  > *"tengo un problema cuando ingreso con el super administrador y administro un parqueadero veo todos sus roles, sin embargo, no me esta filtrando los roles que se encuentran creados para cada parqueadero, porque cuando ingreso a otro parqueadero veo los mismos, requiero es que si yo ingreso a la administracion de un parqueadero, desde el superadministrador me muestre sus roles y usuario, si ingreso a otro igual, caso contrario que pasaria ya cuando ingreso con el usuario administrador de ese parqueadero, a el solo le deberia de mostrar los roles y usuarios asociados a ese parqueadero , valida porque en BD y las apis deben retornar los usuarios y roles de cada parqueadero cuando ingreso a dichos modulos"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Eliminación de auto-aprovisionamiento forzado en `UserRoleRepository.GetUserRoles`**:
    - Se reemplazó `EnsureCompanyDefaultRolesAsync` (que reinsertaba "Supervisor" y "Operador" en cada consulta) por `EnsureCompanyAdminRoleAsync`. Ahora solo se crea el rol `Administrador` si la empresa no cuenta con ningún rol en base de datos.
    - Se garantiza que `GetUserRoles(companyId)` retorne **estrictamente** los roles vinculados a esa empresa (`WHERE CompanyId == companyId`).
    - Para consultas globales (`companyId == null`), retorna únicamente los roles del sistema (`CompanyId == null`, Rol `Super Administrador`).
  - **Persistencia de `CompanyId` en `UserRoleController.SaveOrEditUserRole`**:
    - Se incluyó la extracción del claim `company_id` del token JWT del usuario autenticado si el payload del DTO no lo provee explícitamente.
  - **Aislamiento en Consulta de Usuarios (`UserRepository.GetUsers`)**:
    - Cuando `companyId.HasValue && companyId > 0`: Filtra por `x.CompanyId == cid || (x.CompanyId == null && x.UserBranches.Any(ub => ub.Branch.CompanyId == cid))`.
    - Cuando `companyId == null`: Filtra estrictamente por `x.CompanyId == null` (usuarios de la plataforma global SaaS).
  - **Filtrado por Empresa en `BranchesController.GetAll`**:
    - Soportó parámetro opcional `[FromQuery] int? companyId` con fallback automático al claim `company_id` del token JWT para restringir las sedes retornadas a la empresa del usuario.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Infrastructure/Data/Repositories/UserRoles/UserRoleRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Users/UserRepository.cs`
  - `ParkingApi/Controllers/UserRoleController.cs`
  - `ParkingApi/Controllers/BranchesController.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` ejecutado en `ParkingApi.slnx` con resultado exitoso (**0 Errores**).

## 📌 Entrada: Corrección de Consulta LINQ en Aprovisionamiento de Empresas (EF Core / MySQL)
- **`💬 Prompt Original del Usuario`**:
  > *"cuando intento registrar un nuevo parqueadero me sale como en la segunda imagen"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Corrección de incompatibilidad SQL en `CompanyService.cs`**:
    - Se reemplazó `.Where(m => m.IsActive && m.Id != 16 && !m.Name.Contains("SaaS", StringComparison.OrdinalIgnoreCase))` por `.Where(m => m.IsActive && m.Id != 16 && !m.Name.ToLower().Contains("saas"))`.
    - Esta modificación permite que el proveedor Pomelo MySQL de Entity Framework Core traduzca correctamente la consulta LINQ a `LOWER(m.Name) NOT LIKE '%saas%'` sin lanzar la excepción `InvalidOperationException: The LINQ expression could not be translated`.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` ejecutado en `ParkingApi` con resultado exitoso (**0 Errores**).

## 📌 Entrada: Aprovisionamiento Automático de Organización Tenant y Persistencia Integral de CompanyId (SaaS Multi-Tenant)
- **`💬 Prompt Original del Usuario`**:
  > *"Verificar que cuando se cree la compañia se guarde en la base de datos el companyid por que no se esta guardando entonces eso no va a generara el desacoplamiento que se necesita para cuadno creemos varias organizaciones por que es la idea del saas multitenat"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Aprovisionamiento Integral en `CompanyService.CreateCompanyAsync`**:
    - Al crear una nueva empresa (`POST /api/Companies`), se generan automáticamente:
      1. Entidad `Company` (generando su `Id` numérico).
      2. `UserRole` ("Administrador") con `CompanyId = company.Id`, asignando módulos y acciones permitidos (excluyendo Módulo 16 SaaS).
      3. `Branch` inicial obligatoria (`SEDE-01 - Sede Principal`) con `CompanyId = company.Id` y datos de contacto de la empresa.
      4. `User` administrador inicial con `CompanyId = company.Id`.
      5. Vinculación en `UserBranches` (`UserId = user.Id, BranchId = defaultBranch.Id, IsDefault = true`).
      6. Catálogo inicial de tarifas vehiculares (`VehicleRates`) para la empresa (`CompanyId = company.Id, BranchId = null`).
      7. Resolución de facturación inicial de prueba (`BillingResolutions`) para la empresa y sede (`CompanyId = company.Id, BranchId = defaultBranch.Id`).
      8. Medios de pago activos vinculados a la sede (`BranchPaymentMethods`).
  - **Erradicación del Fallback Quemado `CompanyId ?? 1` en `BranchService.cs`**:
    - Se removió la asignación forzada a empresa 1 en `BranchService.CreateAsync`.
    - `BranchesController.Create` ahora extrae el `company_id` de los claims del JWT si el DTO no lo provee explícitamente.
  - **Persistencia de `CompanyId` en Todos los Controladores y Servicios**:
    - `UsersController.cs`: Asigna `CompanyId` desde claims del usuario autenticado si viene nulo.
    - `VehicleRatesController.cs`: Asigna `CompanyId` desde claims.
    - `ResolutionsController.cs` & `BillingResolutionService.cs`: Mapeo y persistencia de `CompanyId` en `SaveBillingResolutionDto` y `BillingResolutionDto`.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `ParkingApi.Core/Services/Billing/BillingResolutionService.cs`
  - `ParkingApi.Domain/Dtos/Billing/BillingResolutionDto.cs`
  - `ParkingApi.Domain/Dtos/Billing/SaveBillingResolutionDto.cs`
  - `ParkingApi/Controllers/BranchesController.cs`
  - `ParkingApi/Controllers/UsersController.cs`
  - `ParkingApi/Controllers/VehicleRatesController.cs`
  - `ParkingApi/Controllers/ResolutionsController.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build` ejecutado en `ParkingApi` con resultado exitoso (**0 Errores**).

## 📌 Entrada: Aislamiento Estricto de SuperAdmin vs Administrador Tenant y Erradicación Total de Roles Quemados (RBAC 100% Basado en Datos)
- **`💬 Prompt Original del Usuario`**:
  > *"Listo sucede que el superadmin accede y super bien accede al perfil de eso pero cree un administrador y tambien accede al portal del superadmin y eso no deberia ser así creo que esta algo quemado en codigo que sea administrador aparte necesito que revises todo el codigo de todos los 3 proyectos que no tenga cosas quemadas que no deberian estar . analiza completamente todo el desarrollo"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Aislamiento de Multi-Tenant SuperAdmin vs Administrador Tenant (`AuthService.cs`, `CompanyService.cs`, `UserService.cs`)**:
    - **Diagnóstico**: La evaluación `!user.CompanyId.HasValue || ...` causaba que cualquier usuario sin empresa o con rol administrador fuera promovido a Super Administrador global. Además, `UserService.CreateOrEditUser` omitía propagar `CompanyId`, dejando a los administradores recién creados como SuperAdmins de plataforma. `CompanyService.CreateCompanyAsync` asignaba el Módulo 16 (`companies.*`) a los administradores de parqueaderos clientes.
    - **Solución Aplicada**:
      - `isSuperAdmin` ahora evalúa estrictamente: `!user.CompanyId.HasValue && (user.UserRoleId == 1 || roleName.Equals("Super Administrador", ...))`.
      - `isAdmin` evalúa: `isSuperAdmin || (user.CompanyId.HasValue && roleName.Equals("Administrador", ...))`.
      - En `CompanyService.CreateCompanyAsync`, se excluye expresamente el Módulo 16 y acciones con slug `companies.*` al aprovisionar el rol de Administrador de una nueva empresa tenant.
      - En `UserService.CreateOrEditUser`, se asigna y persiste `user.CompanyId` correctamente.
      - En `GetUsersDto` y `UserRepository.cs`, se incluyó y mapeó `CompanyId`.
      - En `BranchRepository.GetUsersByBranchIdAsync`, los administradores retornados son estrictamente aquellos que pertenecen a la misma empresa de la sede (`u.CompanyId == branch.CompanyId`).
  - **Entrega de Permisos Dinámica (`AuthService.cs`)**:
    - Si el usuario no es SuperAdmin, los permisos se consultan en tiempo de ejecución desde la base de datos (`_roleActionRepository.GetActionsByRoleAsync(user.UserRoleId)`), garantizando que los administradores de inquilinos sólo tengan acceso a los módulos operativos y administrativos de su parqueadero.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Dtos/Users/GetUsersDto.cs`
  - `ParkingApi.Core/Services/Users/UserService.cs`
  - `ParkingApi.Core/Services/Companies/CompanyService.cs`
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Users/UserRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Branches/BranchRepository.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.

---
- **`💬 Prompt Original del Usuario`**:
  > *"y creo que deberiamos modificar el rol, el rol de creación deberia ser el superadmin no administrador el administrador es para el que le creamos el parqueadero si me explico"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Diferenciación Canónica de Roles (`02_Init_RBAC_Seed.sql` & `CompanyService.cs`)**:
    - **Rol 1 (Plataforma Global)**: Formalizado con el nombre **`Super Administrador`** (`Role = 'Super Administrador'`, `CompanyId = NULL`).
    - **Rol Cliente (Tenant)**: Creado automáticamente con el nombre **`Administrador`** (`Role = 'Administrador'`, `CompanyId = company.Id`) al registrar una nueva empresa mediante `CompanyService.CreateCompanyAsync`.
  - **Detección Dinámica en Backend (`AuthService.cs`)**:
    - Se actualizó la resolución de permisos y claims para reconocer tanto `roleName == "Super Administrador"`, `roleName == "SuperAdmin"`, como `!user.CompanyId.HasValue`.
  - **Endpoint de Sedes por Empresa (`BranchesController.cs`, `IBranchService.cs`, `BranchService.cs`)**:
    - Se expuso `GET /api/Branches/company/{companyId}` para permitir la consulta e inspección de sucursales/sedes pertenecientes a un parqueadero cliente desde la PWA.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Interfaces/Services/Branches/IBranchService.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi/Controllers/BranchesController.cs`
  - `ParkingApi/Scripts/02_Init_RBAC_Seed.sql`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.
- **`💬 Prompt Original del Usuario`**:
  > *"Tengo una consulta, se penso que el sistema es para venderlo pero es un saas completo entonces necesitamos un super admin que nosotros creemos entremos creemos un administrador y le demos ese usuario al man y que le ingrese cree su parqueadero y sus sedes y si le vendemos el producto a otras personas e igual se les cree su usuario administrador y que ingrese registre su parqueadero y sus sedes si me explico como se quiere manejar antes eso si lo entiendes encesito que revises toda la BD si la logica que tenemos si nos da para eso o que tanto se deberia cambiar ? necesito que revises eso y has un analisis completo y el plan completo que se deberia tomar."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Entidad `Company` (Tenant Maestro)**:
    - Se creó el modelo `Company` (`ParkingApi.Domain.Models.Company`) con propiedades: `Id`, `Name`, `LegalName`, `Nit`, `Email`, `Phone`, `Address`, `City`, `PlanType`, `MaxBranches`, `IsActive`, `SubscriptionExpiresAt`.
  - **Discriminador `CompanyId` en Entidades Operativas y de Seguridad**:
    - `Branch`: `int CompanyId` obligatorio + navegación `Company`.
    - `User`: `int? CompanyId` (null para SuperAdmin de plataforma) + navegación `Company?`.
    - `UserRole`: `int? CompanyId` (null para roles del sistema) + navegación `Company?`.
    - `VehicleRate`, `Store`, `ParkingTicket`, `WorkShift`, `MonthlySubscription`, `BillingResolution`, `VehicleIncident`: `int? CompanyId` + navegación `Company?`.
  - **Configuraciones Fluent API y DataContext (`EntityConfigurations.cs`, `DataContext.cs`)**:
    - Se registró `DbSet<Company> Companies` en `DataContext`.
    - Se configuraron índices y relaciones `OnDelete(DeleteBehavior.Restrict)` para preservar la integridad referencial y evitar borrados accidentales de empresas.
  - **Seguridad, JWT y Aprovisionamiento (`TokenHelper.cs`, `AuthService.cs`, `CompanyService.cs`)**:
    - `TokenHelper`: Emisión de claims `company_id`, `company_name` e `is_super_admin`.
    - `AuthService`: Detección dinámica de SuperAdmin (`!user.CompanyId.HasValue`), verificación de suspensión de empresa (`user.Company.IsActive == false`), y filtrado de sedes asignadas/pertenecientes a la empresa.
    - `CompanyService`: Transacción completa de aprovisionamiento de empresa: creación de `Company`, creación automática de rol `Administrador` para la empresa, asignación del 100% de módulos/acciones al nuevo rol, y creación del usuario administrador inicial con contraseña hasheada (BCrypt).
  - **Controlador API y DTOs (`CompaniesController.cs`, `CompanyDtos.cs`)**:
    - Endpoints CRUD: `GetAll`, `GetActive`, `GetById`, `Create`, `Update`, `ToggleStatus`.
  - **Scripts de Base de Datos MySQL (`01_Clean_All_Tables.sql`, `02_Init_RBAC_Seed.sql`)**:
    - `01_Clean_All_Tables.sql`: Agregado `DROP TABLE IF EXISTS Companies;`.
    - `02_Init_RBAC_Seed.sql`: DDL actualizado con tabla `Companies` y claves foráneas `CompanyId`, inserción de Empresa Matriz (Id: 1), Módulo 16 `Gestión de Empresas SaaS` y 74 Acciones del sistema (incluyendo `companies.view`, `companies.create`, `companies.edit`, `companies.suspend`, `companies.delete`).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Models/Company.cs` (Nuevo)
  - `ParkingApi.Domain/Models/Branch.cs`
  - `ParkingApi.Domain/Models/User.cs`
  - `ParkingApi.Domain/Models/UserRole.cs`
  - `ParkingApi.Domain/Models/VehicleRate.cs`
  - `ParkingApi.Domain/Models/Store.cs`
  - `ParkingApi.Domain/Models/ParkingTicket.cs`
  - `ParkingApi.Domain/Models/WorkShift.cs`
  - `ParkingApi.Domain/Models/MonthlySubscription.cs`
  - `ParkingApi.Domain/Models/BillingResolution.cs`
  - `ParkingApi.Domain/Models/VehicleIncident.cs`
  - `ParkingApi.Domain/Dtos/Companies/CompanyDtos.cs` (Nuevo)
  - `ParkingApi.Domain/Dtos/Auth/AuthResponseDto.cs`
  - `ParkingApi.Domain/Dtos/Auth/LoginResponseDto.cs`
  - `ParkingApi.Domain/Dtos/Branches/BranchDtos.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/Companies/ICompanyRepository.cs` (Nuevo)
  - `ParkingApi.Domain/Interfaces/Repositories/Branches/IBranchRepository.cs`
  - `ParkingApi.Domain/Interfaces/Services/Companies/ICompanyService.cs` (Nuevo)
  - `ParkingApi.Infrastructure/Data/DataContext.cs`
  - `ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Companies/CompanyRepository.cs` (Nuevo)
  - `ParkingApi.Infrastructure/Data/Repositories/Branches/BranchRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Users/UserRepository.cs`
  - `ParkingApi.Infrastructure/Helpers/Jwt/TokenHelper.cs`
  - `ParkingApi.Infrastructure/Extensions/RepositoryExtensions.cs`
  - `ParkingApi.Core/Services/Companies/CompanyService.cs` (Nuevo)
  - `ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `ParkingApi.Core/Extensions/ServiceExtensions.cs`
  - `ParkingApi/Controllers/CompaniesController.cs` (Nuevo)
  - `ParkingApi/Scripts/01_Clean_All_Tables.sql`
  - `ParkingApi/Scripts/02_Init_RBAC_Seed.sql`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.
- **`💬 Prompt Original del Usuario`**:
  > *"fui a crear la migración pues como cambiaron cosas y mira lo que me arrojo que paso hay ? (The entity type 'VehicleIncidentBranch' requires a primary key to be defined)"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Mapeo Fluent API de Clave Primaria Compuesta (`EntityConfigurations.cs`)**:
    - `ParkingBusinessConfigurations` no incluía la interfaz `IEntityTypeConfiguration<VehicleIncidentBranch>` en su declaración de clase, impidiendo que `modelBuilder.ApplyConfigurationsFromAssembly` invocara el método `Configure(EntityTypeBuilder<VehicleIncidentBranch>)`.
    - Se agregó `IEntityTypeConfiguration<VehicleIncidentBranch>` a la clase `ParkingBusinessConfigurations`.
    - La configuración Fluent API define explícitamente:
      `builder.HasKey(ib => new { ib.IncidentId, ib.BranchId });`
      `builder.HasOne(ib => ib.VehicleIncident).WithMany(i => i.IncidentBranches).HasForeignKey(ib => ib.IncidentId).OnDelete(DeleteBehavior.Cascade);`
      `builder.HasOne(ib => ib.Branch).WithMany().HasForeignKey(ib => ib.BranchId).OnDelete(DeleteBehavior.Cascade);`
    - Con esto, las herramientas de diseño de EF Core (`Add-Migration` / `dotnet ef migrations add`) reconocen correctamente la clave primaria compuesta y permiten generar la migración sin inconvenientes.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`
  - `ParkingApi/Scripts/01_Clean_All_Tables.sql`
  - `ParkingApi/Scripts/02_Init_RBAC_Seed.sql`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.

---

## 📌 Entrada: Validación de Bloqueo Activo por Novedad en Ingreso de Vehículos (`CheckInAsync`) y Multi-Sede DTO
- **`💬 Prompt Original del Usuario`**:
  - *"Ahora ayudame en poner en ejecucion el modulo de novedades, ayudame a conectarla creacion de la novedad, la cual debe de ir por api hacia a BD, para que luego el wpf pueda identificar que existe una placa con novedad y no permita registarle entrada (no toques el wpf), adicional quiero que el menu desplegable de a izquierda en la web permita ocultarse asi como se hace en la version mobile"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Bloqueo Centralizado de Placas en API (`ParkingTicketService.cs`)**:
    - Se inyectó `IVehicleIncidentRepository` en `ParkingTicketService`.
    - En el método `CheckInAsync`, al recibir una placa, se consulta `_incidentRepository.GetActiveBlockByPlateAsync(normalizedPlate, dto.BranchId, cancellationToken)`.
    - Si existe una novedad activa con `IsBlocked = true`, el servicio interrumpe el flujo arrojando `InvalidOperationException($"VEHÍCULO BLOQUEADO: La placa '{normalizedPlate}' tiene un bloqueo activo registrado por novedad: '{blockedIncident.IncidentType}' ({blockedIncident.Description}). No está permitido su ingreso.")`.
    - `TicketsController.cs` captura la excepción retornando `400 Bad Request` con el mensaje explícito, garantizando que tanto terminales de escritorio (WPF) como móviles o web vean rechazada la emisión del tiquete para vehículos con bloqueo.
  - **Soporte Multi-Sede en CheckIn DTO (`CheckInRequestDto.cs`)**:
    - Se añadió la propiedad `public int? BranchId { get; set; }` a `CheckInRequestDto` y se asignó en la creación de `ParkingTicket`.
  - **Endpoints de Novedades (`VehicleIncidentsController.cs` & `VehicleIncidentService.cs`)**:
    - Verificación y conectividad de operaciones CRUD: `GetAll`, `GetById`, `CheckPlate`, `Create`, `Update`, `Resolve`, `Delete`.

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain/Dtos/Tickets/CheckInRequestDto.cs`
  - `ParkingApi.Core/Services/Tickets/ParkingTicketService.cs`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**, 4 Advertencias de paquetes estándar.

---

## 📌 Entrada: Control de Sesión Única Concurrente (Single Active Session per User - JWT + SignalR)
- **`💬 Prompt Original del Usuario`**:
  - *"Listo tengo otro ajuste que esta mas complejo pero necesario para cerrar el tema de seguridad completo se necesita que solo 1 usuario se pueda loguear si ya inicio sesión no puede iniciar sesión nuevamente o si lo hace cierra la sesión donde estaba logueado si me hago explicar lo que se requiere en ese tema de seguridad claro se debe validar por el token por el jwt algo que obligue a que se cierre la otra sesion y se abra la nueva me explico ? eso aplica para el wpf y la pwa para los dos."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Arquitectura de Sesión Única Concurrente Multi-Plataforma**:
    - **Backend (`ParkingApi`)**:
      - **JTI Único y Estado en Memoria/BD**: Al autenticarse (`Login`, `LoginAsync`, `LoginStandardAsync`), se genera un JWT con identificador `Jti` único, almacenado en `user.Token` (BD) y cacheado en `IMemoryCache` (`ActiveToken_User_{userId}`).
      - **Validación Estricta en Middleware (`JwtBearerEvents.OnTokenValidated`)**: Cada petición entrante contrasta el claim `jti` contra la sesión activa en BD/Caché. Si el token recibido pertenece a una sesión revocada o anterior, el middleware rechaza con `401 Unauthorized` (`context.Fail`).
      - **Emisión en Tiempo Real**: Al iniciar una nueva sesión, se emite el evento SignalR `UserSessionTerminated` con `UserId` y `SessionToken` (`newJti`).
      - **Endpoint de Comprobación**: Se añadió `[Authorize] GET /api/Auth/validate-session` en `AuthController.cs`.
    - **Cliente Escritorio (`ParkingWpf`)**:
      - **Suscripción SignalR y 401**: `MainShellViewModel` escucha `UserSessionTerminated` (filtrado por `ServerUserId`) y el evento `SessionTerminated` de `ParkingApiClient`.
      - **Cierre y Alerta Automática**: Si la sesión es revocada en otra terminal, muestra el modal explicativo: *"⚠️ Tu sesión ha sido cerrada porque se inició sesión desde otro dispositivo o estación de trabajo"*, limpia credenciales en `ISessionService` y `IApiClientService`, y transiciona limpiamente a la ventana de `Login`.
    - **Aplicación Web Progresiva (`ParkingPwa`)**:
      - **Interceptor HTTP (`apiClient.ts`)**: Ante cualquier respuesta `401`, almacena el motivo en `sessionStorage` y redirige a `/?expired=concurrent`.
      - **Banner de Alerta en Login (`Login.tsx`)**: Muestra un banner amarillo ámbar (`ShieldAlert`) indicando que la sesión previa fue finalizada debido a un inicio concurrente.
      - **Monitor de Latido (`SessionHeartbeat` en `App.tsx`)**: Valida reactivamente el estado de la sesión cada 30 segundos y ante el evento `window.onfocus` para expulsar inmediatamente al usuario si su pestaña estaba en segundo plano.

- **`📦 Componentes Modificados`**:
  - `ParkingApi/ParkingApi.Domain/Dtos/Realtime/ConfigNotificationDto.cs`
  - `ParkingApi/ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingApi/ParkingApi/Program.cs`
  - `ParkingApi/ParkingApi/Controllers/AuthController.cs`
  - `ParkingWpf/Parking/Models/ApiModels/ConfigNotificationDto.cs`
  - `ParkingWpf/Parking/Services/Contracts/IApiClientService.cs`
  - `ParkingWpf/Parking/Services/Implementations/ParkingApiClient.cs`
  - `ParkingWpf/Parking/ViewModels/MainShellViewModel.cs`
  - `ParkingPwa/src/shared/api/apiClient.ts`
  - `ParkingPwa/src/features/auth/ui/Login.tsx`
  - `ParkingPwa/src/App.tsx`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.
  - `dotnet build ParkingWpf.slnx`: **0 Errores**.
  - `npm run build` (`ParkingPwa`): **0 Errores**.

---

## 📌 Entrada: Erradicación de Medios de Pago Mockup y Control RBAC en Retiros de Efectivo (`shift.cash_withdrawal`)
- **`💬 Prompt Original del Usuario`**:
  - *"si no existe medios de pago por que el sistema trae efectivo si ya habiamos dicho que todo debe ser de la BD nada debe ser quemado ni que se inserte automaticamente si ves no existe nada de eso entonces no debe estar nada mockup debria sair la alerta de que no se puede dar cierre o salida pues no existen medios de pago en la sede si me explico analiza eso que te digo claramente."*
  - *"existe otra cosa veo que si existe el permiso de registrar retiros o sangrias pero se inactivo pero creo que no esta asociado en el wpf por que sigue mostrando el boton mira hay te lo anexe. analiza esos datos"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Erradicación de Medios de Pago Mockup / Auto-Insert**:
    - **Diagnóstico**: En `CheckOutViewModel.cs`, al consultar `db.PaymentMethods.Where(p => p.State)`, si la lista estaba vacía (`!methods.Any()`), se insertaba automáticamente un registro `"Efectivo"` con ID 1 en SQLite.
    - **Corrección**: Se eliminó totalmente la inserción de fallback. Si la base de datos no tiene medios de pago para la sede, `AvailablePaymentMethods` permanece vacía y `HasPaymentMethods = false`.
    - **UI y Bloqueo de Seguridad**: En `CheckOutView.xaml` se agregó un banner de advertencia si `HasPaymentMethods == false`. En `CheckOutViewModel.ProcessCheckOutAsync` se bloquea la operación si `!IsMonthlyTicket && (!HasPaymentMethods || SelectedPaymentMethodEntity == null)` mostrando la alerta explicativa de que la sede no tiene medios de pago habilitados.
  - **Control RBAC Estricto en Retiro de Efectivo / Sangrías de Turno**:
    - **Diagnóstico**: En `ShiftClosureView.xaml`, el botón *"Registrar Retiro de Efectivo (Recogida)"* condicionaba su visibilidad a `IsShiftOwner` en vez de consultar el permiso relacional `shift.cash_withdrawal`.
    - **Corrección**: Se inyectó `IPermissionService` en `ShiftClosureViewModel.cs`, se crearon propiedades observables (`CanWithdrawCash`, `CanCloseShift`, `CanHandoverShift`, `CanExportShift`, `CanViewShiftHistory`, `CanOpenShift`) y se enlazó el botón a `CanWithdrawCash`.
    - **Validación en Comando**: En `OpenCashWithdrawalDialogAsync()`, `OpenShiftAsync()`, `CloseShiftDirectAsync()` y `HandoverShiftAsync()` se agregaron validaciones con alertas de acceso denegado si no se cuenta con el permiso correspondiente.
    - **Sincronización Reactiva**: Al modificarse permisos en tiempo real vía SignalR (`PermissionsChanged`), `UpdatePermissions()` actualiza al instante la visibilidad y habilitación de los botones en WPF sin necesidad de cerrar sesión.

- **`📦 Componentes Modificados`**:
  - `ParkingWpf/Parking/ViewModels/CheckOutViewModel.cs`
  - `ParkingWpf/Parking/Views/CheckOutView.xaml`
  - `ParkingWpf/Parking/ViewModels/ShiftClosureViewModel.cs`
  - `ParkingWpf/Parking/Views/ShiftClosureView.xaml`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingWpf.slnx`: **0 Errores**.
  - `dotnet build ParkingApi.slnx`: **0 Errores**.

---

## 📌 Entrada: Cobro Minuto 1, Periodo de Gracia de Liquidación y Sincronización RBAC Realtime
- **`💬 Prompt Original del Usuario`**:
  - *"tengo dos temas que tratar, primero excelente lo del signal R cuando se asignan tarifas medios de pago todo eso pero en la modal no deberia decir signal r eso no le interesa al cliente, otra cosa es los permisos eso cuando la pwa agregue o modifique permisos algun usuario el sistema deberia tener el signal r para que obligue a actualizar el wpf para que los permisos este sincronizados si me explico por que como se le quita permisos a los roles entonces pues debe actualizar si me explico, ese es una. la otra es que no se donde el sistema tiene configurado no se donde o de donde esta tomando que no se le cobre desde el ingreso si ingreso un vehiculo se le cobra desde el primer minuto creo que el esta tomando el periodo de gracia que se le crea a la tarifa como para que inicie el cobro ese periodo de gracia es cuando se quiere liquidar se congele el valor por ese tiempo mientras pues la persona esta reuniendo el dinero si me explico analiza lo que te digo y dime que se debe hacer has el plan analiza bien todo."*

- **`🤖 Resumen Técnico para la IA`**:
  - **Cobro desde el Minuto 1**:
    - **Diagnóstico**: `EfPricingCalculatorService.cs` (WPF) y `PublicTicketsController.cs` (API) contenían `if (totalMinutes <= rate.GracePeriodMinutes) return 0m;`, provocando que los primeros 15 minutos de estancia fueran gratuitos ($0).
    - **Corrección**: Se eliminó la exoneración de estancia. Todo vehículo se liquida desde el primer minuto transcurrido (`Math.Max(1, Math.Ceiling(totalMinutes)) * MinuteRate` o fracción hora).
    - **Redefinición del Periodo de Gracia**: En `CheckOutViewModel.cs` (WPF), el `GracePeriodMinutes` de la tarifa del vehículo se aplica exclusivamente al momento de liquidar en caja, congelando el valor a pagar durante ese lapso (`_currentGracePeriodSeconds`) para permitir el pago y salida antes de recalcular tiempo excedido.
  - **Eliminación del Término Técnico "SignalR" en UI**:
    - En `SyncRequiredDialog.xaml`, se reemplazó `"⚡ TIEMPO REAL (SIGNALR)"` por `"⚡ SINCRONIZACIÓN EN TIEMPO REAL"`.
  - **Sincronización Reactiva de Permisos RBAC en Tiempo Real**:
    - **Backend (`ParkingApi`)**: `RoleActionsController.cs` y `UsersController.cs` ahora inyectan `IRealtimeNotificationService` y emiten `PermissionsChanged` y `UsersChanged` tras asignaciones de permisos o cambios de usuario.
    - **Cliente WPF (`ParkingWpf`)**: `IApiClientService` incorpora `GetRolePermissionsAsync(roleId)`. En `MainShellViewModel.cs`, al recibir el evento en tiempo real, se consultan los permisos actualizados del rol del usuario activo y se recarga `_permissionService.LoadPermissions(...)` en caliente, actualizando la botonera y restricciones de inmediato sin cerrar sesión ni reiniciar la aplicación.

- **`📦 Componentes Modificados`**:
  - `ParkingApi/ParkingApi/Controllers/RoleActionsController.cs`
  - `ParkingApi/ParkingApi/Controllers/UsersController.cs`
  - `ParkingApi/ParkingApi/Controllers/PublicTicketsController.cs`
  - `ParkingApi/ParkingApi.Domain/Dtos/Auth/AuthResponseDto.cs`
  - `ParkingApi/ParkingApi.Core/Services/Auth/AuthService.cs`
  - `ParkingWpf/Parking/Views/SyncRequiredDialog.xaml`
  - `ParkingWpf/Parking/Services/Implementations/EfPricingCalculatorService.cs`
  - `ParkingWpf/Parking/ViewModels/CheckOutViewModel.cs`
  - `ParkingWpf/Parking/ViewModels/MainShellViewModel.cs`
  - `ParkingWpf/Parking/Services/Contracts/IApiClientService.cs`
  - `ParkingWpf/Parking/Services/Implementations/ParkingApiClient.cs`
  - `ParkingWpf/Parking/Services/Implementations/AuthService.cs`
  - `ParkingWpf/Parking/Models/UserSessionModel.cs`
  - `ParkingWpf/Parking/Models/ApiModels/TicketApiModels.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.
  - `dotnet build ParkingWpf.slnx`: **0 Errores**.

---

## 📌 Entrada: Corrección Error 404 en F5 / Rutas Limpias en IIS y React PWA (Rama `dev`)
- **`💬 Prompt Original del Usuario`**:
  - *"mira esta todo normal cuando me logueo perfecto bien funciona bien, pero tengo el siguiente problema si le doy f5 me sale el 404, creo que el tema de las rutas esta super mal algo sucede enserio no entiendo como funciona ya solucionamos el del login creo que en historial de cambios puedes revisar eso, pero creo que todas las rutas deberian estar definidas como rutas no como si fuera alguna carpeta si me explico ? analiza eso que sucede. para darnos el plan de reparacion para que eso no vuelva a suceder."*
  - *"Estabamos en la rama que no era ahora necesito que vuelvas a realizar el analisis en esta rama que es la actualizada necesito que verifiques si es que el plan que acabaste de hacer toca aplicarlo a esta rama o no realiza la revision completa"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Diagnóstico en Rama `dev`**: En `dev`, el frontend React (`src/App.tsx`) ya contaba con `RootAuthHandler`, `ProtectedRoute` y rutas anidadas limpias, y `public/web.config` ya tenía la regla `<action type="Rewrite" url="/" />`. Sin embargo, en `.github/workflows/main.yml`, el paso 5 del pipeline ejecutaba `cat << 'EOF' > dist/web.config` y **sobreescribía** el `web.config` compilado por Vite con el texto viejo que contenía `<action type="Rewrite" url="/Parking/index.html" />`. Esto provocaba que en cada despliegue por FTP a producción, IIS recibiera una regla apuntando a una subcarpeta `/Parking/` inexistente en la URL web, arrojando el error `404 - File or directory not found` al recargar (F5).
  - **Solución Aplicada**:
    - **Pipeline CI/CD (`.github/workflows/main.yml`)**: Se actualizó el paso 5 para generar la regla limpia con `url="/"`, exclusión de `^/api` y encabezados de seguridad `X-Content-Type-Options: nosniff`.
    - **Configuración IIS (`public/web.config`)**: Se completaron los tipos MIME para fuentes (`.woff`, `.woff2`) y manifiestos JSON.
    - **Vite PWA Workbox (`vite.config.ts`)**: Se configuró `navigateFallback: '/'` y `navigateFallbackDenylist: [/^\/api/]` para el soporte offline y recargas en modo PWA.

- **`📦 Componentes Modificados`**:
  - `ParkingPwa/.github/workflows/main.yml`
  - `ParkingPwa/public/web.config`
  - `ParkingPwa/vite.config.ts`

- **`✅ Verificación y Compilación`**:
  - `npm run build`: Compilación exitosa (**0 Errores**). `dist/web.config` y `dist/index.html` generados correctamente con regla de reescritura hacia la raíz `/`.
  - `npx tsc -b`: **0 Errores** de TypeScript.

---

## 📌 Entrada: Erradicación de Roles Quemados y Entrega Dinámica de Permisos RBAC en Login
- **`💬 Prompt Original del Usuario`**:
  - *"bueno tengo este problema con los permisos mira que si se asignaron permisos al usuario que tiene el rol 2 pero ingreso en el wpf y me dice que no cuento con los permisos me imagino por que solo ha tomado los datos de la sql lite nada mas pero no ya elimine la db la volvi a mandar a crear y no no sirvio entonces que sucede por que no esta tomando los permisos correctamente ? que sucede hay revisa eso por que administrador si funciona ."*
  - *"eso esta gravisimo en el sistema no debe a ver nada quemado todo lo que traiga la base de datos si el quisiera crearlo como cajero o cajera o hasta colocar el rol que quisiera desde que tenga los permisos que es lo importante se deberia validar como se te ocurre eso . revisa eso que me acabas de decir esta supremamente mal y eso deberia ir en reglas del agent como colocar eso así eso no es una buena practica"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Backend (ParkingApi)**:
    - Se incorporó la regla de oro en `AGENTS.md` de prohibición estricta de evaluar nombres de roles mediante texto quemado.
    - Se agregó `List<string> Permissions` a `AuthResponseDto`.
    - En `LoginStandardAsync`, se inyectó `IRoleActionRepository` y se consultan dinámicamente los slugs de acciones activas (`ra.ActionName`) asignadas al `user.UserRoleId` en la tabla `RoleAction`, devolviendo la matriz exacta de permisos asignada en la base de datos.
  - **Cliente WPF (ParkingWpf)**:
    - `LoginApiResponse` recibe la lista `Permissions`.
    - `AuthService` elimina por completo listas estáticas y métodos por coincidencia de texto, cargando los permisos reales en memoria tanto en modo Online (desde API) como en modo Offline (desde SQLite `RolePermissions`).

- **`📦 Componentes Modificados`**:
  - `ParkingApi.Domain\Dtos\Auth\AuthResponseDto.cs`
  - `ParkingApi.Core\Services\Auth\AuthService.cs`
  - `AGENTS.md`
  - `HISTORIAL_CAMBIOS.md`

- **`✅ Verificación y Compilación`**:
  - `dotnet build ParkingApi.slnx`: **0 Errores**.

---

## 📌 Entrada: Asignación Explícita de Tipo de Vehículo y Eliminación de Tarifas Vehiculares
- **`💬 Prompt Original del Usuario`**:
  - *"Quiero que en esta pantalla me permita asignar el tipo de vehiculo en la parametrizacion del parqueadero, no que me lo asigne automaticamente, adicional agregale la opcion de eliminar al tipo de vehiculo"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Backend (ParkingApi)**:
    - Se agregó `Task<bool> DeleteAsync(Guid id)` en `IVehicleRateRepository` y `VehicleRateRepository`.
    - Se agregó `Task<bool> DeleteRateAsync(Guid rateId)` y sobrecarga completa `UpdateRateAsync(VehicleRate)` en `IVehicleRateService` y `VehicleRateService`.
    - Se implementó el endpoint `[HttpDelete("{id}")]` y se mejoró `[HttpPut("{id}")]` en `VehicleRatesController` con notificación en tiempo real `RatesChanged`.
  - **Frontend (ParkingPwa)**:
    - **Selector Explícito de Tipo de Vehículo**: Se implementó un `<select>` en el modal de tarifas vehiculares de la sede (`ParqueaderosTab.tsx`) y en el catálogo general (`VehiculosConfigTab.tsx`) con los tipos oficiales (`0: Automóvil / Sedán`, `1: Motocicleta`, `2: Camión / Pesado`, `3: Furgón / Van`, `4: Bicicleta`, `5: Camioneta / SUV`) junto con el nombre descriptivo y valores de cobro.
    - **Acción de Eliminación Reactiva**: Se añadió botón de eliminar (🗑️) en la columna de Acciones de ambas tablas, respaldado por un modal de confirmación temático PWA con alerta, botones secundarios/peligro y spinner de carga, aplicando actualización optimista en el estado de React sin recargar.
    - **Servicio PWA**: Se añadió `deleteConfig(rateId)` en `vehiculosConfigService.ts`.

- **`📦 Componentes Modificados`**:
  - `ParkingApi/ParkingApi.Domain/Interfaces/Repositories/VehicleRates/IVehicleRateRepository.cs`
  - `ParkingApi/ParkingApi.Infrastructure/Data/Repositories/VehicleRates/VehicleRateRepository.cs`
  - `ParkingApi/ParkingApi.Domain/Interfaces/Services/VehicleRates/IVehicleRateService.cs`
  - `ParkingApi/ParkingApi.Core/Services/VehicleRates/VehicleRateService.cs`
  - `ParkingApi/ParkingApi/Controllers/VehicleRatesController.cs`
  - `ParkingPwa/src/features/settings/data/vehiculosConfigService.ts`
  - `ParkingPwa/src/features/settings/ui/ParqueaderosTab.tsx`
  - `ParkingPwa/src/features/settings/ui/VehiculosConfigTab.tsx`

- **`✅ Verificación y Compilación`**:
  - `dotnet build`: Compilación exitosa (**0 Errores**).
  - `npx tsc --noEmit`: Tipado verificado (**0 Errores**).
  - Ambos servicios en ejecución (`http://localhost:5135` y `http://localhost:5173`).

---

## 📌 Entrada: Conexión 100% Dinámica de Medios de Pago y Resoluciones en Dashboard
- **`💬 Prompt Original del Usuario`**:
  - *"Veo , que en dashboard de nuevo esta cargando de nuevo esto que te señale, pero el debe esta conectado a los medios de pago que se encuentren creados en la Bd y api"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Diagnóstico**: En `Dashboard.tsx`, las gráficas de dona de "Distribución por Métodos de Pago" y "Resoluciones de Facturación" tenían arrays con valores hardcodeados por defecto (`Efectivo, Tarjeta, Transferencia / Factura POS`) cuando la lista de la BD no estaba cargada o como fallback, violando el principio Zero-Data y no reflejando los métodos creados por el usuario (ej: `Nequi`).
  - **Solución Aplicada**:
    - Se eliminaron completamente todos los fallbacks estáticos en `paymentDonutData` y `resolutionsDonutData`.
    - La gráfica ahora se construye **estrictamente con los medios de pago activos consultados a la API y base de datos** (`mediosPagoService.getPaymentMethods()`).
    - Si no existen medios de pago o resoluciones creadas en la base de datos, se muestra un estado vacío elegante invitando a crearlos en Configuración.
    - Se mapearon dinámicamente sus iconos, nombres y recaudaciones reales.

- **`📦 Componentes Modificados`**:
  - `ParkingPwa/src/features/dashboard/ui/Dashboard.tsx`

- **`✅ Verificación y Compilación`**:
  - `npx tsc --noEmit`: Tipado verificado (**0 Errores**).

---

## 📌 Entrada: Corrección Error 500 en GET /api/branches y Resoluciones
- **`💬 Prompt Original del Usuario`**:
  - *"http://localhost:5135/api/branches erorr 500"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Diagnóstico**: La entidad `Branch` incluía la propiedad `LogoBase64` que no existe como columna física en la tabla `Branches` de MySQL, provocando que EF Core generara `SELECT b.LogoBase64 ...` fallando con excepción `MySqlException: Unknown column 'b.LogoBase64' in 'field list'` en consultas directas a `/api/branches` y en consultas con JOIN como `/api/BillingResolutions`.
  - **Solución Aplicada**:
    - Se decoró la propiedad `LogoBase64` con `[NotMapped]` en `ParkingApi.Domain/Models/Branch.cs`.
    - Se agregó `builder.Ignore(b => b.LogoBase64);` en `MultiBranchConfigurations` dentro de `ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`.
  - **Resultado**: `GET /api/branches` responde con status **200 OK** correctamente.

- **`📦 Componentes Modificados`**:
  - `ParkingApi/ParkingApi.Domain/Models/Branch.cs`
  - `ParkingApi/ParkingApi.Infrastructure/Data/Configurations/EntityConfigurations.cs`

- **`✅ Verificación y Compilación`**:
  - `dotnet build`: Compilación exitosa (**0 Errores**).
  - Petición verificada con `Invoke-RestMethod` a `http://localhost:5135/api/branches` (**200 OK**).

---

## 📌 Entrada: Eliminación Definitiva de Usuarios, Loaders y Mejoras en PWA
- **`💬 Prompt Original del Usuario`**:
  - *"Noto que tambien al eliminar usuario, me lo deja es inactivo, pero quiero es eliminarlo, adicional eliminarlo desde la BD"*
  - *"en esta pantalla, las esquinas no se ven curveadas por el scrollbar, ajustalos , adicioal la lista de los parqueaderos muestramelo en una lista donde pueda escribir y me salga en busqueda"*
  - *"Cuando elimine el usuario, quiero que sea reactivo porque no me limpio la lista cuando elimine , tuve que refrescar la pantallla"*
  - *"Arreglame porque permite dar click como si fuera a escribir en esta pantalla"*
  - *"Perfecto, ahora agregale un loader cuando se cree el usuario, adicional que me muesre un dialog al estilo del pwa para confirmar la eliiminacion del usuario y lo mismo, agregale loader"*
  - *"Faltan que los botones que digan cancelar en los dialog ajustalos en cuanto a diseño"*
  - *"ejecuta el api y pwa"*
  - *"trata de que las opciones se vean se vean proporcionales lo que te señale en rojo"*

- **`🤖 Resumen Técnico para la IA`**:
  - **Backend (ParkingApi)**:
    - Se implementó `DeleteUser(userId)` en `UserRepository` y `DeleteUserAsync(userId)` en `UserService`.
    - Se desvinculan primero las asociaciones en `UserBranches` para prevenir errores de integridad referencial antes de realizar el borrado físico (`_context.User.Remove(user)`).
    - `UsersController` `[HttpDelete("{id}")]` ahora ejecuta `DeleteUserAsync` con respuesta `200 OK`.
  - **Frontend (ParkingPwa)**:
    - **Optimistic UI Reactivo**: En `UsuariosTab.tsx`, al confirmar la eliminación, el usuario es filtrado instantáneamente del estado local sin requerir recargar la página.
    - **Modal de Confirmación Moderno**: Reemplazo de alertas nativas con un diálogo estilizado al tema PWA (`AlertTriangle`, texto claro, advertencia de irreversibilidad y botones secundarios/peligro).
    - **Loaders y Spinners**: Animación `@keyframes spin` y `<Loader2>` dentro de botones al crear/editar usuario (`Creando...`/`Guardando...`) y al eliminar (`Eliminando...`), desactivando los botones para prevenir peticiones duplicadas.
    - **Scrollbar y Esquinas Redondeadas**: Ajuste en `Settings.css` (`overflow: hidden` en el modal y `overflow-y: auto` en `.modal-body` con `::-webkit-scrollbar` personalizado) para evitar que la barra de scroll recorte las esquinas curvas `border-radius: 20px`.
    - **Buscador Dinámico de Sedes**: En la sección "4. Sedes Autorizadas (Parqueaderos)", se agregó input de búsqueda en tiempo real, botones de selección masiva y contador de sedes seleccionadas.
    - **Control de Caret / Selección**: Se aplicó `user-select: none; cursor: default;` en fondos, títulos y etiquetas para evitar que aparezca el cursor de escritura al hacer clic en espacios vacíos.
    - **Estilos de Botones Cancelar**: Se estandarizó `.btn-secondary` en `Settings.css` e `index.css` con esquinas redondeadas (`10px`), microinteracciones hover/active y contraste suave.
    - **Navegación de Pestañas Proporcional**: Se ajustó `.settings-nav-tabs` con `grid-template-columns: repeat(auto-fit, minmax(130px, 1fr))` para que todas las pestañas de configuración mantengan el mismo ancho proporcional en una sola línea.

- **`📦 Componentes Modificados`**:
  - `ParkingApi/ParkingApi.Domain/Interfaces/Repositories/Users/IUserRepository.cs`
  - `ParkingApi/ParkingApi.Infrastructure/Data/Repositories/Users/UserRepository.cs`
  - `ParkingApi/ParkingApi.Domain/Interfaces/Services/Users/IUserService.cs`
  - `ParkingApi/ParkingApi.Core/Services/Users/UserService.cs`
  - `ParkingApi/ParkingApi/Controllers/UsersController.cs`
  - `ParkingPwa/src/features/settings/data/usuariosService.ts`
  - `ParkingPwa/src/features/settings/ui/UsuariosTab.tsx`
  - `ParkingPwa/src/features/settings/ui/Settings.css`
  - `ParkingPwa/src/index.css`
  - `ParkingPwa/src/features/settings/ui/RolesTab.tsx`

- **`✅ Verificación y Compilación`**:
  - `dotnet build`: Compilación exitosa (**0 Errores**).
  - `npx tsc --noEmit`: Tipado verificado (**0 Errores**).
  - Ambos servicios en ejecución (`http://localhost:5135` y `http://localhost:5173`).
