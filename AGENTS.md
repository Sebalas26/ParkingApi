# 📜 REGLAS ESTRICTAS DE DESARROLLO Y ARQUITECTURA PARA LA IA (PARKING API)

Este documento define las **Reglas de Oro y Estándares Obligatorios** para cualquier asistente de IA o desarrollador trabajando en el backend central (Parking API). **Estas reglas son inquebrantables.**

---

## 🛑 1. REGLA DE ORO: PLANIFICACIÓN PREVIA OBLIGATORIA
1. **Nunca modificar ni crear código directamente** ante una nueva solicitud o cambio de comportamiento sin antes elaborar un **Plan de Arquitectura e Implementación** detallado (`implementation_plan.md`).
2. **Esperar siempre la aprobación explícita del usuario** antes de ejecutar cualquier edición en los archivos del proyecto o ejecutar migraciones de base de datos.

---

## 🏢 2. ESTÁNDARES MULTI-SEDE Y SEGURIDAD
1. **Administradores Globales**: Los usuarios con rol Administrador siempre acceden a todas las sedes activas (`_branchRepository.GetActiveAsync()`).
2. **Operadores por Sede**: Las consultas y asignaciones operativas deben vincularse a través de `UserBranches` y filtrarse por `BranchId`.
3. **Autenticación Híbrida**: Permitir inicio de sesión tanto por `Username` como por `Email` en `LoginStandardAsync` y `LoginAsync`.
4. **Contratos de Datos Limpios (DTOs)**: Todo endpoint de sincronización y consulta pública debe entregar DTOs desacoplados de las entidades de EF Core para evitar referencias circulares o incompatibilidades de serialización con clientes de escritorio (WPF) o móviles.

---

## 🔒 3. PROHIBICIÓN ESTRICTA DE ROLES O PERMISOS QUEMADOS (HARDCODED)
> [!CAUTION]
> **PROHIBICIÓN ESTRICTA**: Jamás asumir, validar o asignar permisos mediante comparación de texto de nombres de rol (ej: `roleName.Contains("operador")`, `roleName.Contains("cajero")`).

1. **RBAC 100% Basado en Datos**:
   - La evaluación de permisos en el sistema debe provenir exclusivamente de la matriz relacional de la base de datos (`RoleActions` / `RolePermissions` / `Action.Slug`).
   - El administrador del sistema tiene libertad absoluta de crear roles con cualquier nombre (*"Cajera Noche"*, *"Auxiliar Patio"*, *"Operario Caja"*, etc.). El backend y los clientes deben resolver los permisos dinámicamente consultando la tabla de acciones asignadas al rol.

---

## 📝 4. PROTOCOLO ESTRICTO DE REGISTRO Y CONTEXTO MULTI-PC
> [!IMPORTANT]
> **PRESERVACIÓN DE CONTEXTO ENTRE COMPUTADORES**: Como el desarrollo se realiza alternando entre diferentes estaciones de trabajo (PCs), este protocolo garantiza que la IA nunca pierda el hilo técnico ni el contexto acumulado.

1. **Registro Obligatorio en Cada Modificación**:
   - Toda modificación, corrección de bug o nueva funcionalidad debe registrarse de inmediato en [`HISTORIAL_CAMBIOS.md`](file:///c:/Users/migue/source/repos/ParkingApi/HISTORIAL_CAMBIOS.md) antes de finalizar el turno.
2. **Estructura Requerida para Cada Entrada**:
   - **`💬 Prompt Original del Usuario`**: Transcripción exacta o requerimiento solicitado por el usuario.
   - **`🤖 Resumen Técnico para la IA`**: Explicación técnica de arquitectura, contratos de datos modificados, DTOs, entidades, decisiones tomadas, estado del sistema y advertencias relevantes.
   - **`📦 Componentes Modificados`**: Lista precisa de rutas de archivos modificados, creados o eliminados.
   - **`✅ Verificación y Compilación`**: Resultado de compilación `dotnet build` (**0 Errores**) y pruebas funcionales.
3. **Directiva de Reanudación de Sesión (Nuevo PC / Nueva Conversación)**:
   - Cuando el usuario inicie en otro computador o abra un nuevo chat e indique *"Lee el historial de cambios / contexto"* o similar, la IA **DEBE LEER OBLIGATORIAMENTE `HISTORIAL_CAMBIOS.md`** como primer paso antes de elaborar planes o tocar código.
4. **Cero Errores de Compilación**:
   - Todo cambio debe compilar limpiamente con `dotnet build` (**0 Errores**) antes de dar por finalizada la tarea.
