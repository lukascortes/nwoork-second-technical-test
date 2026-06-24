# REVISIÓN DE SEGURIDAD — TimeOff Manager

> Complemento de [AUDIT_REPORT.md](AUDIT_REPORT.md). Análisis de seguridad por componente, mapeo a OWASP Top 10 (2021), con evidencia, prueba de concepto, severidad y remediación concreta.

**Resumen:** **2 Críticas, 4 Altas, 4 Medias.** Existe una ruta directa a **compromiso total** del sistema desde un cliente anónimo. **No desplegar a producción** hasta resolver S1 y S2.

| ID | Vulnerabilidad | OWASP 2021 | Severidad |
| --- | --- | --- | --- |
| S1 | CRUD de usuarios sin autenticación/autorización | A01 Broken Access Control | 🔴 **Crítica** |
| S2 | Secreto de firma JWT en el repositorio + fallback débil | A02 Cryptographic Failures / A05 | 🔴 **Crítica** |
| S3 | Diseño de alta de usuarios inseguro (sin registro real) | A04 Insecure Design | 🟠 Alta |
| S4 | CORS totalmente permisivo | A05 Security Misconfiguration | 🟠 Alta |
| S5 | JWT en `localStorage` + token logueado en consola | A07 Auth Failures (XSS) | 🟠 Alta |
| S6 | Transporte sin TLS forzado | A02 Cryptographic Failures | 🟠 Alta |
| S7 | Sin rate limiting ni bloqueo de cuenta en login | A07 Auth Failures | 🟡 Media |
| S8 | Sin política de contraseñas ni validación de email server-side | A04 Insecure Design | 🟡 Media |
| S9 | Over-posting / mass assignment al crear solicitudes | A08 Integrity Failures | 🟡 Media |
| S10 | Sin refresh tokens / revocación; `exp` con hora local | A07 Auth Failures | 🟡 Media |

---

## S1 — CRUD de usuarios sin autorización 🔴 CRÍTICA

**OWASP:** A01:2021 Broken Access Control.
**Evidencia:** [UsersController.cs:12-14](backend/TimeOffManager/Controllers/UsersController.cs#L12-L14) — el controller declara `[ApiController]` y `[Route("api/[controller]")]` **sin `[Authorize]`** a nivel de clase ni de método. Comparar con [TimeOffRequestsController.cs:26-27](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L26-L27) que sí usa `[Authorize(Roles=...)]`.

Endpoints expuestos a **cualquier anónimo**:
- `GET /api/Users` → enumeración de todos los usuarios (emails + roles).
- `GET /api/Users/{id}` → detalle de cualquier usuario.
- `POST /api/Users` → **crear usuario con `Role=Admin`** ([UsersController.cs:59-66](backend/TimeOffManager/Controllers/UsersController.cs#L59-L66)).
- `PUT /api/Users/{id}` → cambiar email/contraseña/rol de cualquiera (toma de cuenta).
- `DELETE /api/Users/{id}` → borrar cualquier usuario (y por cascada, sus solicitudes).

**Prueba de concepto (sin token):**
```bash
# 1) Crear un administrador
curl -X POST http://localhost:5000/api/Users \
  -H "Content-Type: application/json" \
  -d '{"email":"attacker@evil.com","password":"x","role":0}'   # role 0 = Admin

# 2) Login con esa cuenta → JWT de Admin → control total
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"attacker@evil.com","password":"x"}'

# (alternativa destructiva) borrar usuarios:
curl -X DELETE http://localhost:5000/api/Users/1
```

**Impacto:** Compromiso total de confidencialidad, integridad y disponibilidad. Escalada de privilegios a Admin, robo/secuestro de cualquier cuenta, borrado masivo de datos (la cascada elimina solicitudes).
**Riesgo:** Máximo. Es la vulnerabilidad de mayor prioridad del proyecto.

**Remediación:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]      // ← toda la gestión de usuarios solo para Admin
public class UsersController : ControllerBase { ... }
```
- Si se requiere auto-registro, hacerlo en un endpoint **dedicado** (`POST /api/auth/register`) que **fuerce `Role=Employee`** y nunca acepte el rol del cliente (ver S3/S8).
- Añadir test de integración que verifique `401/403` sin token/rol.

---

## S2 — Secreto de firma JWT en el repositorio 🔴 CRÍTICA

**OWASP:** A02:2021 Cryptographic Failures / A05 Misconfiguration.
**Evidencia:**
- [appsettings.json:3](backend/TimeOffManager/appsettings.json#L3): `"Key": "WZ9XPnWY7g3CsVOYPkIzjfsr9VgqU38T"` — clave HS256 **commiteada en texto plano** (y replicada en `bin/.../appsettings.json`, también versionado).
- [Program.cs:50](backend/TimeOffManager/Program.cs#L50): fallback hardcodeado `?? "clave_jwt_por_defecto_123456"`.

**Impacto:** Cualquiera con acceso al repo (o al historial Git) puede **firmar tokens arbitrarios** para cualquier `userId`/`role`, suplantando a cualquier usuario o Admin sin credenciales. Equivale a una llave maestra filtrada.

**Problemas adicionales:**
- El fallback `clave_jwt_por_defecto_123456` tiene 28 bytes (224 bits) < 256 bits que exige HS256 → si llegara a usarse, el firmado lanzaría `IDX10653`. Es un secreto débil y predecible.
- **Inconsistencia de codificación:** la validación usa `Encoding.ASCII.GetBytes` ([Program.cs:51](backend/TimeOffManager/Program.cs#L51)) y la firma `Encoding.UTF8.GetBytes` ([JwtService.cs:23](backend/TimeOffManager/Services/JwtService.cs#L23)). Funciona por azar (la clave es ASCII puro) pero es frágil.
- `ValidateIssuer = false` y `ValidateAudience = false` ([Program.cs:66-67](backend/TimeOffManager/Program.cs#L66-L67)) pese a tener `Issuer`/`Audience` configurados → no se validan.

**Remediación:**
- Mover la clave a **variable de entorno / secret manager** (User Secrets en dev, env var / Kubernetes `Secret` / Vault en prod). **Rotar** la clave actual de inmediato (ya está comprometida por estar en el historial).
- Exigir longitud ≥ 32 bytes y fallar el arranque si falta (no usar fallback).
- Unificar codificación (UTF-8) y **activar** `ValidateIssuer`/`ValidateAudience` con los valores configurados.
```csharp
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurada");
// ValidateIssuer = true, ValidIssuer = cfg["Jwt:Issuer"], idem Audience
```

---

## S3 — Diseño de alta de usuarios inseguro 🟠 ALTA

**OWASP:** A04:2021 Insecure Design.
**Evidencia:** El README anuncia `POST /api/Auth/register`, pero `AuthController` solo expone `login` ([AuthController.cs](backend/TimeOffManager/Controllers/AuthController.cs)). El único alta es el `POST /api/Users` anónimo de S1, que **acepta el rol del cliente**.
**Impacto:** El "registro" de facto permite auto-asignarse `Admin`. Incluso corrigiendo S1, si el alta admite `role`, persiste el riesgo de escalada.
**Remediación:** Endpoint `register` dedicado, anónimo pero que **ignore** cualquier rol entrante y fije `Role=Employee`; la creación de Admins solo vía endpoint protegido por Admin.

---

## S4 — CORS totalmente permisivo 🟠 ALTA

**OWASP:** A05:2021 Security Misconfiguration.
**Evidencia:** [Program.cs:37-42](backend/TimeOffManager/Program.cs#L37-L42) — política `AllowAll` con `AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`.
**Impacto:** Cualquier sitio web puede invocar la API desde el navegador de la víctima. Como la auth es Bearer (no cookies), no hay envío automático de credenciales, pero combinado con un token robado por XSS (S5) facilita su uso desde cualquier origen y degrada la postura defensiva.
**Remediación:** Lista blanca de orígenes por entorno:
```csharp
policy.WithOrigins(allowedOrigins)   // desde configuración
      .AllowAnyHeader().AllowAnyMethod();
```

---

## S5 — JWT en `localStorage` + token en logs 🟠 ALTA

**OWASP:** A07:2021 Identification & Authentication Failures (vector XSS).
**Evidencia:**
- [apiClient.ts:10-13](frontend/src/api/apiClient.ts#L10-L13) lee `localStorage.getItem('token')`.
- [useAuth.tsx:29](frontend/src/hooks/useAuth.tsx#L29) y [useLogin.ts:42](frontend/src/pages/Login/useLogin.ts#L42) guardan el token en `localStorage`.
- [useLogin.ts:39](frontend/src/pages/Login/useLogin.ts#L39): `console.log('Token recibido:', response.token)` — **el JWT se escribe en consola** (y queda en logs del navegador / herramientas de monitoreo).

**Impacto:** Un único XSS permite exfiltrar el token (accesible a todo JS). El logging del token amplía la superficie (capturas, extensiones, agregadores de logs).
**Remediación:**
- Preferir **cookie `HttpOnly` + `Secure` + `SameSite`** para el token (con protección CSRF asociada), o como mínimo minimizar exposición y vida del token.
- **Eliminar** todos los `console.log` de credenciales/tokens/PII.
- Endurecer contra XSS: CSP en nginx, evitar `dangerouslySetInnerHTML` (hoy no se usa — bien).

---

## S6 — Transporte sin TLS forzado 🟠 ALTA

**OWASP:** A02:2021.
**Evidencia:** [Program.cs:60](backend/TimeOffManager/Program.cs#L60) `RequireHttpsMetadata = false`; no hay `app.UseHttpsRedirection()`; `ASPNETCORE_URLS=http://+:5000` ([Dockerfile:20](backend/TimeOffManager/Dockerfile#L20)); k8s expone NodePort sin TLS.
**Impacto:** Credenciales y JWT viajan en claro; susceptibles a interceptación (MITM) en red no confiable.
**Remediación:** TLS terminado en Ingress/reverse-proxy; `UseHttpsRedirection`+HSTS; `RequireHttpsMetadata=true` en producción.

---

## S7 — Sin rate limiting ni bloqueo de cuenta 🟡 MEDIA

**OWASP:** A07:2021.
**Evidencia:** `AuthController.Login` no aplica throttling ni contador de intentos. BCrypt (factor por defecto 11) ralentiza, pero no impide fuerza bruta/credential stuffing distribuido.
**Impacto:** Ataques de fuerza bruta sobre contraseñas y enumeración.
**Remediación:** Rate limiting (`AddRateLimiter` en .NET 7+, o middleware/Ingress en .NET 6), bloqueo temporal tras N fallos, y respuestas/timing uniformes para no revelar existencia de cuentas.

---

## S8 — Sin política de contraseñas ni validación server-side 🟡 MEDIA

**OWASP:** A04:2021.
**Evidencia:** `UserCreateDto`/`UserUpdateDto` ([UserDtos.cs](backend/TimeOffManager/DTOs/UserDtos.cs)) y `LoginRequest` no tienen `[Required]`, `[EmailAddress]`, `[MinLength]`. Se acepta contraseña vacía o `"x"` (ver PoC de S1). El email no se valida en servidor.
**Impacto:** Cuentas con credenciales triviales; emails inválidos/duplicados (ver A-D1).
**Remediación:** Data Annotations o FluentValidation: email válido y único, contraseña con longitud/complejidad mínima, normalización de email.

---

## S9 — Over-posting al crear solicitudes 🟡 MEDIA

**OWASP:** A08:2021 Software & Data Integrity Failures.
**Evidencia:** [TimeOffRequestsController.cs:54](backend/TimeOffManager/Controllers/TimeOffRequestsController.cs#L54) — `Create(TimeOffRequest request)` bindea la **entidad completa**, incluida la navegación `User` y `Id`. Aunque luego sobreescribe `UserId/Status/CreatedAt` (defensa correcta), enviar un grafo `User` podría provocar inserciones no deseadas vía tracking de EF.
**Impacto:** Manipulación de integridad del modelo; potencial creación de entidades laterales.
**Remediación:** DTO de entrada estricto (`CreateTimeOffRequestDto { StartDate, EndDate, Type, Reason }`) y mapear explícitamente.

**Nota positiva:** El control de autoridad ya existe: el servidor ignora `userId`/`status` del cliente y los fija desde el token. Mantenerlo.

---

## S10 — Gestión de sesión: sin refresh/revocación, `exp` local 🟡 MEDIA

**OWASP:** A07:2021.
**Evidencia:** [JwtService.cs:30](backend/TimeOffManager/Services/JwtService.cs#L30) `expires: DateTime.Now.AddHours(4)` (hora **local**, no UTC → expiración desviada según offset del servidor, ver bug B2). No hay refresh tokens; el `logout` solo limpia `localStorage` ([authService.ts:34-38](frontend/src/api/authService.ts#L34-L38)), por lo que un token robado sigue válido hasta expirar (no hay revocación server-side).
**Impacto:** Ventana de uso de tokens robados; imposibilidad de cerrar sesión real; expiración impredecible entre entornos.
**Remediación:** Usar `DateTime.UtcNow`; access token corto + refresh token rotatorio con almacenamiento server-side y revocación; lista de revocación / `jti` para logout efectivo.

---

## Aspectos positivos de seguridad (a preservar)

- ✅ **BCrypt** para hashing de contraseñas ([UsersController.cs:64](backend/TimeOffManager/Controllers/UsersController.cs#L64)).
- ✅ Respuestas con **DTOs que no exponen `PasswordHash`** (`UserResponseDto`).
- ✅ **Autorización por rol** correctamente aplicada en `TimeOffRequestsController` (Employee/Admin).
- ✅ **Consultas EF parametrizadas** → sin inyección SQL.
- ✅ **Autoridad en servidor** al crear solicitudes (ignora `userId`/`status` del cliente).
- ✅ Auth **Bearer en header** (no cookies) → **CSRF no aplica** a la API.
- ✅ React **escapa la salida** por defecto; no se usa `dangerouslySetInnerHTML`.

---

## Plan de remediación priorizado

| Orden | Acción | Esfuerzo | Severidad resuelta |
| --- | --- | --- | --- |
| 1 | `[Authorize(Roles="Admin")]` en `UsersController` | 5 min | S1 (Crítica) |
| 2 | Sacar y **rotar** clave JWT (env/secret), exigir ≥256 bits, sin fallback | 1–2 h | S2 (Crítica) |
| 3 | Endpoint `register` que fuerza `Employee` | 1 h | S3 (Alta) |
| 4 | Restringir CORS por origen | 30 min | S4 (Alta) |
| 5 | Quitar logs de token; plan de cookie HttpOnly | 1 h / 1 día | S5 (Alta) |
| 6 | TLS + `UseHttpsRedirection` + HSTS | 0.5 día | S6 (Alta) |
| 7 | Rate limiting + lockout en login | 0.5 día | S7 |
| 8 | Validación de email/contraseña + unicidad | 0.5 día | S8 |
| 9 | DTO de entrada para solicitudes | 1 h | S9 |
| 10 | `exp` en UTC + refresh tokens | 1–2 días | S10 |
