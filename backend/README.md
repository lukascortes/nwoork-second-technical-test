# TimeOff Manager — Backend (.NET 8, Clean Architecture)

[![CI](https://github.com/lukascortes/nwoork-second-technical-test/actions/workflows/ci.yml/badge.svg)](https://github.com/lukascortes/nwoork-second-technical-test/actions/workflows/ci.yml)

REST API for managing employee time-off requests, rebuilt from the ground up with
**.NET 8**, **Clean Architecture**, **PostgreSQL** and a security-first design. This
rewrite addresses every critical finding from the original technical audit
(see [../AUDIT_REPORT.md](../AUDIT_REPORT.md)).

## Architecture

```
TimeOffManager.Api            ← HTTP: controllers, auth, ProblemDetails, Swagger
        │
TimeOffManager.Application    ← use cases, DTOs, FluentValidation, ports (interfaces)
        │
TimeOffManager.Domain         ← entities with invariants + state machine (no dependencies)
        ↑
TimeOffManager.Infrastructure ← EF Core + PostgreSQL, repositories, JWT, BCrypt
```

The dependency rule points inward: `Domain` knows nothing about the outside world;
`Infrastructure` implements the interfaces declared in `Application`.

## Tech stack

- **.NET 8** · ASP.NET Core Web API
- **Entity Framework Core 8** + **Npgsql** (PostgreSQL)
- **JWT Bearer** authentication, **BCrypt** password hashing (work factor 12)
- **FluentValidation** · RFC 7807 **ProblemDetails** · built-in **rate limiting**
- **xUnit · FluentAssertions · NSubstitute** — 41 tests (unit + integration)

## Running it

### Option A — Docker Compose (recommended)

```bash
docker compose up --build        # from the repository root
```

- API → http://localhost:5000 (Swagger at `/swagger`, health at `/health`)
- PostgreSQL → localhost:5432 (data persisted in a named volume)

### Option B — Local dev

```bash
# 1) A PostgreSQL instance (e.g. via Docker)
docker run --name timeoff-db -e POSTGRES_USER=timeoff -e POSTGRES_PASSWORD=timeoff \
  -e POSTGRES_DB=timeoffmanager -p 5432:5432 -d postgres:16-alpine

# 2) Run the API (applies migrations + seeds demo data on startup)
dotnet run --project backend/src/TimeOffManager.Api
```

> The signing key and connection string come from configuration. For local dev they
> live in `appsettings.Development.json`; in production inject `Jwt__Key` and
> `ConnectionStrings__Default` from a secret store / environment variables.

## Demo credentials (seeded on first run)

| Role     | Email                | Password       |
| -------- | -------------------- | -------------- |
| Admin    | `admin@timeoff.dev`  | `Admin123!`    |
| Employee | `emma@timeoff.dev`   | `Employee123!` |
| Employee | `liam@timeoff.dev`   | `Employee123!` |

## API endpoints

| Method | Route                                | Auth        | Purpose                         |
| ------ | ------------------------------------ | ----------- | ------------------------------- |
| POST   | `/api/auth/register`                 | Anonymous   | Self-register (always Employee) |
| POST   | `/api/auth/login`                    | Anonymous   | Obtain a JWT                    |
| GET    | `/api/users`                         | Admin       | List users                      |
| GET    | `/api/users/{id}`                    | Admin       | Get a user                      |
| POST   | `/api/users`                         | Admin       | Create a user (any role)        |
| PUT    | `/api/users/{id}`                    | Admin       | Update a user                   |
| DELETE | `/api/users/{id}`                    | Admin       | Delete a user (not yourself)    |
| POST   | `/api/timeoffrequests`               | Employee    | Submit a request                |
| GET    | `/api/timeoffrequests/me`            | Employee    | List my requests                |
| GET    | `/api/timeoffrequests`               | Admin       | List all requests               |
| PUT    | `/api/timeoffrequests/{id}/status`   | Admin       | Approve / reject a request      |

## Testing

```bash
dotnet test backend/TimeOffManager.sln
```

- **Domain** unit tests — entity invariants & the request state machine
- **Application** unit tests — use cases with mocked ports
- **API** integration tests — the full HTTP stack via `WebApplicationFactory` over an
  in-memory SQLite database (no Docker required), covering auth, role authorization,
  secure registration and the request lifecycle.

## Security highlights (audit fixes)

- Every endpoint is authorized by role; user management is **Admin-only**.
- JWT signing key is required & validated at startup (≥ 256 bits, no insecure fallback);
  tokens validate issuer, audience, lifetime and signature, with expiry in **UTC**.
- Self-registration can never grant elevated roles.
- Request DTOs never accept `userId`/`status` — the server is the sole authority.
- Unique, normalized emails; `Guid` identifiers (no IDOR enumeration); restrict-on-delete
  foreign keys (history is never silently destroyed).
- Constant-time login (dummy-hash verification) mitigates user enumeration; auth endpoints
  are rate-limited.
