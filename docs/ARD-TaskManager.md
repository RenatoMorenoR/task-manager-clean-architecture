# Architecture Requirements Document (ARD)

**Project:** TaskManager Application
**Version:** 1.0
**Date:** May 2026
**Author:** Senior .NET Architect
**Status:** APPROVED

---

## Table of Contents

- [1. Project Overview](#1-project-overview)
- [2. Architecture Decision Records](#2-architecture-decision-records)
  - [ADR-001: Clean Architecture Layer Structure](#adr-001-clean-architecture-layer-structure)
  - [ADR-002: Raw Npgsql — No ORM](#adr-002-raw-npgsql--no-orm)
  - [ADR-003: PostgreSQL over SQL Server](#adr-003-postgresql-over-sql-server)
  - [ADR-004: Use Cases as Plain Classes](#adr-004-use-cases-as-plain-classes)
  - [ADR-005: Domain Entity Factory Methods + Private Setters](#adr-005-domain-entity-factory-methods--private-setters)
  - [ADR-006: JWT Authentication — Stateless](#adr-006-jwt-authentication--stateless)
  - [ADR-007: Docker Multi-Stage Build](#adr-007-docker-multi-stage-build)
- [3. Layer Dependency Rules](#3-layer-dependency-rules)
- [4. Exception → HTTP Mapping](#4-exception--http-mapping)
- [5. Infrastructure & Docker Architecture](#5-infrastructure--docker-architecture)
- [6. Test Strategy](#6-test-strategy)

---

## 1. Project Overview

TaskManager is a full-stack web application built as a technical interview exercise for a Senior .NET Full Stack position. The application demonstrates mastery of Clean Architecture, Test-Driven Development, raw SQL data access (without ORM), and modern frontend development.

---

## 2. Architecture Decision Records

Each ADR follows the format: **Context → Decision → Rationale → Consequences**

---

### ADR-001: Clean Architecture Layer Structure

| Field | Detail |
|-------|--------|
| **Context** | The exercise explicitly evaluates adherence to Clean Architecture principles. The solution must demonstrate clear separation of concerns and independence of components. |
| **Decision** | Four-layer structure: Domain → Application → Infrastructure → API. Dependencies point strictly inward. Domain has zero external dependencies. |
| **Rationale** | Enables independent testing of each layer. Business logic is testable without a database. Infrastructure is swappable without touching domain or application code. |
| **Consequences** | More initial boilerplate, but dramatically easier to test, extend, and maintain. All unit tests for domain and application layers run in milliseconds with no I/O. |

```
┌──────────────────────────────────┐
│          API Layer               │  ← Depends on Application
│  Controllers · Middleware · DI   │
└───────────────┬──────────────────┘
                │
┌───────────────▼──────────────────┐
│       Application Layer          │  ← Depends on Domain only
│  Use Cases · DTOs · Interfaces   │
└───────────────┬──────────────────┘
                │
┌───────────────▼──────────────────┐
│         Domain Layer             │  ← Depends on NOTHING
│  Entities · Interfaces · Rules   │
└──────────────────────────────────┘
                ▲
┌───────────────┴──────────────────┐
│      Infrastructure Layer        │  ← Implements Domain interfaces
│  Npgsql · JWT · BCrypt           │
└──────────────────────────────────┘
```

---

### ADR-002: Raw Npgsql — No ORM

| Field | Detail |
|-------|--------|
| **Context** | The exercise explicitly forbids Entity Framework, Dapper, and MediatR to test raw data access skills. |
| **Decision** | Use `NpgsqlDataSource` with fully parameterized SQL. Registered as a singleton for connection pooling. Manual mapping from `NpgsqlDataReader` to domain entities via private `Reconstruct()` factory methods. |
| **Rationale** | Demonstrates deep SQL competency. Full control over queries. No abstraction leaking infrastructure concerns into the domain. |
| **Consequences** | More verbose repository code, offset by explicit SQL visibility and full query control. SQL injection prevented by always using parameters. `RETURNING` clause used for create/update to avoid extra round-trips. |

**Key pattern:**
```csharp
cmd.CommandText = """
    INSERT INTO tasks (id, user_id, title, ...)
    VALUES (@id, @userId, @title, ...)
    RETURNING id, user_id, title, ...
    """;
cmd.Parameters.AddWithValue("@id", task.Id); // Always parameterized
```

---

### ADR-003: PostgreSQL over SQL Server

| Field | Detail |
|-------|--------|
| **Context** | Job description lists PostgreSQL as a bonus skill. SQL Server requires Windows licensing for local development. |
| **Decision** | PostgreSQL 16 via Docker. Schema managed via raw SQL scripts in `/scripts/` (no migration tool). |
| **Rationale** | Demonstrates the listed bonus skill. Docker-friendly alpine image (~240 MB). `uuid-ossp` extension used for UUID primary keys. `TIMESTAMPTZ` ensures UTC storage. |
| **Consequences** | All datetime columns use `TIMESTAMPTZ`. UUID primary keys over `INT` for distributed-system readiness. DB auto-initialized by Docker Compose on first start. |

---

### ADR-004: Use Cases as Plain Classes

| Field | Detail |
|-------|--------|
| **Context** | MediatR is the common choice for CQRS in .NET, but it is explicitly forbidden by the exercise. |
| **Decision** | Simple use case classes with a single `ExecuteAsync()` method. Injected directly into controllers via the DI container. |
| **Rationale** | Demonstrates that good architecture does not require a framework. Each use case is independently testable with simple constructor injection and mocks. |
| **Consequences** | Slightly more DI registration code. Benefit: zero framework overhead, transparent call stack, no handler resolution magic at runtime. |

```csharp
// Simple, no framework needed
public class CreateTaskUseCase(ITaskRepository repo, ICurrentUserService currentUser)
{
    public async Task<TaskDto> ExecuteAsync(CreateTaskRequest request, CancellationToken ct = default)
    { ... }
}
```

---

### ADR-005: Domain Entity Factory Methods + Private Setters

| Field | Detail |
|-------|--------|
| **Context** | Entities must always be in a valid state. Standard public constructors allow invalid state to be constructed externally. |
| **Decision** | Static `Create()` factory method enforces all invariants. Private setters prevent external mutation. Separate `Reconstruct()` method (internal use only) for DB hydration without re-validating creation rules. |
| **Rationale** | Impossible to create a `TaskItem` with an empty title or a past due date. Business rules live in the entity, not scattered across services or controllers. |
| **Consequences** | TDD is natural — write tests against `Create()` before implementing it. Domain layer tests cover all business rules without any mocks needed. |

```csharp
// ✅ Enforces invariants at construction time
public static TaskItem Create(Guid userId, string title, string description, DateTime dueDate)
{
    if (string.IsNullOrWhiteSpace(title))
        throw new DomainValidationException("Title cannot be empty.");
    if (dueDate.Date < DateTime.UtcNow.Date)
        throw new DomainValidationException("Due date cannot be in the past.");
    // ...
}
```

---

### ADR-006: JWT Authentication — Stateless

| Field | Detail |
|-------|--------|
| **Context** | The application needs user identity on every request. Session-based auth requires server-side state and complicates horizontal scaling. |
| **Decision** | JWT Bearer tokens signed with HMAC-SHA256. Token returned in response body for demo simplicity. 24-hour expiration. `ICurrentUserService` extracts `UserId` from token claims via `IHttpContextAccessor`. |
| **Rationale** | Stateless and horizontally scalable. Industry standard in .NET ecosystem. Auth concern stays in Infrastructure and API — the Domain layer has no knowledge of JWT. |
| **Trade-off** | **Production note:** the token should be returned in an `HttpOnly` cookie to prevent XSS attacks. This trade-off is explicitly documented and called out during the presentation. |

---

### ADR-007: Docker Multi-Stage Build

| Field | Detail |
|-------|--------|
| **Context** | The application must be portable, reproducible, and runnable without a local .NET SDK or Node.js installation. |
| **Decision** | Multi-stage Dockerfiles: `SDK → Runtime (alpine)` for the API; `Node → Nginx (alpine)` for the frontend. Tests run during the build stage — a failing test prevents image creation. |
| **Rationale** | Production images contain only the runtime artifact. No SDK, no source code, no dev dependencies in the final image. |
| **Consequences** | API image ~120 MB (alpine). Frontend image ~25 MB (nginx:alpine). `docker compose up` is the single command to run the full stack, including schema initialization and seed data. |

---

## 3. Layer Dependency Rules

| Layer | Depends On | NuGet Packages | Key Rule |
|-------|-----------|----------------|----------|
| **Domain** | Nothing | None (`System.*` only) | Zero external dependencies |
| **Application** | Domain only | FluentValidation (optional) | No DB or HTTP types |
| **Infrastructure** | Domain + Application | Npgsql, BCrypt.Net, Microsoft.IdentityModel.Tokens | No business logic |
| **API** | Application + Infrastructure (DI only) | AspNetCore, Swashbuckle | Thin controllers only |

### Forbidden in each layer

```
Domain        → ❌ Any NuGet package, ❌ Data annotations, ❌ Infrastructure types
Application   → ❌ NpgsqlConnection, ❌ HttpContext, ❌ Infrastructure types
Infrastructure → ❌ Business logic, ❌ Domain rule enforcement
API           → ❌ SQL queries, ❌ Repository calls, ❌ Business logic
```

---

## 4. Exception → HTTP Mapping

Handled centrally by `GlobalExceptionMiddleware`. Controllers never contain try/catch blocks.

| Exception | HTTP Status | Scenario |
|-----------|-------------|----------|
| `DomainValidationException` | 400 Bad Request | Empty title, past due date, invalid status |
| `NotFoundException` | 404 Not Found | Task or user does not exist |
| `UnauthorizedTaskAccessException` | 403 Forbidden | Accessing another user's task |
| `ConflictException` | 409 Conflict | Duplicate email on registration |
| `AuthenticationException` | 401 Unauthorized | Invalid credentials / no token |
| Unhandled exceptions | 500 Internal Server Error | Logged with request path |

All error responses follow [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807):

```json
{
  "type": "https://httpstatuses.com/403",
  "title": "You do not have access to this resource.",
  "status": 403,
  "instance": "/api/tasks/3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

---

## 5. Infrastructure & Docker Architecture

Three containers coordinated by Docker Compose with explicit health checks and startup ordering.

| Container | Internal Port | External Port | Base Image | Role |
|-----------|---------------|---------------|------------|------|
| `taskmanager_db` | 5432 | 5432 | postgres:16-alpine | Data persistence, schema init |
| `taskmanager_api` | 8080 | 5000 | .NET 8 alpine | REST API, JWT auth, business logic |
| `taskmanager_frontend` | 80 | 3000 | nginx:alpine | React SPA, SPA fallback routing |

**Startup order:** `postgres` (healthcheck: `pg_isready`) → `api` (healthcheck: `GET /api/health`) → `frontend`

**Frontend Nginx configuration includes:**
- SPA fallback (`try_files $uri /index.html`) for React Router
- Gzip compression for JS, CSS, JSON
- Aggressive caching for static assets (`Cache-Control: immutable`)
- Security headers: `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`

---

## 6. Test Strategy

| Test Type | Layer | Tools | Coverage Target |
|-----------|-------|-------|-----------------|
| Pure unit tests | Domain | xUnit, FluentAssertions | **100%** |
| Unit tests with mocks | Application | xUnit, Moq, FluentAssertions | ≥ 90% |
| Integration tests (real DB) | Infrastructure | xUnit, DatabaseFixture | ≥ 70% |
| Integration tests (full stack) | API | WebApplicationFactory | ≥ 80% |

**TDD methodology applied throughout:**

```
1. RED    → Write a failing test that describes the desired behavior
2. GREEN  → Write the minimum code to make the test pass
3. REFACTOR → Clean up without breaking any tests
```

No implementation code is written without a failing test first.
