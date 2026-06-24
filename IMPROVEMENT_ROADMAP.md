# ROADMAP DE MEJORAS — TimeOff Manager

> Complemento de [AUDIT_REPORT.md](AUDIT_REPORT.md). Plan de evolución priorizado por **riesgo · impacto · esfuerzo**, desde el "detener el sangrado" hasta la escalabilidad empresarial.

**Leyenda de esfuerzo:** S = horas · M = 1–2 días · L = 3–5 días · XL = semanas.

---

## 1. Quick Wins (1–2 días) — máximo impacto, mínimo esfuerzo

| # | Acción | Esfuerzo | Resuelve | Por qué primero |
| --- | --- | --- | --- | --- |
| Q1 | Añadir `[Authorize(Roles="Admin")]` a `UsersController` | S | [S1](SECURITY_REVIEW.md) Crítico | Cierra la escalada de privilegios anónima |
| Q2 | Mover clave JWT a env/secret, **rotarla**, exigir ≥256 bits, sin fallback | S–M | [S2](SECURITY_REVIEW.md) Crítico | Anula la falsificación de tokens |
| Q3 | Revertir la regresión de login (`userId`) | S | [B1](TECH_DEBT_REPORT.md) Crítico | Restaura la creación de solicitudes |
| Q4 | Índice **único** en `Email` + normalización a minúsculas | S | [TD-06](TECH_DEBT_REPORT.md) Alto | Login determinista, sin duplicados |
| Q5 | Eliminar `console.log` de token/PII | S | [S5](SECURITY_REVIEW.md) Alto | Reduce fuga por logs |
| Q6 | `DateTime.UtcNow` en el `exp` del JWT | S | [B2](TECH_DEBT_REPORT.md) Alto | Expiración consistente |
| Q7 | Restringir CORS a orígenes conocidos | S | [S4](SECURITY_REVIEW.md) Alto | Reduce superficie |
| Q8 | `.gitignore` raíz + `git rm --cached` de `bin/ obj/ *.db *.pdb` | S | [TD-12](TECH_DEBT_REPORT.md) Medio | Repo limpio, sin churn binario |
| Q9 | Borrar código muerto (`Task*`, `FilterBar`, `useApi`, `getByRole`, `UserService` o usarlo) | S–M | [TD-11](TECH_DEBT_REPORT.md) Medio | Menos confusión |
| Q10 | Corregir asset del logo (`import`) y alinear `.env` al puerto real | S | [B3](TECH_DEBT_REPORT.md), [B5](TECH_DEBT_REPORT.md) | Producción/dev funcionales |

**Resultado esperado:** se eliminan **los 3 bloqueantes de código** y los riesgos altos más baratos. La app deja de ser trivialmente comprometible.

---

## 2. Mejoras a Corto Plazo (próxima semana)

| # | Acción | Esfuerzo | Referencia |
| --- | --- | --- | --- |
| C1 | DTO de entrada para `POST /TimeOffRequests` (evitar over-posting) | M | [S9](SECURITY_REVIEW.md) |
| C2 | Serializar enums como **string** en toda la API (`JsonStringEnumConverter`) y eliminar `requestConverters` | M | [TD-13](TECH_DEBT_REPORT.md), [A-F2](ARCHITECTURE_ANALYSIS.md) |
| C3 | Endpoint `POST /api/auth/register` que **fuerza `Role=Employee`** | M | [S3](SECURITY_REVIEW.md) |
| C4 | Validación server-side: email válido/único, contraseña con política mínima (Data Annotations o FluentValidation) | M | [S8](SECURITY_REVIEW.md) |
| C5 | HTTPS: `UseHttpsRedirection` + HSTS + `RequireHttpsMetadata=true` en prod; TLS en Ingress | M | [S6](SECURITY_REVIEW.md) |
| C6 | Rate limiting + bloqueo de cuenta en login | M | [S7](SECURITY_REVIEW.md) |
| C7 | **Suite de tests inicial:** auth, autorización por rol (habría detectado S1), `TimeOffRequestValidator`, `requestConverters` | L | [TD-07](TECH_DEBT_REPORT.md) |
| C8 | **CI** (GitHub Actions): `dotnet build/test` + `npm ci/lint/build` en cada PR | M | [TD-21](TECH_DEBT_REPORT.md) |
| C9 | Reemplazar `EnsureCreated()` por `Migrate()`; verificar coherencia de migraciones | M | [BP2](TECH_DEBT_REPORT.md) |
| C10 | Identidad del frontend desde el **token** (claims) en vez de `localStorage` | M | [A-F1](ARCHITECTURE_ANALYSIS.md) |

**Resultado esperado:** API endurecida, contrato de datos robusto, red de seguridad de tests + CI, despliegue de esquema fiable.

---

## 3. Mejoras a Mediano Plazo (próximo mes)

| # | Acción | Esfuerzo | Referencia |
| --- | --- | --- | --- |
| M1 | **Migrar SQLite → PostgreSQL** (o SQL Server); cadena de conexión por secret | L | [Base de Datos](ARCHITECTURE_ANALYSIS.md#5-infraestructura) |
| M2 | Capa de **servicios de aplicación** + repositorios; sacar lógica de controllers | L | [A-B3](ARCHITECTURE_ANALYSIS.md) |
| M3 | **FluentValidation** con `ValidationFilter` (desacoplar validación de MVC) | M | [TD-14](TECH_DEBT_REPORT.md) |
| M4 | **Middleware de manejo de errores** → `ProblemDetails` consistente; quitar `Console.WriteLine` | M | [TD-22](TECH_DEBT_REPORT.md) |
| M5 | **Refresh tokens** rotatorios + logout server-side (revocación por `jti`) | L | [S10](SECURITY_REVIEW.md) |
| M6 | Observabilidad: **Serilog** estructurado + `/health` (health checks) | M | — |
| M7 | Corregir **Kubernetes**: volumen persistente (PVC), `Secret` para JWT/DB, probes liveness/readiness, `resources` limits, Ingress+TLS, imágenes versionadas | L | [Infra](ARCHITECTURE_ANALYSIS.md#52-kubernetes) |
| M8 | Cambiar FK a `OnDelete: Restrict` + **soft-delete** de usuarios (preservar auditoría) | M | [TD-10](TECH_DEBT_REPORT.md) |
| M9 | Actualizar runtime: **.NET 8 LTS** (desde .NET 6 EOL) | M | [TD-08](TECH_DEBT_REPORT.md) |
| M10 | `docker-compose.yml` para entorno local completo (api+db+front) | S–M | — |

**Resultado esperado:** base de datos productiva, arquitectura por capas testeable, sesiones seguras, infraestructura resiliente y runtime soportado.

---

## 4. Mejoras a Largo Plazo (próximos 3 meses)

| # | Acción | Esfuerzo |
| --- | --- | --- |
| L1 | **Paginación, filtrado y ordenación server-side** en listados (requests/users) | L |
| L2 | **Auditoría** completa: quién aprobó/rechazó, `UpdatedAt`, historial de cambios | L |
| L3 | Tests **e2e** (Playwright) del flujo login→dashboard→crear/aprobar | L |
| L4 | Notificaciones (email/in-app) al cambiar estado de una solicitud | L |
| L5 | Cobertura de tests objetivo ≥ 70% en dominio/casos de uso | XL |
| L6 | Internacionalización (i18n) real; eliminar mezcla es/en | M |
| L7 | Gestión de saldos de días (balance de vacaciones, acumulación, política por tipo) | XL |

---

## 5. Escalabilidad Empresarial

| Capacidad | Descripción | Esfuerzo |
| --- | --- | --- |
| E1 | **RBAC granular** y políticas (p. ej. Manager que aprueba solo a su equipo) + claims/policies en .NET | XL |
| E2 | **Multi-tenant** (organizaciones/equipos) con aislamiento de datos | XL |
| E3 | Secretos gestionados (HashiCorp **Vault** / Sealed Secrets / cloud KMS) y rotación automática | L |
| E4 | **HPA** (Horizontal Pod Autoscaler) — viable solo tras migrar de SQLite | M |
| E5 | API gateway + rate limiting centralizado + WAF | L |
| E6 | Observabilidad completa: métricas (Prometheus), trazas (OpenTelemetry), dashboards (Grafana), alertas | L |
| E7 | Pipeline CD con entornos (dev/stage/prod), aprobaciones y rollback | L |
| E8 | Feature flags + despliegues canary/blue-green | L |
| E9 | Cumplimiento: retención/borrado de datos (GDPR), cifrado en reposo, registros de auditoría inmutables | XL |

---

## 6. Cronograma sugerido (Fase 13)

### Próximas 24 horas — *detener el sangrado*
- Q1 (authz Users), Q2 (rotar/securizar secreto JWT), Q3 (revertir regresión), Q4 (email único), Q5 (quitar logs de token).
- **Criterio de salida:** no existe ruta anónima a Admin; el secreto ya no está en el repo; login y creación de solicitudes funcionan.

### Próxima semana
- C1–C10: API endurecida (DTOs, enums string, register seguro, CORS/HTTPS, rate limiting), `Migrate()`, primera suite de tests y CI.
- **Criterio de salida:** pipeline verde con tests de authz; despliegue de esquema reproducible.

### Próximo mes
- M1–M10: PostgreSQL, capa de servicios, FluentValidation, manejo de errores, refresh tokens, observabilidad, k8s correcto, .NET 8.
- **Criterio de salida:** entorno de staging estable con datos persistentes y sesiones seguras.

### Próximos 3 meses
- L1–L7 + arranque de E1–E9 según necesidad de negocio.
- **Criterio de salida:** producto escalable, auditable y operable, con cobertura de pruebas significativa.

---

## 7. Matriz de priorización (riesgo × impacto × esfuerzo)

| Prioridad | Acciones | Justificación |
| --- | --- | --- |
| **P0 — Inmediato** | Q1, Q2, Q3, Q4 | Riesgo crítico (seguridad/datos/funcionalidad), esfuerzo mínimo |
| **P1 — Esta semana** | Q5–Q10, C5, C6, C7, C8, C9 | Alto impacto en seguridad y calidad, esfuerzo bajo-medio |
| **P2 — Este mes** | C1–C4, C10, M1–M9 | Robustez estructural y productiva |
| **P3 — Trimestre** | L1–L7 | Madurez de producto |
| **P4 — Estratégico** | E1–E9 | Escala empresarial bajo demanda de negocio |
