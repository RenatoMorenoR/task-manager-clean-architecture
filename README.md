# TaskManager

> Full-stack task management application — .NET 8 Clean Architecture + React + TypeScript

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com)
[![React](https://img.shields.io/badge/React-18-61DAFB)](https://react.dev)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://postgresql.org)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED)](https://docker.com)

---

## Table of Contents

- [Overview](#overview)
- [Bonus Features & Extras](#bonus-features--extras)
- [Project Documentation](#project-documentation)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Backend — Local Development](#backend--local-development)
- [Frontend — Local Development](#frontend--local-development)
- [Docker — Full Stack](#docker--full-stack)
- [API Reference](#api-reference)
- [Demo Credentials](#demo-credentials)
- [Architecture](#architecture)
- [Testing](#testing)
- [Environment Variables](#environment-variables)

---

## Overview

TaskManager is a personal task management application where authenticated users can create, view, update, and delete their own tasks. Built as a technical interview exercise demonstrating:

- **Clean Architecture** — strict layer separation with inward-only dependencies
- **TDD** — tests written before implementation (Red → Green → Refactor)
- **Raw SQL** — Npgsql without Entity Framework, Dapper, or MediatR
- **JWT Authentication** — stateless, ownership-enforced on every operation
- **Docker** — full stack via single command

---

## Bonus Features & Extras

In addition to fulfilling all core requirements, this project implements several "bonus" features that reflect production-grade engineering practices:

- **100% Warning-Free Codebase**: Strict compiler settings (`/warnaserror`) and IDE analyzer compliance ensure zero warnings in both the Backend (.NET) and Frontend (React/TS).
- **Full CI/CD Pipeline**: GitHub Actions workflow (`ci.yml`) automatically builds, tests (Backend & Frontend), and verifies Docker images on every commit.
- **Frontend TDD & Testing**: Implemented Vitest and React Testing Library to cover UI components, state management (Zustand), and interaction flows.
- **RFC 7807 Exception Handling**: A centralized `GlobalExceptionMiddleware` guarantees that all errors (even unexpected 500s) return a structured, standardized JSON problem detail, preventing information leakage.
- **Multi-stage Docker Builds**: Optimized Alpine-based Dockerfiles keep image sizes minimal (Frontend ~25MB, Backend ~120MB) and secure.
- **Strict Accessibility (a11y)**: Frontend forms use semantic HTML (`id` and `htmlFor` bindings) for screen readers and better usability.

---

## Project Documentation

This repository contains comprehensive documentation detailing the project requirements, architecture, and design decisions. You can find these documents in the `docs/` folder:

- 📄 [Original Requirements (PDF)](docs/req/Net%20-%20BLA%20-%20Technical%20Interview%20Exercise%20-%20V5.pdf) — The foundational technical exercise specifications.
- 📖 [User Story](docs/user-story.md) — The informal user story driving the domain logic, epic breakdown, and security acceptance criteria.
- 🤖 [GenAI Workflow](docs/genai-workflow.md) — Detailed log of how Generative AI was used, highlighting prompt engineering, validation, and architectural guidance.
- 🏗️ [Architecture Decision Records (ARD)](docs/ARD-TaskManager.md) — Explanations of core architectural choices (Clean Architecture, Raw SQL vs ORM, JWT, etc.).
- 📝 [Product Requirements Document (PRD)](docs/PRD-TaskManager.md) — Formal project constraints and product definitions.

*Note: For specific layer documentation, refer to [Backend README](backend/README.md) and [Frontend README](frontend/README.md).*

---

## Quick Start

> **Only requirement: [Docker Desktop](https://www.docker.com/products/docker-desktop/)**

```bash
git clone <repository-url>
cd taskmanager

# Start all services (API + Frontend + PostgreSQL)
make up

# Or without Make:
docker compose -f docker-compose.yml up -d
```

| Service    | URL                           |
|------------|-------------------------------|
| Frontend   | <http://localhost:3000>         |
| API        | <http://localhost:5000>         |
| Swagger UI | <http://localhost:5000/swagger> |
| PostgreSQL | localhost:5432                |

**Demo account:** `demo@taskmanager.com` / `Demo1234!`

---

## Project Structure

```
taskmanager/
├── backend/                              # .NET 8 solution
│   ├── TaskManager.Domain/               # Entities, interfaces, domain rules
│   │   ├── Entities/                     #   TaskItem.cs, User.cs
│   │   ├── Enums/                        #   TaskItemStatus.cs
│   │   ├── Exceptions/                   #   DomainExceptions.cs
│   │   └── Interfaces/                   #   ITaskRepository, IUserRepository, etc.
│   ├── TaskManager.Application/          # Use cases, DTOs
│   │   ├── DTOs/                         #   Request/Response records
│   │   ├── Interfaces/                   #   ICurrentUserService
│   │   └── UseCases/
│   │       ├── Tasks/                    #   CRUD use cases
│   │       └── Auth/                     #   Register, Login
│   ├── TaskManager.Infrastructure/       # Npgsql repos, JWT, BCrypt
│   │   ├── Repositories/                 #   TaskRepository, UserRepository
│   │   └── Services/                     #   JwtTokenService, BcryptPasswordHasher
│   ├── TaskManager.API/                  # ASP.NET Core entry point
│   │   ├── Controllers/                  #   TasksController, AuthController, HealthController
│   │   ├── Middleware/                   #   GlobalExceptionMiddleware
│   │   ├── Extensions/                   #   DI registration, CurrentUserService
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── TaskManager.Tests/                # xUnit + Moq + FluentAssertions
│       ├── Domain/                       #   Pure unit tests (no mocks needed)
│       ├── Application/UseCases/         #   Unit tests with mocked repos
│       ├── Infrastructure/               #   Integration tests (real DB)
│       ├── API/                          #   WebApplicationFactory tests
│       └── Helpers/                      #   TaskItemBuilder, UserBuilder
│
├── frontend/                             # React + Vite + TypeScript
│   ├── src/
│   │   ├── api/                          #   client.ts, auth.ts, tasks.ts
│   │   ├── components/
│   │   │   ├── ui/                       #   Reusable primitives (Button, Input, Modal)
│   │   │   └── tasks/                    #   TaskCard, TaskForm, TaskList
│   │   ├── hooks/                        #   useTasks.ts (React Query)
│   │   ├── pages/                        #   LoginPage, RegisterPage, TasksPage
│   │   ├── store/                        #   authStore.ts (Zustand)
│   │   └── types/                        #   index.ts (shared TypeScript types)
│   ├── Dockerfile                        #   Production: Node build → Nginx
│   ├── Dockerfile.dev                    #   Development: Vite HMR
│   ├── nginx.conf                        #   SPA routing + gzip + security headers
│   └── package.json
│
├── scripts/
│   ├── 001_schema.sql                    # Tables, indexes, updated_at triggers
│   └── 002_seed.sql                      # Demo user + sample tasks
│
├── docs/
│   ├── ARD-TaskManager.md                # Architecture Decision Records
│   ├── PRD-TaskManager.md                # Product Requirements Document
│   └── genai-workflow.md                 # GenAI prompts and validation notes
│
├── docker-compose.yml                    # Production orchestration
├── docker-compose.override.yml           # Development overrides (hot reload)
├── Makefile                              # Developer convenience commands
└── README.md
```

---

## Backend — Local Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- PostgreSQL running locally **or** start only the DB container (see below)

### 1. Start PostgreSQL only (recommended)

```bash
docker compose up postgres -d
```

This starts PostgreSQL on port `5432` and automatically applies schema + seed data from the `scripts/` folder.

### 2. Restore and run

```bash
cd backend/TaskManager.API
dotnet restore
dotnet run
```

The API starts at `http://localhost:5000`.
Swagger UI is available at `http://localhost:5000/swagger`.

### 3. Override configuration (optional)

Create `backend/TaskManager.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=taskmanager_db;Username=taskmanager;Password=taskmanager_pass"
  },
  "Jwt": {
    "Secret": "your-local-dev-secret-at-least-32-chars",
    "Issuer": "TaskManagerAPI",
    "Audience": "TaskManagerClient",
    "ExpirationHours": "24"
  }
}
```

### 4. Build the solution

```bash
cd backend
dotnet build
```

### 5. Hot reload during development

```bash
cd backend/TaskManager.API
dotnet watch run
```

---

## Frontend — Local Development

### Prerequisites

- [Node.js 20+](https://nodejs.org/)
- API running locally or via Docker on port `5000`

### 1. Install dependencies

```bash
cd frontend
npm install
```

### 2. Configure environment

Create `frontend/.env.local`:

```env
VITE_API_URL=http://localhost:5000
```

### 3. Start development server

```bash
npm run dev
```

Frontend starts at `http://localhost:3000` with Hot Module Replacement (HMR).

### 4. Build for production

```bash
npm run build
# Output written to frontend/dist/
```

### 5. Preview production build locally

```bash
npm run preview
```

---

## Docker — Full Stack

### Available Make commands

```bash
make up             # Start production build (docker-compose.yml only)
make dev            # Start with hot reload (compose + override)
make down           # Stop all services
make build          # Rebuild all images from scratch (no cache)
make prod           # Alias for production build with --build flag
make logs           # Tail logs from all containers
make logs-api       # Tail API logs only
make logs-db        # Tail database logs only
make db-reset       # Drop and re-seed the database
make db-shell       # Open psql shell inside the running container
make db-backup      # Backup database to ./backups/
make test           # Run all backend tests with coverage
make test-watch     # Run tests in watch mode
make test-coverage  # Generate HTML coverage report
make clean          # Remove containers, volumes, and local images
make clean-all      # Full clean including node_modules and .NET artifacts
make help           # Show all available commands
```

### Manual Docker commands (without Make)

```bash
# Start full stack (production)
docker compose -f docker-compose.yml up -d

# Start full stack (development with hot reload)
docker compose up -d

# Stop all services
docker compose down

# View logs
docker compose logs -f api
docker compose logs -f frontend
docker compose logs -f postgres

# Reset database to initial seed state
docker compose exec postgres psql -U taskmanager -d taskmanager_db \
  -c "TRUNCATE tasks, users CASCADE;"
docker compose exec postgres psql -U taskmanager -d taskmanager_db \
  -f /docker-entrypoint-initdb.d/01_schema.sql
docker compose exec postgres psql -U taskmanager -d taskmanager_db \
  -f /docker-entrypoint-initdb.d/02_seed.sql
```

### Container overview

| Container              | Internal Port | External Port | Base Image         |
|------------------------|---------------|---------------|--------------------|
| `taskmanager_db`       | 5432          | 5432          | postgres:16-alpine |
| `taskmanager_api`      | 8080          | 5000          | .NET 8 alpine      |
| `taskmanager_frontend` | 80            | 3000          | nginx:alpine       |

**Startup order:** `postgres` (healthcheck passes) → `api` (healthcheck passes) → `frontend`

### Docker image sizes (approximate)

| Image              | Size    |
|--------------------|---------|
| postgres:16-alpine | ~240 MB |
| taskmanager_api    | ~120 MB |
| taskmanager_frontend | ~25 MB |

---

## API Reference

All task endpoints require the header: `Authorization: Bearer <token>`

### Authentication

| Method | Endpoint             | Auth | Request Body                          |
|--------|----------------------|------|---------------------------------------|
| POST   | `/api/auth/register` | No   | `{ "email", "password", "name" }`     |
| POST   | `/api/auth/login`    | No   | `{ "email", "password" }`             |

**Response — both endpoints:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "name": "John Doe",
  "expiresAt": "2026-05-05T12:00:00Z"
}
```

### Tasks

| Method | Endpoint          | Auth | Request Body        | Success |
|--------|-------------------|------|---------------------|---------|
| GET    | `/api/tasks`      | JWT  | —                   | 200     |
| POST   | `/api/tasks`      | JWT  | `CreateTaskRequest` | 201     |
| GET    | `/api/tasks/{id}` | JWT  | —                   | 200     |
| PUT    | `/api/tasks/{id}` | JWT  | `UpdateTaskRequest` | 200     |
| DELETE | `/api/tasks/{id}` | JWT  | —                   | 204     |
| GET    | `/api/health`     | No   | —                   | 200     |

**CreateTaskRequest:**

```json
{
  "title": "Review Clean Architecture book",
  "description": "Read chapters 5-8",
  "dueDate": "2026-05-10T00:00:00Z"
}
```

**UpdateTaskRequest:**

```json
{
  "title": "Review Clean Architecture book",
  "description": "Read chapters 5-8 and take notes",
  "status": "InProgress",
  "dueDate": "2026-05-10T00:00:00Z"
}
```

**Task status values:** `Pending` | `InProgress` | `Completed` | `Cancelled`

**Task response object:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
  "title": "Review Clean Architecture book",
  "description": "Read chapters 5-8",
  "status": "InProgress",
  "dueDate": "2026-05-10T00:00:00Z",
  "createdAt": "2026-05-04T10:00:00Z",
  "updatedAt": "2026-05-04T11:30:00Z"
}
```

### Error responses

All errors follow [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807):

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Title cannot be empty.",
  "status": 400,
  "instance": "/api/tasks"
}
```

| Status | Scenario                                    |
|--------|---------------------------------------------|
| 400    | Validation failure (empty title, past date) |
| 401    | Missing, expired, or invalid JWT token      |
| 403    | Task exists but belongs to another user     |
| 404    | Task or user not found                      |
| 409    | Email already registered                    |
| 500    | Unexpected server error (logged with path)  |

---

## Demo Credentials

| Field    | Value                  |
|----------|------------------------|
| Email    | `demo@taskmanager.com` |
| Password | `Demo1234!`            |

The demo account comes pre-loaded with 5 sample tasks across different statuses (Pending, InProgress, Completed).

**To reset to the initial demo state:**

```bash
make db-reset
```

---

## Architecture

### Clean Architecture — Layer dependency diagram

```
┌──────────────────────────────────────┐
│             API Layer                │
│  TaskManager.API                     │
│  Controllers · Middleware · DI wiring│
└───────────────┬──────────────────────┘
                │ depends on
┌───────────────▼──────────────────────┐
│          Application Layer           │
│  TaskManager.Application             │
│  Use Cases · DTOs · App interfaces   │
└───────────────┬──────────────────────┘
                │ depends on
┌───────────────▼──────────────────────┐
│            Domain Layer              │
│  TaskManager.Domain                  │
│  Entities · Interfaces · Exceptions  │
│  ← Zero external dependencies →      │
└──────────────────────────────────────┘
                ▲
                │ implements
┌───────────────┴──────────────────────┐
│        Infrastructure Layer          │
│  TaskManager.Infrastructure          │
│  Npgsql · JWT · BCrypt               │
└──────────────────────────────────────┘
```

### Key architectural decisions

| Decision | Choice | Reason |
|----------|--------|--------|
| Data access | Raw Npgsql | Required by exercise; full SQL control |
| Database | PostgreSQL 16 | Docker-friendly; bonus skill in JD |
| Auth | JWT Bearer | Stateless, horizontally scalable |
| Use cases | Plain classes | MediatR explicitly forbidden |
| Entity construction | Factory methods + private setters | Enforce invariants at construction time |
| Exception handling | GlobalExceptionMiddleware | Thin controllers; centralized HTTP mapping |
| Container base | Alpine images | Minimal attack surface and image size |

Full rationale with trade-offs in [`docs/ARD-TaskManager.docx`](docs/ARD-TaskManager.docx).

### Domain rules enforced in entities

- `TaskItem.Create()` — validates title is non-empty, due date is not in the past
- `TaskItem.EnsureOwnedBy(userId)` — throws `UnauthorizedTaskAccessException` if user doesn't own the task
- `User.Create()` — validates email, name, and password hash are non-empty

### SOLID Principles Application

The architecture was designed explicitly to uphold SOLID principles:

- **Single Responsibility Principle (SRP)**: Each Use Case has exactly one reason to change. `CreateTaskUseCase` only orchestrates creation; it does not validate domain rules or execute SQL.
- **Open/Closed Principle (OCP)**: The system is open for extension but closed for modification. You can add a `ExportTasksUseCase` without touching any existing controllers, use cases, or entities.
- **Liskov Substitution Principle (LSP)**: `TaskRepository` strictly honors the `ITaskRepository` contract. The Application layer can substitute it with an `InMemoryTaskRepository` during testing without altering the program's correctness.
- **Interface Segregation Principle (ISP)**: Interfaces are strictly focused. `IUserRepository` does not force implementations to know about Tasks. Interfaces are segregated by entity.
- **Dependency Inversion Principle (DIP)**: High-level modules (Application/Domain) do not depend on low-level modules (Infrastructure). Both depend on abstractions (`ITaskRepository`) defined in the Domain layer.

---

## Testing

### Run all tests

```bash
# Requires .NET 8 SDK
cd backend
dotnet test TaskManager.Tests/ --collect:"XPlat Code Coverage"
```

### Run with HTML coverage report

```bash
make test-coverage
# Open: TestResults/coverage-report/index.html
```

### Watch mode during development

```bash
make test-watch
```

### Test layer breakdown

| Layer | Test Type | Tools | Coverage Target |
|-------|-----------|-------|-----------------|
| Domain | Pure unit (no mocks) | xUnit, FluentAssertions | 100% |
| Application | Unit with mocks | xUnit, Moq, FluentAssertions | ≥ 90% |
| Infrastructure | Integration (real DB) | xUnit, DatabaseFixture | ≥ 70% |
| API | Integration (full stack) | WebApplicationFactory | ≥ 80% |

### TDD methodology

All implementation code was written following strict Red → Green → Refactor:

1. **Red** — write a failing test that describes the desired behavior
2. **Green** — write the absolute minimum code to make the test pass
3. **Refactor** — clean up the code without breaking any tests

No code is written without a failing test first.

---

## Environment Variables

### Backend

Set via `appsettings.json`, `appsettings.Development.json`, or environment variables (the latter take precedence in Docker).

| Variable | Development Default | Description |
|----------|---------------------|-------------|
| `ConnectionStrings__Postgres` | `Host=localhost;...` | PostgreSQL connection string |
| `Jwt__Secret` | `super-secret-key-...` | HMAC-SHA256 key (min 32 chars in production) |
| `Jwt__Issuer` | `TaskManagerAPI` | JWT `iss` claim |
| `Jwt__Audience` | `TaskManagerClient` | JWT `aud` claim |
| `Jwt__ExpirationHours` | `24` | Token lifetime |
| `SWAGGER_ENABLED` | `true` | Expose Swagger UI outside Development |
| `ASPNETCORE_ENVIRONMENT` | `Development` | ASP.NET Core environment name |

### Frontend

Set via `.env.local` for local development. Build-time only (Vite bakes them into the bundle).

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_URL` | `http://localhost:5000` | Backend API base URL |

> **Security note:** Never commit real secrets to source control. The values in `appsettings.json` are safe for local development only. In production environments, inject secrets via environment variables, Docker secrets, or a secrets manager such as AWS Secrets Manager or Azure Key Vault.
