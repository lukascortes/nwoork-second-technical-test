# INFORME DE DEUDA TÉCNICA Y BUGS — TimeOff Manager

> Complemento de [AUDIT_REPORT.md](AUDIT_REPORT.md). Catálogo accionable de deuda técnica, bugs confirmados, bugs potenciales, código muerto y duplicación, con evidencia (`archivo:línea`), impacto, riesgo, prioridad y solución recomendada.

---

## 1. Tabla maestra de deuda técnica

| ID | Hallazgo | Evidencia | Impacto | Riesgo | Prioridad |
| --- | --- | --- | --- | --- | --- |
| TD-01 | `UsersController` sin `[Authorize]` | [UsersController.cs:12](backend/TimeOffManager/Controllers/UsersController.cs#L12) | Escalada de privilegios total | Compromiso completo | **Crítico** |
| TD-02 | Secreto JWT commiteado + fallback débil | [appsettings.json:3](backend/TimeOffManager/appsettings.json#L3), [Program.cs:50](backend/TimeOffManager/Program.cs#L50) | Falsificación de tokens | Suplantación total | **Crítico** |
| TD-03 | Regresión de login sin commitear | `git diff` [authService.ts:27](frontend/src/api/authService.ts#L27) | Empleado no puede crear solicitudes | Funcionalidad rota | **Crítico** |
| TD-04 | SQLite sobre `emptyDir` en k8s | [k8s/backend-deployment.yaml:23-25](k8s/backend-deployment.yaml#L23-L25) | Pérdida total de datos al reiniciar | Pérdida de datos | **Crítico** |
| TD-05 | `EnsureCreated()` con migraciones presentes | [Program.cs:92](backend/TimeOffManager/Program.cs#L92) | Esquema divergente / deploy roto | Inconsistencia | **Alto** |
| TD-06 | Sin índice único en `Email` | [AppDbContextModelSnapshot.cs:54-74](backend/TimeOffManager/Migrations/AppDbContextModelSnapshot.cs#L54-L74) | Cuentas duplicadas, login ambiguo | Integridad | **Alto** |
| TD-07 | Cobertura de tests 0% | (todo el repo) | Regresiones silenciosas | Alto | **Alto** |
| TD-08 | .NET 6 fuera de soporte (EOL nov-2024) | [TimeOffManager.csproj:4](backend/TimeOffManager/TimeOffManager.csproj#L4) | Sin parches de seguridad | Vulnerabilidades sin fix | **Alto** |
| TD-09 | CORS `*` + token en localStorage + sin HTTPS | [Program.cs:37-42](backend/TimeOffManager/Program.cs#L37-L42) | Exposición de tokens | Robo de sesión | **Alto** |
| TD-10 | FK `OnDelete: Cascade` Users→Requests | [AppDbContextModelSnapshot.cs:81](backend/TimeOffManager/Migrations/AppDbContextModelSnapshot.cs#L81) | Borrado de usuario destruye historial | Pérdida de auditoría | **Alto** |
| TD-11 | Código muerto: `UserService`, `useApi`, `Task*`, `FilterBar` | ver §4 | Confusión, mantenimiento | Bajo | **Medio** |
| TD-12 | `bin/`, `obj/`, `*.dll`, `*.pdb`, `timeoff.db` versionados | [git ls-files] | Ruido en VCS, churn binario | Bajo | **Medio** |
| TD-13 | Mapeo manual enum↔número (front) | [requestConverters.ts](frontend/src/utils/requestConverters.ts) | Acoplamiento frágil a ordinales | Bug silencioso | **Medio** |
| TD-14 | Validador acoplado a MVC (`IActionResult`) | [TimeOffRequestValidator.cs](backend/TimeOffManager/Validators/TimeOffRequestValidator.cs) | No reutilizable, difícil de testear | Mantenibilidad | **Medio** |
| TD-15 | Transacciones manuales sobre un único `SaveChanges` | [TimeOffRequestsController.cs:56](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L56) | Overhead innecesario | Bajo | **Medio** |
| TD-16 | Idiomas mezclados (es/en) en código y UI | múltiple | UX/onboarding inconsistente | Bajo | **Medio** |
| TD-17 | `console.log` de depuración (incl. token, responses) | múltiple | Ruido + fuga en logs | Bajo-Medio | **Medio** |
| TD-18 | DTO de entrada ausente (over-posting) | [TimeOffRequestsController.cs:54](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L54) | Manipulación de modelo | Medio | **Medio** |
| TD-19 | `DbContextOptions` inyectado y no usado | [UsersController.cs:17](backend/TimeOffManager/Controllers/UsersController.cs#L17), [TimeOffRequestsController.cs:17](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L17) | Confusión | Bajo | **Bajo** |
| TD-20 | README desactualizado (endpoints inexistentes) | [README.md](README.md) | Onboarding erróneo | Bajo | **Bajo** |
| TD-21 | Sin CI/CD | (no `.github/workflows`) | Sin gates de calidad | Medio | **Medio** |
| TD-22 | Sin manejo de errores centralizado / `ProblemDetails` | (no middleware) | Respuestas inconsistentes | Bajo | **Medio** |
| TD-23 | Sin paginación en listados | [timeOffService.ts](frontend/src/api/timeOffService.ts), `GetAll` | Lentitud a escala | Bajo | **Bajo** |
| TD-24 | Validación de negocio duplicada cliente/servidor | [EmployeeDashboard.tsx:58-70](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx#L58-L70) | Lógica divergente | Bajo | **Bajo** |

---

## 2. Catálogo de bugs

### 2.1 Bugs confirmados (observados directamente en el código)

#### B1 — Regresión de login rompe la creación de solicitudes 🔴 Crítico
- **Descripción:** El working tree (cambios sin commitear) introduce una regresión en la cadena de identidad del usuario.
- **Evidencia:**
  - `git diff` en [authService.ts:27](frontend/src/api/authService.ts#L27): `id: response.data.userId` → `id: response.data.id`. El backend devuelve `userId`, no `id` ([AuthController.cs:39](backend/TimeOffManager/Controllers/AuthController.cs#L39)) → `id` queda `undefined`.
  - [useLogin.ts:44](frontend/src/pages/Login/useLogin.ts#L44): se quitó `.toString()`; `localStorage.setItem('userId', undefined)` guarda la cadena `"undefined"`.
  - [EmployeeDashboard.tsx:77-78](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx#L77-L78): `Number("undefined")` → `NaN`; `if (!userId) throw 'User not authenticated'`.
- **Archivo(s):** `authService.ts`, `useLogin.ts`, `EmployeeDashboard.tsx`.
- **Severidad:** Crítica (funcionalidad central rota).
- **Solución:** Revertir a `response.data.userId` y `response.id.toString()`. Mejor aún: derivar la identidad **del token JWT** (decodificar claims) en lugar de `localStorage`, y no enviar `userId` desde el cliente (el backend ya lo ignora).

#### B2 — `exp` del JWT calculado con hora local 🟠 Alto
- **Descripción:** La expiración del token usa hora local del servidor; el claim `exp` se interpreta como UTC.
- **Evidencia:** [JwtService.cs:30](backend/TimeOffManager/Services/JwtService.cs#L30) `expires: DateTime.Now.AddHours(4)`.
- **Severidad:** Alta (sesiones expiran antes/después de lo previsto según zona horaria; en UTC-negativo el token puede nacer "ya válido por más tiempo" o casi expirado).
- **Solución:** `DateTime.UtcNow.AddHours(4)`.

#### B3 — Logo roto en build de producción 🟡 Medio
- **Descripción:** Ruta relativa de asset que solo existe en el dev server de Vite.
- **Evidencia:** [LoginPage.tsx:24](frontend/src/pages/Login/LoginPage.tsx#L24) `src="../src/img/nwoork.png"`.
- **Severidad:** Media (imagen rota en producción).
- **Solución:** `import logo from '../../img/nwoork.png'` y usar `src={logo}` para que Vite lo procese.

#### B4 — `authService.logout` limpia claves equivocadas 🟢 Bajo
- **Descripción:** Borra `'id'` en vez de `'userId'`/`'userRole'`, dejando datos huérfanos en `localStorage`.
- **Evidencia:** [authService.ts:34-38](frontend/src/api/authService.ts#L34-L38).
- **Severidad:** Baja (el `logout` real está en `useAuth` y sí limpia bien; esto es inconsistencia).
- **Solución:** Unificar el logout en un solo lugar y limpiar `token`, `userRole`, `userId`.

#### B5 — `.env` apunta al puerto incorrecto 🟡 Medio (dev)
- **Descripción:** El `.env` local usa `:8000` pero el backend corre en `:5000`.
- **Evidencia:** [frontend/.env](frontend/.env) `http://127.0.0.1:8000/api` vs [.env.example](frontend/.env.example) `http://localhost:5000/api`.
- **Severidad:** Media en desarrollo (todas las llamadas fallan).
- **Solución:** Alinear `.env` con `:5000` (o el puerto real). No versionar `.env` (ya está en `.gitignore`).

#### B6 — `userService.getByRole` llama a un endpoint inexistente 🟢 Bajo
- **Descripción:** Hace `GET /users/role/{role}`, ruta no expuesta por el backend.
- **Evidencia:** [userService.ts:86-97](frontend/src/api/userService.ts#L86-L97).
- **Severidad:** Baja (método muerto; fallaría con 404/405 si se invocara).
- **Solución:** Eliminar el método o implementar el endpoint si se necesita.

### 2.2 Bugs potenciales (probables por diseño)

#### BP1 — Over-posting / mass assignment 🟡 Medio
- **Evidencia:** [TimeOffRequestsController.cs:54](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L54) bindea `TimeOffRequest` completo (incluye navegación `User`).
- **Solución:** DTO de entrada estricto. Ver [SECURITY_REVIEW.md](SECURITY_REVIEW.md) S9.

#### BP2 — `EnsureCreated` + migraciones divergentes 🟠 Alto
- **Evidencia:** [Program.cs:92](backend/TimeOffManager/Program.cs#L92). `EnsureCreated` crea el esquema desde el modelo sin `__EFMigrationsHistory`; un `dotnet ef database update` posterior fallará o producirá un esquema inconsistente.
- **Solución:** Reemplazar por `db.Database.Migrate()` y eliminar `EnsureCreated`.

#### BP3 — Actualización optimista con estado obsoleto 🟢 Bajo
- **Evidencia:** [useAdminDashboard.ts:28-41](frontend/src/pages/AdminDashboard/useAdminDashboard.ts#L28-L41) usa la variable `requests` del cierre (no updater funcional) tanto en el optimismo como en el rollback.
- **Solución:** `setRequests(prev => prev.map(...))` y guardar el snapshot previo para el rollback.

#### BP4 — Emails duplicados rompen login determinista 🟠 Alto
- **Evidencia:** Sin unicidad (TD-06); login usa `FirstOrDefaultAsync` ([AuthController.cs:25-26](backend/TimeOffManager/Controllers/AuthController.cs#L25-L26)).
- **Solución:** Índice único + normalización de email.

---

## 3. Duplicación de código (DRY)

| Duplicación | Ubicaciones | Recomendación |
| --- | --- | --- |
| Creación de usuario | [UserService.cs:20-38](backend/TimeOffManager/Services/UserService.cs#L20-L38) ≡ [UsersController.cs:59-77](backend/TimeOffManager/Controllers/UsersController.cs#L59-L77) | Usar el servicio desde el controller; borrar la lógica inline |
| Validación de solapamiento | [EmployeeDashboard.tsx:58-70](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx#L58-L70) (cliente) ≡ [TimeOffRequestValidator.cs:21-29](backend/TimeOffManager/Validators/TimeOffRequestValidator.cs#L21-L29) (servidor) | Mantener la del servidor como autoridad; el cliente solo UX, con mensaje consistente |
| Navbars | [Navbar.tsx](frontend/src/components/layout/Navbar.tsx) y [AdminNavbar.tsx](frontend/src/components/layout/AdminNavbar.tsx) | Un único componente parametrizado por rol |
| Patrón de formulario | Formik en `RequestForm` vs `useState` manual en `UserForm`/`useRequestForm` | Unificar en Formik+Yup |

---

## 4. Código muerto y artefactos a eliminar

| Elemento | Evidencia | Acción |
| --- | --- | --- |
| `UserService` (nunca inyectado) | [UserService.cs](backend/TimeOffManager/Services/UserService.cs) | Reactivar (usarlo) o eliminar |
| `DbContextOptions` inyectado | ambos controllers | Eliminar del constructor |
| Hook `useApi` (no usado) | [useApi.ts](frontend/src/hooks/useApi.ts) | Adoptarlo en servicios o eliminar |
| `TaskList`/`TaskItem`/`TaskForm` (plantilla to-do) | [components/](frontend/src/components/) | Eliminar |
| `FilterBar` (filtros "completed/pending" de to-do) | [FilterBar.tsx](frontend/src/components/FilterBar.tsx) | Eliminar o reimplementar para estados de solicitud |
| `userService.getByRole` (endpoint inexistente) | [userService.ts:86](frontend/src/api/userService.ts#L86) | Eliminar |
| `console.log` de depuración | múltiple | Eliminar (especialmente el del token) |
| `bin/ obj/ *.dll *.pdb *.db *.sqbpro` versionados | repo | `.gitignore` raíz + `git rm --cached` |

---

## 5. Estimación de esfuerzo de pago de deuda

| Bloque | Items | Esfuerzo aprox. |
| --- | --- | --- |
| Bloqueantes (TD-01..04, B1) | Seguridad + datos + regresión | 1–2 días |
| Alta prioridad (TD-05..10, B2) | Migraciones, unicidad, CORS/TLS, .NET, cascada | 3–5 días |
| Limpieza/calidad (TD-11..24, B3..B6) | Código muerto, DTOs, enums, logs, CI, errores | 3–5 días |
| Tests (TD-07) | Suite inicial backend+front | 3–5 días |

> **Total estimado para "listo para producción mínimo viable seguro": ~2–3 semanas** de un ingeniero, priorizando primero los bloqueantes.
