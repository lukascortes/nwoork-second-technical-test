# ANÁLISIS DE ARQUITECTURA — TimeOff Manager

> Complemento de [AUDIT_REPORT.md](AUDIT_REPORT.md). Profundiza en arquitectura backend/frontend, modelo de datos e infraestructura, con evaluación de cohesión, acoplamiento, escalabilidad y mantenibilidad.

---

## 1. Vista de alto nivel

```
┌────────────────────────┐        HTTPS? (no)        ┌──────────────────────────────┐
│  Frontend (SPA)        │  ───────────────────────▶ │  Backend API (.NET 6)        │
│  React 19 + Vite       │   axios + Bearer JWT      │  ASP.NET Core MVC            │
│  nginx:alpine (prod)   │  ◀─────────────────────── │                              │
└────────────────────────┘        JSON               │  Controllers → DbContext     │
        │                                            │        │                     │
   localStorage(token)                               │   EF Core 6 (SQLite)         │
                                                     │        ▼                     │
                                                     │   timeoff.db (archivo)       │
                                                     └──────────────────────────────┘
```

- **Comunicación:** REST/JSON. Auth por **JWT Bearer** en header `Authorization` (no cookies).
- **Frontend** servido por nginx (SPA fallback a `index.html`).
- **Backend** monolítico, sin gateway, sin capa de caché, sin cola.

---

## 2. Arquitectura Backend

### 2.1 Estructura de carpetas (real)

```
backend/TimeOffManager/
├── Controllers/        Auth, Users, TimeOffRequests
├── Services/           JwtService (usado), UserService (MUERTO)
├── Validators/         TimeOffRequestValidator (estático, devuelve IActionResult)
├── DTOs/               TimeOffRequestDto, UserBasicDto  (ns: TimeOffManager.DTOs)
│   └── UserDtos.cs     UserCreate/Update/ResponseDto    (ns: TimeOffManager.Models.DTOs)  ← inconsistente
├── Models/             User, TimeOffRequest, Auth/LoginRequest
├── Data/               AppDbContext (anémico), timeoff.db
├── Migrations/         3 migraciones + snapshot
└── Program.cs          composición raíz (DI, auth, CORS, swagger)
```

### 2.2 Separación de responsabilidades y patrones

| Capa esperada | ¿Existe? | Observación |
| --- | --- | --- |
| Controllers | ✅ | Contienen orquestación **y** acceso a datos directo a `DbContext`. |
| Application/Service | ⚠️ Parcial/muerto | `UserService` replica `UsersController.CreateUser` y **no se inyecta** en ningún sitio. |
| Repository / UoW | ❌ | Se usa `DbContext` directamente como repositorio genérico. |
| Domain | ❌ | Entidades anémicas; reglas en validador estático. |
| DTOs | ✅ | Bien usados para respuestas; **falta DTO de entrada** en `POST /TimeOffRequests`. |
| Middlewares | ❌ | Solo los de framework (auth). Sin manejo de errores ni logging propio. |

**Patrones presentes:** Inyección de dependencias (DI nativa), DTO (parcial), Migrations.
**Patrones ausentes / mal aplicados:** Repository, Unit of Work, Mediator/CQRS (innecesario aquí, pero sí una capa de servicio limpia), Result pattern (el validador devuelve `IActionResult`, acoplando dominio a MVC).

### 2.3 Evaluación

- **Cohesión:** media. Los controllers mezclan HTTP, validación, transacciones y persistencia.
- **Acoplamiento:** **alto**. `TimeOffRequestValidator` depende de `Microsoft.AspNetCore.Mvc` (`BadRequestObjectResult`) y de `AppDbContext`; no es reutilizable fuera de un controller. Los controllers dependen de `DbContext` concreto.
- **Escalabilidad:** limitada por el motor de datos (ver §4) más que por el código.
- **Mantenibilidad:** penalizada por duplicación (`UserService` vs controller), dependencias muertas (`DbContextOptions` inyectado y no usado en ambos controllers) e idiomas mezclados.

### 2.4 Hallazgos arquitectónicos backend

- **A-B1 (Alto):** `Program.cs` usa `EnsureCreated()` con migraciones presentes → estrategias de esquema mutuamente excluyentes. Elegir **una**: en producción, `db.Database.Migrate()`.
- **A-B2 (Alto):** Validación acoplada a MVC. Extraer a un `Result`/excepción de dominio o adoptar `FluentValidation` con un `ValidationFilter`.
- **A-B3 (Medio):** Sin capa de servicio real → lógica de negocio (transacciones, reglas) embebida en controllers.
- **A-B4 (Medio):** `AppDbContext` no configura el modelo (`OnModelCreating` vacío): unicidad de email, índices y conversión de enums deberían declararse aquí.
- **A-B5 (Bajo):** Inconsistencia de namespaces de DTOs (`TimeOffManager.DTOs` vs `TimeOffManager.Models.DTOs`) y `UpdateStatusRequest` declarado dentro del archivo del controller.

### 2.5 Arquitectura objetivo (backend)

```
Controllers (HTTP)  →  Application Services (casos de uso)  →  Repositories / DbContext
                          │
                          ├─ FluentValidation (validadores POCO, sin MVC)
                          ├─ Domain entities con invariantes
                          └─ Result<T> / excepciones → middleware → ProblemDetails
Cross-cutting: ExceptionMiddleware, Serilog, AuthZ policies, AutoMapper (DTO↔entidad)
```

---

## 3. Arquitectura Frontend

### 3.1 Estructura (real)

```
frontend/src/
├── api/            apiClient (axios+interceptor), authService, userService, timeOffService
├── hooks/          useAuth (Context), useApi (MUERTO), useUserManagement
├── pages/          Login, AdminDashboard, EmployeeDashboard, admin/UsersPage, Unauthorized
│   └── */use*.ts   hook por página (buen patrón container/hook)
├── components/
│   ├── requests/   RequestForm (+ ilustraciones), RequestsTable
│   ├── admin/users/ UserForm, UserList, UserDetails, UserActions
│   ├── common/     ProtectedRoute, LoadingSpinner, ErrorMessage
│   ├── layout/     Navbar, AdminNavbar
│   └── Task*.tsx, FilterBar.tsx   ← CÓDIGO MUERTO (plantilla de to-do)
├── types/          authTypes, userTypes, requestTypes
├── utils/          requestConverters (mapeo enum↔número)
└── config.ts       API_BASE_URL (fallback :5000)
```

### 3.2 Componentes, hooks, routing, estado

- **Routing:** `react-router-dom` v7, rutas protegidas por `ProtectedRoute allowedRoles`. Catch-all → `/login`. Correcto.
- **Estado:** local con `useState`; **Context** solo para autenticación (`useAuth`). No hay store global (no se necesita a esta escala). El estado de auth se **rehidrata desde `localStorage`** en el init del provider.
- **Formularios:** dos enfoques conviviendo — Formik+Yup en `RequestForm` (✅) y `useState` manual en `UserForm`/`useRequestForm` (duplicación de patrón).
- **Validación:** Yup en cliente para fechas; reglas de solapamiento replicadas en cliente y servidor.

### 3.3 Evaluación

- **Reutilización:** `RequestsTable` se reusa en ambos dashboards (✅). `useApi` es un buen abstractor… **pero no se usa en ninguna parte** (muerto).
- **Escalabilidad (front):** sin paginación ni virtualización de tablas; carga total de datos. Aceptable hoy.
- **Organización:** buena salvo el código muerto y la mezcla de patrones de formulario.

### 3.4 Hallazgos arquitectónicos frontend

- **A-F1 (Alto):** Identidad derivada de `localStorage` (`userId`, `userRole`) en vez de decodificar el JWT. Cualquiera puede editar `localStorage.userRole='Admin'` y, aunque el backend valide el token, el **frontend** mostrará vistas de Admin (defensa solo cosmética). El `role` debe leerse del token (claims) y el backend es quien realmente autoriza.
- **A-F2 (Medio):** Contrato de serialización inconsistente con el backend obliga a `requestConverters` (mapeo manual a ordinales 0/1/2). Frágil ante reordenamiento de enums. Solución: que el backend serialice enums como **string** en todos los endpoints.
- **A-F3 (Medio):** `useLogin` hace navegación con `window.location.href` (recarga completa) en vez de `navigate()` de react-router, perdiendo el estado de SPA y forzando rehidratación.
- **A-F4 (Bajo):** Dos navbars casi idénticos (`Navbar`, `AdminNavbar`); candidatos a un único componente parametrizado.

---

## 4. Modelo de Datos

### 4.1 Esquema

```
┌───────────────────────────┐         ┌─────────────────────────────────────┐
│ Users                     │ 1     N │ TimeOffRequests                      │
├───────────────────────────┤────────▶├─────────────────────────────────────┤
│ Id            INTEGER PK   │         │ Id         INTEGER PK                │
│ Email         TEXT  NOT NULL│        │ UserId     INTEGER FK → Users.Id     │
│ PasswordHash  TEXT  NOT NULL│        │ StartDate  TEXT (DateTime)           │
│ Role          INTEGER       │        │ EndDate    TEXT (DateTime)           │
└───────────────────────────┘         │ Type       INTEGER (LeaveType)       │
   índices: solo PK                    │ Reason     TEXT NULL                 │
   ⚠ SIN índice único en Email         │ Status     INTEGER (RequestStatus)   │
                                       │ CreatedAt  TEXT (DateTime)           │
                                       └─────────────────────────────────────┘
                                         índice: IX_TimeOffRequests_UserId
                                         FK OnDelete: CASCADE
```

- Enums: `Role { Admin=0, Employee=1 }`, `LeaveType { Vacation=0, Sick=1, Other=2 }`, `RequestStatus { Pending=0, Approved=1, Rejected=2 }`. Persistidos como **enteros** (ordinal) → frágil ante reordenamiento.

### 4.2 Hallazgos de datos

| ID | Hallazgo | Riesgo | Prioridad |
| --- | --- | --- | --- |
| A-D1 | **Sin índice único en `Email`** | Cuentas duplicadas; login `FirstOrDefault` no determinista | **Alto** |
| A-D2 | Email **case-sensitive**, sin normalización | `User@x.com` ≠ `user@x.com` | Medio |
| A-D3 | FK `OnDelete: Cascade` | Borrar usuario destruye su historial (auditoría perdida) | Alto |
| A-D4 | `DateTime` como `TEXT` + mezcla local/UTC | Inconsistencias de zona horaria y comparación | Medio |
| A-D5 | Enums como entero ordinal | Acoplamiento al orden; ver A-F2 | Medio |
| A-D6 | Sin índices en `Status`/`CreatedAt`/fechas | Filtros/orden lentos a escala | Bajo |
| A-D7 | Sin columna de auditoría (`UpdatedAt`, quién aprobó) | Sin trazabilidad de decisiones | Medio |
| A-D8 | Sin soft-delete | Pérdida irreversible | Bajo |

### 4.3 Recomendaciones de modelo

```csharp
protected override void OnModelCreating(ModelBuilder b)
{
    b.Entity<User>(e => {
        e.HasIndex(u => u.Email).IsUnique();          // A-D1
        e.Property(u => u.Email).HasMaxLength(256);
        e.Property(u => u.Role).HasConversion<string>(); // A-D5 legibilidad
    });
    b.Entity<TimeOffRequest>(e => {
        e.HasOne(r => r.User).WithMany(u => u.Requests)
         .OnDelete(DeleteBehavior.Restrict);          // A-D3
        e.HasIndex(r => new { r.UserId, r.Status });
        e.Property(r => r.Type).HasConversion<string>();
        e.Property(r => r.Status).HasConversion<string>();
    });
}
```

Además: normalizar email a minúsculas antes de guardar/buscar; usar `DateTime.UtcNow` de forma consistente; añadir `UpdatedAt`/`ReviewedByUserId` para auditoría de aprobaciones.

---

## 5. Infraestructura

### 5.1 Docker

- **Backend `Dockerfile`:** multi-stage correcto, pero `COPY timeoff.db ./timeoff.db` **hornea la BD en la imagen** → escrituras efímeras y datos de seed embarcados. Falta usuario no-root, healthcheck y `ASPNETCORE_ENVIRONMENT=Production`.
- **Frontend `Dockerfile`:** build node:18 → nginx:alpine (✅), pero `npm install` (no `npm ci`) → builds no deterministas. `VITE_API_BASE_URL` se fija en build-time; no hay forma de reconfigurar el backend URL sin reconstruir.
- **Falta `docker-compose.yml`** para levantar ambos servicios con red común.

### 5.2 Kubernetes

| Hallazgo | Riesgo | Prioridad |
| --- | --- | --- |
| `emptyDir: {}` para SQLite | **Pérdida total de datos** al reiniciar el pod | **Crítico** |
| `replicas: 1` + SQLite mono-escritor | Sin alta disponibilidad ni escalado | Alto |
| Sin `resources.requests/limits` | Riesgo de OOM / vecino ruidoso | Medio |
| Sin `livenessProbe`/`readinessProbe` | Sin auto-recuperación | Medio |
| Secreto JWT no es `Secret` k8s | Secreto en imagen/config | Alto |
| `image: backend-app:latest` (sin registry, `latest`) | Despliegues no reproducibles | Medio |
| Solo `NodePort`, sin Ingress/TLS | Sin terminación TLS ni enrutado | Medio |

### 5.3 CI/CD y repositorio

- **Sin pipeline** (no `.github/workflows`). No hay build/test/lint automatizados.
- **Artefactos versionados:** `bin/`, `obj/`, `*.dll`, `*.pdb`, `timeoff.db`, `*.sqbpro` están **commiteados** (no hay `.gitignore` en la raíz/backend; el de frontend no cubre el backend). Genera ruido (aparecen como *modified* en `git status`) y mete binarios en VCS.
- **Recomendación:** `.gitignore` raíz con `bin/ obj/ *.db *.user`; dejar de versionar la BD; añadir CI (build + `dotnet test` + `npm run lint`/test).

---

## 6. Resumen de prioridades arquitectónicas

1. **Persistencia (Crítico):** PostgreSQL + volumen persistente; eliminar `EnsureCreated`, usar `Migrate()`.
2. **Capa de servicio + validación desacoplada (Alto):** sacar lógica de controllers; `FluentValidation`.
3. **Contrato de API (Alto):** serializar enums como string; DTO de entrada para requests; identidad desde el token en el front.
4. **Infra (Alto):** secrets gestionados, probes, límites, Ingress/TLS, CI/CD.
5. **Higiene (Medio):** eliminar código muerto, unificar namespaces, `.gitignore`, dejar de versionar binarios/BD.
