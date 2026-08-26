# 📜 HISTORIAL DE CAMBIOS Y CONTEXTO TÉCNICO MULTI-PC

Este archivo registra de forma acumulativa y cronológica todos los requerimientos, decisiones arquitectónicas, cambios en DTOs/entidades y estado de compilación del ecosistema Parking.

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
