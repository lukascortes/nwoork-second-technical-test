# AUDITORÍA TÉCNICA INTEGRAL — TimeOff Manager

> **Tipo de revisión:** Technical Due Diligence / Pre-Producción
> **Alcance:** Repositorio completo (backend .NET 6, frontend React, infraestructura, base de datos, Git)
> **Fuente de verdad:** El código (el README se trató como referencia histórica, no fiable)
> **Fecha:** 2026-06-23
> **Auditor:** Revisión senior (Architect / Staff Engineer / Security)

Documentos complementarios:
- [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md) — Arquitectura y modelo de datos en profundidad
- [SECURITY_REVIEW.md](SECURITY_REVIEW.md) — Análisis de seguridad y OWASP Top 10
- [TECH_DEBT_REPORT.md](TECH_DEBT_REPORT.md) — Deuda técnica y catálogo de bugs
- [IMPROVEMENT_ROADMAP.md](IMPROVEMENT_ROADMAP.md) — Roadmap priorizado

---

## 0. Veredicto Ejecutivo

TimeOff Manager es una aplicación full-stack **funcional en su flujo feliz** que demuestra amplitud técnica notable para una prueba: backend .NET 6 con EF Core + JWT + BCrypt, frontend React 19 + TypeScript con separación de hooks/presentación, migraciones versionadas, Dockerfiles y manifiestos Kubernetes. Sin embargo, **NO está lista para producción** y presenta **defectos críticos de seguridad y de integridad de datos** que impiden su despliegue tal cual.

| Aspecto | Estado |
| --- | --- |
| ✅ Lo que funciona | Login con JWT, dashboards por rol, creación/aprobación de solicitudes, hashing BCrypt, gating de rutas frontend, migraciones EF |
| 🔴 Bloqueante #1 | `UsersController` **sin autorización**: cualquiera (anónimo) crea usuarios Admin → toma de control total |
| 🔴 Bloqueante #2 | Clave de firma JWT **commiteada en texto plano** en `appsettings.json` → falsificación de tokens |
| 🔴 Bloqueante #3 | **Regresión sin commitear** en el flujo de login rompe la creación de solicitudes del empleado |
| 🔴 Bloqueante #4 | Persistencia SQLite sobre `emptyDir` en k8s → **pérdida total de datos** al reiniciar el pod |
| 🟠 Alto | Sin tests (0% cobertura), CORS abierto, token en `localStorage`, sin HTTPS, email sin unicidad |

**Recomendación:** No promover a producción. Resolver primero los 4 bloqueantes (estimado 1–2 días) y luego el bloque de seguridad/calidad (1–2 semanas) descrito en el [roadmap](IMPROVEMENT_ROADMAP.md).

### Scorecard

| Categoría | Nota | Resumen |
| --------------- | ---- | --- |
| Arquitectura | 5/10 | Intención por capas, pero lógica en controllers y capa de servicio/repositorio inexistente o muerta |
| Backend | 5/10 | Buenas bases (BCrypt, roles en requests), pero brecha de authz, `EnsureCreated` vs migraciones, over-posting |
| Frontend | 5/10 | Estructura limpia de hooks, pero token en localStorage, código muerto, regresión, mapeo manual de enums |
| Seguridad | 2/10 | CRUD de usuarios anónimo (escalada de privilegios), secreto JWT en repo, CORS `*`, sin HTTPS |
| Base de Datos | 3/10 | Sin unicidad de email, SQLite efímero, cascade delete, fechas como TEXT con mezcla de zonas horarias |
| Testing | 0/10 | No existe ni un solo test |
| Performance | 6/10 | Aceptable a esta escala; queries/transacciones redundantes, sin paginación |
| Mantenibilidad | 4/10 | Código muerto, duplicación, idiomas mezclados, `console.log` por todas partes, README desactualizado |
| Escalabilidad | 3/10 | SQLite mono-escritor + `emptyDir` + `replicas:1` sin ruta de escalado horizontal |
| **Calidad General** | **4/10** | Prototipo competente; lejos de grado producción |

---

## FASE 1 — Descubrimiento

### Visión General

- **Objetivo del sistema:** Gestionar solicitudes de tiempo libre (vacaciones, enfermedad, otros) de empleados, con aprobación/rechazo por parte de administradores.
- **Problema que resuelve:** Centralizar y dar trazabilidad al proceso de solicitud-aprobación de ausencias, reemplazando hojas de cálculo o correos.
- **Flujo principal de negocio:**
  1. Un administrador crea usuarios (no hay auto-registro real — ver hallazgo crítico).
  2. El empleado inicia sesión, ve su historial y crea solicitudes (quedan en `Pending`).
  3. El sistema valida fechas (no pasadas, inicio ≤ fin) y solapamientos por tipo.
  4. El administrador ve todas las solicitudes y las aprueba/rechaza (solo si están `Pending`).
- **Casos de uso identificados:**
  - CU-01 Login (Auth) — implementado.
  - CU-02 Empleado: listar mis solicitudes (`GET /api/TimeOffRequests/my`) — implementado.
  - CU-03 Empleado: crear solicitud (`POST /api/TimeOffRequests`) — implementado (roto en working tree, ver Fase 10).
  - CU-04 Admin: listar todas (`GET /api/TimeOffRequests/all`) — implementado.
  - CU-05 Admin: cambiar estado (`PUT /api/TimeOffRequests/{id}/status`) — implementado.
  - CU-06 Admin: gestión de usuarios CRUD (`/api/Users`) — implementado **pero sin autenticación**.
  - CU-07 Registro de usuario — **declarado en README, NO existe** en el backend.
  - CU-08 Filtrado por estado (`/filter?status=`) — **declarado en README, NO existe**; `FilterBar.tsx` es código muerto de otra app.
- **Actores del sistema:**
  - **Empleado** (`Role.Employee`): crea y consulta sus propias solicitudes.
  - **Administrador** (`Role.Admin`): consulta todas, aprueba/rechaza, gestiona usuarios.
  - **Anónimo:** hoy puede operar todo el CRUD de usuarios (defecto de seguridad).

### Stack Tecnológico Real (detectado en código)

**Backend** ([TimeOffManager.csproj](backend/TimeOffManager/TimeOffManager.csproj)):
- .NET **6.0** (LTS, **fin de soporte 12-nov-2024** — ya sin soporte de Microsoft).
- Entity Framework Core **6.0.28** (+ `Microsoft.EntityFrameworkCore.Sqlite`, `.Design`).
- Autenticación: `Microsoft.AspNetCore.Authentication.JwtBearer` 6.0.28.
- `BCrypt.Net-Next` 4.0.3 (hashing de contraseñas).
- `Swashbuckle.AspNetCore` 6.2.3 (Swagger).

**Frontend** ([package.json](frontend/package.json)):
- React **19.1** + React DOM 19.1, TypeScript 5.8.
- Vite **7.0** (build), `@vitejs/plugin-react`.
- `axios` 1.11, `react-router-dom` **7.7**, `formik` 2.4 + `yup` 1.6.
- `tailwindcss` 3.4, `@heroicons/react`, `react-icons`, `date-fns` 4.1.
- ESLint 9 + `typescript-eslint`.

**Base de datos:**
- Motor: **SQLite** (archivo `timeoff.db`, **commiteado en el repo**).
- ORM: EF Core 6; 3 migraciones (`InitialCreate`, `AddTimeOffRequest`, `RecreateTimeOffRequestsTable`).
- ⚠️ Contradicción: el arranque usa `db.Database.EnsureCreated()` ([Program.cs:92](backend/TimeOffManager/Program.cs#L92)), que **ignora las migraciones**.

**Infraestructura:**
- Variables de entorno: frontend `VITE_API_BASE_URL` (`.env`, `.env.example`); backend `ASPNETCORE_ENVIRONMENT`, `Jwt:*` (en `appsettings.json`).
- Docker: dos `Dockerfile` (backend aspnet:6.0, frontend node:18 → nginx:alpine).
- Orquestación: `k8s/backend-deployment.yaml`, `k8s/frontend-deployment.yaml` (Deployment + Service NodePort).
- **CI/CD: inexistente** (no hay `.github/workflows`, GitLab CI ni Azure Pipelines).
- Deploy: manual.

---

## FASE 2 — Arquitectura (resumen)

> Detalle completo en [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md).

**Backend** — Arquitectura de 2 capas de facto: Controllers que hablan directo con `AppDbContext`. Existe `Services/UserService.cs` pero está **sin usar** (el controller duplica su lógica inline). No hay capa de repositorio ni de aplicación. Validación delegada a `Validators/TimeOffRequestValidator` (estático) que **retorna `IActionResult`** (acoplamiento de la validación a MVC).

- **Cohesión:** media. **Acoplamiento:** alto (controllers ↔ EF directo; validador ↔ MVC).
- **Escalabilidad/Mantenibilidad:** limitada por SQLite y por lógica de negocio dispersa.

**Frontend** — Buena separación *container/hook/presentational*: cada página tiene su hook (`useAdminDashboard`, `useLogin`, `useUserManagement`) y componentes presentacionales. Estado por `useState` local + `Context` solo para auth. Routing con `react-router-dom` v7 y `ProtectedRoute` por rol.

- **Reutilización:** moderada (`RequestsTable`, `useApi`). `useApi` está **definido pero no se usa**.
- **Organización:** correcta, salvo código muerto (`components/Task*.tsx`, `FilterBar.tsx`) heredado de una plantilla de to-do.

---

## FASE 3 — Calidad de Código (resumen)

**Backend:**
- **SOLID/DRY:** `UserService.CreateUser` y `UsersController.CreateUser` son idénticos → duplicación. `DbContextOptions` se inyecta en ambos controllers y **nunca se usa**.
- **KISS:** transacciones manuales alrededor de un único `SaveChanges` (innecesarias); query de "verificación" post-insert ([TimeOffRequestsController.cs:78-85](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L78-L85)) que añade un round-trip inútil.
- **Naming:** mezcla español/inglés (`"Cant request time off for past dates"`, `// No hay errores`). `Models/Users.cs` define la clase `User` (singular) en archivo plural.
- **Código muerto:** `UserService`, `DbContextOptions` inyectado, `IConfiguration` redundante.
- **Complejidad:** baja-media; aceptable.

**Frontend:**
- **Componentización:** buena en el dominio de requests; débil en admin/users.
- **Duplicación:** validación de solapamiento replicada en cliente ([EmployeeDashboard.tsx:58-70](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx#L58-L70)) y servidor.
- **Patrones React:** correctos (custom hooks), pero `console.log` de depuración por todo el código (incluido el **token JWT**), y mapeo manual enum↔número frágil en `utils/requestConverters.ts`.

---

## FASE 4 — Seguridad (resumen)

> Análisis exhaustivo, PoC y remediación en [SECURITY_REVIEW.md](SECURITY_REVIEW.md).

| # | Hallazgo | OWASP | Nivel |
| - | --- | --- | --- |
| S1 | `UsersController` sin `[Authorize]`: CRUD de usuarios anónimo, creación de Admin | A01 Broken Access Control | **Crítico** |
| S2 | Clave de firma JWT commiteada en `appsettings.json` + fallback débil hardcodeado | A02 / A05 | **Crítico** |
| S3 | Sin endpoint de registro real; el "alta" es la brecha S1 | A04 Insecure Design | **Alto** |
| S4 | CORS `AllowAnyOrigin/Header/Method` | A05 Misconfiguration | **Alto** |
| S5 | JWT en `localStorage` + token logueado en consola | A07 / XSS | **Alto** |
| S6 | Sin HTTPS (`RequireHttpsMetadata=false`, sin `UseHttpsRedirection`) | A02 | **Alto** |
| S7 | Sin rate limiting / lockout en login | A07 | **Medio** |
| S8 | Sin política de contraseñas ni validación de email server-side | A04 | **Medio** |
| S9 | Over-posting en `POST /TimeOffRequests` (bindea entidad completa con navegación `User`) | A08 | **Medio** |
| S10 | Sin refresh tokens ni revocación; `exp` calculado con hora local | A07 | **Medio** |

**Aspectos positivos:** BCrypt para contraseñas; DTOs de respuesta que **no exponen** `PasswordHash`; `[Authorize(Roles=...)]` correctamente aplicado en `TimeOffRequestsController`; consultas EF parametrizadas (sin SQLi); React escapa salidas (sin `dangerouslySetInnerHTML`); el token es Bearer en header → **CSRF no aplica**; el backend ignora `userId/status` enviados por el cliente al crear (autoridad en servidor).

---

## FASE 5 — Base de Datos (resumen)

> Detalle y diagrama en [ARCHITECTURE_ANALYSIS.md](ARCHITECTURE_ANALYSIS.md#modelo-de-datos).

- **Esquema:** `Users (Id, Email, PasswordHash, Role)` 1—N `TimeOffRequests (Id, UserId FK, StartDate, EndDate, Type, Reason, Status, CreatedAt)`.
- **Relaciones:** FK `UserId` con **`OnDelete: Cascade`** → borrar un usuario destruye su historial de solicitudes (combinado con S1 = borrado masivo anónimo).
- **Índices:** solo `IX_TimeOffRequests_UserId`. **No hay índice único en `Email`** (confirmado en `AppDbContextModelSnapshot.cs`) → emails duplicados posibles; el login usa `FirstOrDefault` → ambigüedad.
- **Restricciones/Integridad:** sin unicidad de email, sin normalización (case-sensitive), sin `CHECK`.
- **Tipos:** `DateTime` como `TEXT`; mezcla de `DateTime.Today` (local), `DateTime.UtcNow` (UTC) y `DateTime.Now` (local, en el `exp` del JWT) → inconsistencia de zona horaria.
- **Escalabilidad/Consistencia:** SQLite es mono-escritor; en k8s con `emptyDir` los datos son **efímeros**.

---

## FASE 6 — Performance (resumen)

| Área | Hallazgo | Impacto estimado |
| --- | --- | --- |
| Backend | `GetAll` ejecuta `CountAsync()` + `Console.WriteLine` extra en cada carga (debug) | Bajo (1 query extra) |
| Backend | `Create` hace query de "verificación" `AsNoTracking` tras guardar | Bajo (1 round-trip/creación) |
| Backend | Transacciones manuales sobre un único `SaveChanges` | Muy bajo (overhead) |
| Backend | **Sin N+1**: `GetAll` proyecta con `Include` en una sola consulta SQL | ✅ Positivo |
| Frontend | Sin paginación: carga todas las solicitudes/usuarios de golpe | Medio a escala |
| Frontend | Sin memoización; re-render completo de tablas | Bajo |
| Frontend | `console.log` de objetos grandes (responses) en producción | Bajo |

A la escala actual (prueba técnica) el rendimiento es **aceptable**; los problemas son de higiene y de diseño para crecer.

---

## FASE 7 — UX y Producto (resumen)

- **Flujo de usuario:** claro; dashboards diferenciados; tabs en empleado (Mis solicitudes / Nueva).
- **Validación:** Formik + Yup en el formulario (fechas no pasadas, fin ≥ inicio). ✅
- **Mensajes:** **mezcla de idiomas** (errores en español dentro de UI en inglés: *"Ya tienes una solicitud aprobada/pendiente…"*). Inconsistente.
- **Estados de carga/vacío:** bien resueltos (`LoadingSpinner`, estados "No requests yet").
- **Accesibilidad:** `<label>` asociados en su mayoría; el login usa `type="text"` para email (no `type="email"`); el logo usa `src="../src/img/nwoork.png"` (ruta de dev que **rompe en build de producción**); enlaces `href="#!"` sin función ("Forgot password", "Remember me" no operativos).
- **Feedback:** el empleado fija `status: 1` (Approved) al crear (el backend lo corrige, pero es señal engañosa).

---

## FASE 8 — Testing

- **Tests existentes:** **ninguno** (ni xUnit/NUnit en backend, ni Vitest/Jest/RTL/Playwright en frontend). Cobertura **0%**.
- **Calidad/Riesgo de regresión:** **muy alto**. La prueba viviente: el working tree contiene una **regresión sin commitear** que rompe la creación de solicitudes del empleado y que ningún test detecta (ver Fase 10).
- **Áreas que deberían tener pruebas y no las tienen (prioridad):**
  1. `AuthController.Login` y emisión/validación de JWT (unit + integración).
  2. Autorización por rol de cada endpoint (integración) — habría detectado S1.
  3. `TimeOffRequestValidator` (fechas pasadas, inicio>fin, solapamiento) — lógica de negocio pura, ideal para unit tests.
  4. `requestConverters` y el contrato enum↔número (unit).
  5. Flujo login→dashboard→crear solicitud (e2e) — habría detectado la regresión.

---

## FASE 9 — Deuda Técnica

> Catálogo completo en [TECH_DEBT_REPORT.md](TECH_DEBT_REPORT.md).

| Hallazgo | Impacto | Riesgo | Prioridad |
| -------- | ------- | ------ | --------- |
| `UsersController` sin autorización | Escalada de privilegios total | Compromiso completo | **Crítico** |
| Secreto JWT en repositorio | Falsificación de tokens | Suplantación de cualquier usuario | **Crítico** |
| Regresión working tree (login `userId`) | Empleado no puede crear solicitudes | Funcionalidad rota | **Crítico** |
| SQLite sobre `emptyDir` en k8s | Pérdida de datos al reiniciar | Pérdida total | **Crítico** |
| `EnsureCreated()` vs migraciones | Esquema divergente / deploy roto | Inconsistencia | **Alto** |
| Sin índice único en `Email` | Cuentas duplicadas, login ambiguo | Integridad | **Alto** |
| Sin tests (0%) | Regresiones silenciosas | Alto | **Alto** |
| CORS `*` + token en localStorage + sin HTTPS | Exposición de tokens | Robo de sesión | **Alto** |
| .NET 6 fuera de soporte | Sin parches de seguridad | Vulnerabilidades sin fix | **Alto** |
| Código muerto (`UserService`, `Task*`, `FilterBar`, `useApi`) | Confusión, mantenimiento | Bajo | **Medio** |
| `bin/`, `obj/`, `*.dll`, `*.pdb`, `timeoff.db` commiteados | Ruido en repo, binarios en VCS | Bajo | **Medio** |
| Mapeo manual enum↔número (frontend) | Acoplamiento frágil al orden ordinal | Bug silencioso | **Medio** |
| Idiomas mezclados, `console.log` de token | Mantenibilidad / fuga en logs | Bajo-Medio | **Medio** |
| README desactualizado (endpoints inexistentes) | Onboarding erróneo | Bajo | **Bajo** |

---

## FASE 10 — Bugs

> Detalle, evidencia y solución en [TECH_DEBT_REPORT.md](TECH_DEBT_REPORT.md#catálogo-de-bugs).

### Bugs Confirmados

- **B1 — Regresión de login sin commitear (Crítico).** `git diff` muestra en [authService.ts:27](frontend/src/api/authService.ts#L27) el cambio `id: response.data.userId` → `id: response.data.id`. El backend devuelve `userId` ([AuthController.cs:39](backend/TimeOffManager/Controllers/AuthController.cs#L39)), no `id`, por lo que `id` queda `undefined`. En [useLogin.ts:44](frontend/src/pages/Login/useLogin.ts#L44) se eliminó `.toString()` y se guarda `undefined` → `localStorage` almacena la cadena `"undefined"`. En [EmployeeDashboard.tsx:77-78](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx#L77-L78), `Number("undefined")` = `NaN`, `if (!userId) throw 'User not authenticated'` → **el empleado no puede crear solicitudes**. *Solución:* revertir a `userId`/`.toString()` y, mejor, derivar el id del token, no de `localStorage`.

- **B2 — `exp` del JWT con hora local (Alto).** [JwtService.cs:30](backend/TimeOffManager/Services/JwtService.cs#L30) usa `DateTime.Now.AddHours(4)`; el `exp` se interpreta como UTC. En servidores con offset, la expiración real se desvía (p. ej. UTC-5 ⇒ token válido ~9h o ~ -1h). *Solución:* `DateTime.UtcNow.AddHours(4)`.

- **B3 — Ruta de imagen rota en producción (Medio).** [LoginPage.tsx:24](frontend/src/pages/Login/LoginPage.tsx#L24) usa `src="../src/img/nwoork.png"`; tras `vite build` ese path no existe → logo roto. *Solución:* `import logo from '../../img/nwoork.png'`.

- **B4 — `authService.logout` limpia claves equivocadas (Bajo).** Working tree borra `'id'` ([authService.ts:37](frontend/src/api/authService.ts#L37)) en vez de `'userId'`/`'userRole'`, dejando datos huérfanos.

- **B5 — `.env` apunta a puerto erróneo (Medio, dev).** [frontend/.env](frontend/.env) usa `http://127.0.0.1:8000/api` mientras el backend corre en `:5000` ([.env.example](frontend/.env.example)). Quien use el `.env` presente verá fallar todas las llamadas.

- **B6 — `userService.getByRole` llama endpoint inexistente (Bajo).** [userService.ts:86](frontend/src/api/userService.ts#L86) hace `GET /users/role/${role}`, ruta que el backend no expone → siempre 404/405. Método muerto.

### Bugs Potenciales

- **BP1 — Over-posting (Medio).** `POST /TimeOffRequests` recibe la entidad `TimeOffRequest` completa, incluida la navegación `User`; un cliente podría enviar un grafo `User` y EF intentar insertarlo. Aunque se sobreescriben `UserId/Status/CreatedAt`, debe usarse un DTO de entrada.
- **BP2 — `EnsureCreated` + migraciones (Alto).** En una BD nueva, `EnsureCreated()` crea el esquema desde el modelo **sin** `__EFMigrationsHistory`; un posterior `dotnet ef database update` fallará o divergerá.
- **BP3 — Actualización optimista con estado obsoleto (Bajo).** [useAdminDashboard.ts:28-41](frontend/src/pages/AdminDashboard/useAdminDashboard.ts#L28-L41) usa `requests` capturado en cierre en vez de updater funcional; en errores concurrentes puede revertir a un estado obsoleto.
- **BP4 — Emails duplicados (Alto).** Sin unicidad, dos registros con el mismo email rompen el login determinista.

---

## FASE 11 — Oportunidades de Mejora

> Plan detallado en [IMPROVEMENT_ROADMAP.md](IMPROVEMENT_ROADMAP.md).

- **Quick Wins (1–2 días):** añadir `[Authorize(Roles="Admin")]` a `UsersController`; mover el secreto JWT a variables de entorno/secret; revertir la regresión B1; índice único en `Email`; eliminar `console.log` de tokens; corregir `DateTime.UtcNow` en el JWT; borrar código muerto.
- **Corto plazo:** capa de servicios/validación desacoplada de MVC; DTO de entrada para requests; serializar enums como string (`JsonStringEnumConverter`); CORS restringido; HTTPS; rate limiting en login; suite de tests inicial; CI básico.
- **Mediano plazo:** migrar de SQLite a PostgreSQL; refresh tokens; `FluentValidation`; manejo de errores centralizado (middleware + `ProblemDetails`); observabilidad (Serilog + health checks).
- **Largo plazo / Empresa:** RBAC más granular, auditoría, multi-tenant, paginación/filtros server-side, feature flags, despliegue con secretos gestionados (Vault/Sealed Secrets), HPA.

---

## FASE 12 — Evaluación Arquitectónica (justificación de notas)

- **Arquitectura 5/10:** hay intención de capas y DTOs, pero la lógica de negocio vive en controllers, el `UserService` está muerto y los validadores acoplan a MVC. No hay frontera de dominio.
- **Backend 5/10:** sólido en lo básico (BCrypt, roles en requests, autoridad en servidor), penalizado por la brecha de authz en Users, `EnsureCreated` vs migraciones, over-posting y transacciones innecesarias.
- **Frontend 5/10:** buena separación hooks/UI y TypeScript tipado, pero token en localStorage, código muerto, regresión de login, mapeo de enums frágil y asset roto en build.
- **Seguridad 2/10:** un endpoint anónimo permite crear Admins y borrar usuarios; el secreto de firma está en el repo; CORS abierto y sin HTTPS. Defectos de severidad crítica.
- **Base de Datos 3/10:** modelo simple correcto, pero sin unicidad de email, cascada destructiva, fechas inconsistentes y persistencia efímera en k8s.
- **Testing 0/10:** sin un solo test; regresión real no detectada.
- **Performance 6/10:** sin N+1 y con `AsNoTracking`; resta por queries/transacciones redundantes y falta de paginación.
- **Mantenibilidad 4/10:** código muerto, duplicación, idiomas mezclados, logs de depuración y README divergente.
- **Escalabilidad 3/10:** SQLite mono-escritor, `emptyDir`, `replicas:1`, sin paginación ni ruta de escalado.
- **Calidad General 4/10:** prototipo competente y amplio, pero con bloqueantes de seguridad/datos que exigen trabajo antes de producción.

---

## FASE 13 — Roadmap Recomendado (resumen)

> Versión completa y priorizada por riesgo/impacto/esfuerzo en [IMPROVEMENT_ROADMAP.md](IMPROVEMENT_ROADMAP.md).

- **Próximas 24 h (detener el sangrado):** autorizar `UsersController`; sacar el secreto JWT del repo y rotarlo; revertir B1; índice único en email; quitar logs de token.
- **Próxima semana:** CORS restringido + HTTPS; DTO de entrada + serialización de enums string; rate limiting; primera tanda de tests (auth, authz, validador); CI con build+lint+test; limpiar repo (`.gitignore`, dejar de versionar `bin/obj/*.db`).
- **Próximo mes:** PostgreSQL + `Migrate()` en arranque; refresh tokens + logout server-side; `FluentValidation`; middleware de errores con `ProblemDetails`; Serilog + health checks; corregir k8s (volumen persistente, probes, límites, secrets).
- **Próximos 3 meses:** RBAC granular + auditoría, paginación/filtrado server-side, e2e (Playwright), HPA y secretos gestionados, eventual flujo de auto-registro seguro si el negocio lo requiere.

---

## Anexo — Inventario de evidencia (archivos clave)

| Área | Archivo |
| --- | --- |
| Bootstrap / Auth / CORS / JWT | [Program.cs](backend/TimeOffManager/Program.cs) |
| Login | [AuthController.cs](backend/TimeOffManager/Controllers/AuthController.cs) |
| CRUD usuarios (sin authz) | [UsersController.cs](backend/TimeOffManager/Controllers/UsersController.cs) |
| Solicitudes | [TimeOffRequestsController.cs](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs) |
| Emisión JWT | [JwtService.cs](backend/TimeOffManager/Services/JwtService.cs) |
| Servicio muerto | [UserService.cs](backend/TimeOffManager/Services/UserService.cs) |
| Validación negocio | [TimeOffRequestValidator.cs](backend/TimeOffManager/Validators/TimeOffRequestValidator.cs) |
| Secreto JWT | [appsettings.json](backend/TimeOffManager/appsettings.json) |
| Esquema/índices | [AppDbContextModelSnapshot.cs](backend/TimeOffManager/Migrations/AppDbContextModelSnapshot.cs) |
| Cliente HTTP / token | [apiClient.ts](frontend/src/api/apiClient.ts) |
| Regresión login | [useLogin.ts](frontend/src/pages/Login/useLogin.ts), [authService.ts](frontend/src/api/authService.ts) |
| Flujo empleado | [EmployeeDashboard.tsx](frontend/src/pages/EmployeeDashboard/EmployeeDashboard.tsx) |
| Infra | [k8s/backend-deployment.yaml](k8s/backend-deployment.yaml), [Dockerfile](backend/TimeOffManager/Dockerfile) |
