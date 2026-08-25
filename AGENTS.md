# 📜 REGLAS ESTRICTAS DE DESARROLLO Y ARQUITECTURA PARA LA IA (PARKING API)

## 🛑 1. PLANIFICACIÓN PREVIA OBLIGATORIA
1. Siempre elaborar un `implementation_plan.md` antes de realizar cambios de código o esquema de base de datos y esperar la aprobación del usuario.

## 🏢 2. ESTÁNDARES MULTI-SEDE Y SEGURIDAD
1. **Administradores Globales**: Los usuarios con rol Administrador siempre acceden a todas las sedes activas (`_branchRepository.GetActiveAsync()`).
2. **Operadores por Sede**: Las consultas y asignaciones operativas deben vincularse a través de `UserBranches` y filtrarse por `BranchId`.
3. **Autenticación Híbrida**: Permitir inicio de sesión tanto por `Username` como por `Email` en `LoginStandardAsync` y `LoginAsync`.
