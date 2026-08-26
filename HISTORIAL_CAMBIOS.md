# 📜 HISTORIAL DE CAMBIOS Y CONTEXTO TÉCNICO MULTI-PC

Este archivo registra de forma acumulativa y cronológica todos los requerimientos, decisiones arquitectónicas, cambios en DTOs/entidades y estado de compilación del ecosistema Parking.

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
