# Historial Oficial de Modificaciones y Control de Cambios (Parking API)
**Proyecto**: ParkFlow API Central (ASP.NET Core Web API)  
**Fecha de Creación**: 2026-08-25  

---

## 📌 Protocolo Obligatorio de Registro y Contexto Multi-PC
> [!IMPORTANT]
> **PRESERVACIÓN DE CONTEXTO ENTRE COMPUTADORES**: Como el desarrollo se realiza alternando entre diferentes estaciones de trabajo (PCs), este protocolo garantiza que la IA nunca pierda el hilo técnico ni el contexto acumulado.

Cualquier asistente de IA, desarrollador o mantenedor que realice cambios en el código fuente de este proyecto **DEBE** registrar su modificación en este documento antes de finalizar su turno o tarea, incluyendo:
1. **Fecha y Hora Exacta**.
2. **Autor / Agente Responsable**.
3. **`💬 Prompt Original del Usuario`**: Transcripción exacta o requerimiento solicitado por el usuario.
4. **`🤖 Resumen Técnico para la IA`**: Explicación técnica de arquitectura, contratos de datos modificados, DTOs, entidades, decisiones tomadas, estado del sistema y advertencias relevantes.
5. **`📦 Componentes Modificados`**: Lista precisa de rutas de archivos modificados, creados o eliminados.
6. **`✅ Verificación y Compilación`**: Resultado de compilación `dotnet build` (**0 Errores**) y pruebas funcionales.

---

## 📋 Registro Cronológico de Cambios

### [2026-08-25 21:45:00] - [FIX] [PERF] [DB] - Optimización de Connection Pooling MySQL y Resolución de Excepción max_user_connections (20)
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Prompt Original del Usuario**:
  > *"excelente lo primero funciono perfecto lo de los turnos excelente, pero sabes que no funciono mira esto el tema de las conexión me preocupa eso por que veo que no estas cerrando conexiones estas dejando conexiones abiertas en el backend eso esta gravisimos cuando hace varias operaciones ojo con eso necesito que realices un analisis completo de eso de que sucede con las conexiones."*
- **🤖 Resumen Técnico para la IA**:
  1. **Diagnóstico Causa Raíz**:
     - En hosting compartido (`site4now.net`), MySQL impone una cuota estricta de `max_user_connections = 20` por usuario.
     - `MySqlConnector` (driver de Pomelo EF Core) maneja por defecto `MaximumPoolSize = 100` y `ConnectionIdleTimeout = 180` segundos (3 minutos).
     - Al entrar peticiones concurrentes o sincronizaciones periódicas, el pool retenía hasta 20 conexiones TCP abiertas e intentaba abrir conexiones adicionales provocando `MySqlException: User has exceeded the 'max_user_connections' resource (20)`.
  2. **Configuración de Connection Pooling en `appsettings.json`**:
     - Se añadió: `Pooling=true;MinimumPoolSize=0;MaximumPoolSize=12;ConnectionIdleTimeout=5;ConnectionTimeout=20;`.
     - `MaximumPoolSize=12`: Limita el consumo de conexiones del backend a un máximo de 12 (por debajo del límite de 20 del hosting), evitando desbordes.
     - `ConnectionIdleTimeout=5`: Devuelve y cierra físicamente las conexiones inactivas tras solo 5 segundos (en vez de retenerlas 3 minutos).
     - `MinimumPoolSize=0`: No pre-aloja conexiones ociosas.
  3. **Escalabilidad y Planes de Producción**:
     - Se documentó que al migrar a base de datos dedicada o VPS (AWS RDS, Azure, DigitalOcean), el límite de MySQL suele ser de 150 a 1000+ conexiones (`max_user_connections = 0`), por lo que esta configuración asegura compatibilidad tanto en el plan gratuito como en alta concurrencia productiva.
- **📦 Componentes Modificados**:
  - `ParkingApi/appsettings.json`
  - `HISTORIAL_CAMBIOS.md`
- **✅ Verificación y Compilación**:
  - `dotnet build ParkingApi\ParkingApi.csproj` -> **0 Errores** (Compilación Correcta).

### [2026-08-25 21:30:00] - [FEAT] [MULTI-BRANCH] [SECURITY] - Independencia Multi-Sede Estricta de Turnos de Trabajo (WorkShifts), DTOs con BranchId y Filtrado de Métricas en Repositorio
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Prompt Original del Usuario**:
  > *"mira como se ve de feo eso, segundo fui a cerrar turno en una sede yo como administrador y mira como salio el error y eso daño todo el sistema. analiza eso y revisa bien como funciona eso por que no esta funcionando completamente bien.*
  > *tengo otra duda, se supone que los turnos son igual independientes de sedes claro ? eso espero sea claro si ? un turno pertenece a una sede especifica."*
- **🤖 Resumen Técnico para la IA**:
  1. **Contratos DTO y Enrutamiento Multi-Sede de Turnos**:
     - Se enriquecieron `OpenShiftRequestDto`, `ShiftSummaryDto` y `WorkShiftDto` en `ParkingApi.Domain/Dtos/Shifts/ShiftDtos.cs` con la propiedad `public int? BranchId { get; set; }`.
     - En `IShiftService` y `ShiftService` se incorporó el parámetro `int? branchId` en las operaciones de apertura (`OpenShiftAsync`), consulta de turno activo (`GetActiveShiftAsync`) e historial de turnos (`GetHistoryAsync`).
     - Al aperturar turno se almacena de forma persistente el `BranchId` en la entidad `WorkShift`.
  2. **Filtrado Estricto por Sede en Capa de Datos (`ShiftRepository.cs`)**:
     - En `IShiftRepository` y `ShiftRepository` se actualizaron las consultas EF Core (`GetActiveShiftByUserIdAsync`, `GetActiveShiftAsync`, `GetHistoryAsync`, `CalculateShiftMetricsAsync`) para filtrar las transacciones de parqueadero (`ParkingTickets`) y turnos (`WorkShifts`) exclusivamente por la sede activa (`BranchId == branchId`).
     - Esto garantiza aislamiento absoluto de métricas, dinero recaudado y liquidación entre distintas sedes para franquicias o sedes simultáneas.
  3. **Controlador HTTP (`ShiftsController.cs`)**:
     - Se actualizaron los endpoints `GET /api/shifts/active` y `GET /api/shifts/history` para admitir `[FromQuery] int? branchId` permitiendo a terminales cliente sincronizar y consultar estados de sede específicos.
- **📦 Componentes Modificados**:
  - `ParkingApi.Domain/Dtos/Shifts/ShiftDtos.cs`
  - `ParkingApi.Domain/Interfaces/Repositories/Shifts/IShiftRepository.cs`
  - `ParkingApi.Infrastructure/Data/Repositories/Shifts/ShiftRepository.cs`
  - `ParkingApi.Domain/Interfaces/Services/Shifts/IShiftService.cs`
  - `ParkingApi.Core/Services/Shifts/ShiftService.cs`
  - `ParkingApi.Core/Services/Sync/SyncService.cs`
  - `ParkingApi/Controllers/ShiftsController.cs`
  - `HISTORIAL_CAMBIOS.md`
- **✅ Verificación y Compilación**:
  - `dotnet build ParkingApi\ParkingApi.csproj` -> **0 Errores** (Compilación Correcta).

### [2026-08-25 21:00:00] - [FEAT] [DB] [SYNC] - Soporte de Logo de Sede en Base64 Comprimido en MySQL, DTOs de API y Transporte de Sincronización
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Prompt Original del Usuario**:
  > *"Mira en la primera imagen se ve supremamente mal el tema del diseño de la parte de arriba sigue siendo wpf pero sin diseño sin nada eso se ve mal si me explico.*
  > *en la seguna imagen no tiene logo si revisas el codigo de la PWA ves que cuando crea las sedes debe subir el logo que deberia tener entonces usar un logo sii que sea configurable o que tengamos un logo en el sistema o no se si puedas usar un ico o algo dime que se puede hacer hay pues lo digo por que cada sede tiene un logo.*
  > *y si ves la 3 imagen no tiene esa columna para el logo entonces eso como se va a subir donde se esta guardando eso deberia guardarse en base 64 comprimido para que se pueda leer desde la bd y sin generar tanto consumo de espacio si analiza eso recuerda que como regla de oro si no esta en el agent deberia estar no vas a tocar el pwa si mi autorizacion."*
- **🤖 Resumen Técnico para la IA**:
  1. **Esquema de Base de Datos (MySQL DDL)**:
     - Se creó el script SQL `ParkingApi/Scripts/04_Add_Branch_LogoBase64.sql` para agregar la columna `LogoBase64 LONGTEXT NULL` después de `Notes` en la tabla `branches`.
     - Esto permite almacenar logos comprimidos (WebP, PNG, JPEG en Base64 data URI) directamente en la base de datos sin depender de servicios de archivos externos ni sobrecargar la base de datos.
  2. **Modelo de Dominio y DTOs de Backend**:
     - En `ParkingApi.Domain/Models/Branch.cs`: Se agregó la propiedad `public string? LogoBase64 { get; set; }`.
     - En `ParkingApi.Domain/Dtos/Branches/BranchDtos.cs`: Se agregó `LogoBase64` a `BranchDto`, `CreateBranchDto` y `UpdateBranchDto`.
     - En `ParkingApi.Core/Services/Branches/BranchService.cs`: Se incluyó el mapeo bidireccional de `LogoBase64` en creación, actualización y consulta.
     - En `BootstrapSyncDto` / `SyncDtos.cs`: `Branches` transporta de manera automática el campo `LogoBase64` hacia las terminales WPF clientes.
  3. **Preservación de la PWA**:
     - Conforme a la regla estricta del usuario, **no se modificó ningún archivo de `ParkingPwa`**. La API y la base de datos quedan preparadas para que cuando el usuario dé la autorización, la PWA pueda enviar y recibir imágenes de logo en base64 en la creación/edición de sedes.
- **📦 Componentes Modificados**:
  - `ParkingApi/Scripts/04_Add_Branch_LogoBase64.sql`
  - `ParkingApi.Domain/Models/Branch.cs`
  - `ParkingApi.Domain/Dtos/Branches/BranchDtos.cs`
  - `ParkingApi.Core/Services/Branches/BranchService.cs`
  - `HISTORIAL_CAMBIOS.md`
- **✅ Verificación y Compilación**:
  - `dotnet build ParkingApi.slnx` -> **0 Errores**.
  - `dotnet build ParkingWpf.slnx` -> **0 Errores**.

### [2026-08-25 20:45:00] - [FEAT] [UI/UX] [MULTI-BRANCH] - Sincronización de Capacidad Real de Sede, Escalado Global de Tipografía (+2px), Remoción de Botón X y Banner Amarillo
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Prompt Original del Usuario**:
  > *"mira que si esta la capacidad del parqueadero pero veo que el wpf no la trae dice sin configurar esas cosas no deberian salir así. aparte toda la letra del sistema necesito que me le subas 2 px mas a cada letra si alguna tiene 8 pues queda en 10 y la de 10 en 12 si me hago entender , este boton no deberia estar toca quitarlo, este mensaje no deberia ser así de ese color por que no es error es algo informativo deberia ser amarillo. ya con eso procede a crear el plan"*
- **🤖 Resumen Técnico para la IA**:
  1. **Capacidad de Parqueadero Multi-Sede y Ocupación en Tiempo Real**:
     - Se ajustó el servicio de tiquetes y estadísticas en WPF para que la capacidad de parqueadero se derive dinámicamente de la sede activa (`_sessionService.CurrentBranch?.TotalCapacity` o tabla SQLite `Branches` sincronizada desde MySQL), eliminando el estado *"Sin configurar"*.
     - Se mapearon en `OccupancyStats.cs` las propiedades compatibles `AvailableSlots` y `OccupiedSlots`.
     - Se validó que el contrato `Branch` y DTOs de sincronización provean `TotalCapacity` a la terminal.
  2. **Escalado Global de Tipografía (+2px)**:
     - Se incrementaron en +2px todas las fuentes del sistema en `Typography.xaml`, `Controls.xaml`, `CheckInView.xaml`, `CheckOutView.xaml`, `MainShellWindow.xaml` y `BranchSelectionDialog.xaml`.
  3. **Limpieza de UI en Salida / Caja**:
     - Se retiró el botón `✕` (`ClearSearchCommand`) en `CheckOutView.xaml`, dejando un buscador moderno y despejado.
  4. **Banner Informativo / Advertencia en Amarillo**:
     - Se cambiaron los colores de advertencia/feedback en `CheckInView.xaml` para usar la paleta institucional amarilla (`BrushWarningBg`, `BrushWarning`, `BrushWarningText`).
- **📦 Componentes Modificados**:
  - `Parking (WPF)`: `OccupancyStats.cs`, `EfParkingTicketService.cs`, `TicketApiModels.cs`, `Typography.xaml`, `Controls.xaml`, `CheckOutView.xaml`, `CheckInView.xaml`, `MainShellWindow.xaml`, `BranchSelectionDialog.xaml`, `HISTORIAL_CAMBIOS.md`.
  - `ParkingApi`: `HISTORIAL_CAMBIOS.md`.
- **✅ Verificación y Compilación**:
  - `dotnet build ParkingApi.slnx` -> **0 Errores**.
  - `dotnet build ParkingWpf.slnx` -> **0 Errores**.

### [2026-08-25 20:15:00] - [FIX] [SYNC] [MULTI-PC] - Corrección de Fallo de Sincronización Bootstrap y Establecimiento de Protocolo de Contexto Multi-PC
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Prompt Original del Usuario**:
  > *"oye por que sale que no se tiene el servidor no respondio, pues si arria dice, eso deberia ya estar claro osea que si esta conectada la api osea que paso ? eso lo probe y estaba funcionando ahorita pero ahora no funciona, que suecede sabes que otra 0cosa pasa es que como estoy trabajando en dos lugares entonces creo que se esta perdiendo el contexto y eso esta terrible no sirve necesito que se cree un archivo en ese agent que se creo que son reglas donde diga que cada cambio nuevo o realizado debe crear en un archivo de registros con el promp que se hizo o el resumen que la IA entienda y cuando yo me encuentre en otro pc pues le diga que lo lea y tenga todo entendido lo ultimo que realizamos eso aplica tanto para el wpf y el api si me explico, ya con esto crea un plan completo y detallado."*
- **🤖 Resumen Técnico para la IA**:
  1. **Causa del Fallo de Sincronización**:
     - El indicador superior en la app WPF se mostraba *"API Central Online • Sincronizado"* porque `PingAsync()` contra `/api/health` respondía `200 OK`.
     - Sin embargo, la sincronización fallaba en el Paso 3 (`/api/sync/bootstrap`) debido a una discrepancia en los contratos de serialización: la API serializaba `WorkShift.Status` como string (`"Open"`/`"Closed"`) vía `JsonStringEnumConverter`, mientras que WPF lo esperaba como `int`, además de pequeñas discrepancias en nombres de propiedades de medios de pago y tipos de enums.
     - En el cliente WPF, `ParkingApiClient.GetBootstrapAsync()` capturaba silenciosamente la excepción devolviendo `null`, lo que disparaba la advertencia *"El servidor no respondió con los datos de sincronización"*.
  2. **Arquitectura y Solución Aplicada**:
     - En el backend API se verificó que `SyncService.cs` y `SyncController.cs` entreguen el 100% de los datos de todas las entidades (Sedes, Usuarios, Medios de pago, Tarifas, Comercios, Convenios, Turnos, Suscripciones y Tiquetes).
     - En WPF se implementaron DTOs resilientes desacoplados con normalizadores inteligentes para convertir tipos de forma segura sin excepciones.
  3. **Protocolo Multi-PC**:
     - Se crearon y alinearon las reglas en `AGENTS.md` y `HISTORIAL_CAMBIOS.md` en ambos repositorios (`ParkingApi` y `ParkingWpf`) estipulando que cada tarea registre el prompt original + resumen técnico estructurado para la IA.
     - **Directiva de Reanudación**: Cuando el usuario cambie de PC y diga *"Lee el historial de cambios / contexto"*, la IA leerá este archivo de inmediato para recuperar todo el contexto del proyecto.
- **📦 Módulos Modificados**:
  - `AGENTS.md`: Reglas obligatorias de planificación y protocolo de contexto multi-PC.
  - `HISTORIAL_CAMBIOS.md`: Historial oficial y contexto técnico para la IA.
- **✅ Verificación y Compilación**:
  - `dotnet build ParkingApi.slnx` -> **0 Errores**.
  - `dotnet build ParkingWpf.slnx` -> **0 Errores**.

### [2026-08-25 17:34:00] - [FEAT] [MULTI-BRANCH] [AUTH] - Retorno Global de Sedes para Administradores y Endpoint de Operadores por Sede
- **Autor**: Antigravity AI Assistant & .NET Software Architect
- **💬 Requerimiento**: Acceso multi-sede global para administradores y filtrado de operadores vinculados a sedes.
- **🤖 Resumen Técnico para la IA**:
  - En `AuthService.cs` y `BranchService.cs`, los administradores (`UserRoleId == 1`) obtienen siempre el 100% de las sedes activas vía `_branchRepository.GetActiveAsync()`.
  - Se creó el endpoint `GET /api/branches/{id}/users` en `BranchesController.cs` para obtener usuarios asignados en `UserBranches` para una sede específica.
- **📦 Módulos Modificados**:
  - `ParkingApi.Core`: `AuthService.cs`, `BranchService.cs`
  - `ParkingApi.Domain`: `IBranchRepository.cs`, `IBranchService.cs`
  - `ParkingApi.Infrastructure`: `BranchRepository.cs`
  - `ParkingApi`: `BranchesController.cs`
- **✅ Verificación**: `dotnet build ParkingApi.slnx` -> **0 Errores**.
