# TaskManager — Project Intelligence Hub

## 🎯 Project Context

Technical interview exercise for a Senior .NET Full Stack position at Ballast Lane.
This is a **Task Management** web application showcasing:

- Clean Architecture in .NET 8
- TDD with xUnit + Moq
- Raw SQL with Npgsql (NO Entity Framework, NO Dapper, NO Mediator)
- JWT Authentication
- React + TypeScript frontend
- GenAI-assisted development workflow

**Interview criteria (weighted):**

1. Clean Architecture & separation of concerns
2. TDD / test coverage
3. Code quality & best practices
4. Functionality without bugs
5. GenAI fluency & critical thinking

---

## 🏗️ Solution Architecture

```
taskmanager/
├── CLAUDE.md                        ← You are here (orchestrator)
├── .claude/
│   └── agents/
│       ├── architect.md             ← Architecture decisions & design
│       ├── developer.md             ← Implementation & coding standards
│       └── qa.md                   ← Testing strategy & quality gates
├── docs/
│   ├── user-story.md
│   ├── architecture-decisions.md
│   └── genai-workflow.md           ← For interview presentation
├── backend/
│   ├── TaskManager.Domain/          ← Entities, interfaces, domain rules
│   ├── TaskManager.Application/     ← Use cases, DTOs, service interfaces
│   ├── TaskManager.Infrastructure/  ← Npgsql repos, JWT, external services
│   ├── TaskManager.API/             ← Controllers, middleware, DI wiring
│   └── TaskManager.Tests/           ← All tests (unit + integration)
└── frontend/                        ← React + TypeScript + Vite
```

---

## 🧱 Clean Architecture Layers (STRICT)

```
        ┌─────────────────────────────────┐
        │           API Layer             │  ← Depends on Application
        │    Controllers, Middleware       │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │        Application Layer        │  ← Depends on Domain only
        │   Use Cases, DTOs, Interfaces   │
        └──────────────┬──────────────────┘
                       │
        ┌──────────────▼──────────────────┐
        │          Domain Layer           │  ← Depends on NOTHING
        │   Entities, Domain Interfaces   │
        │   Business Rules, Value Objects │
        └─────────────────────────────────┘
                       ▲
        ┌──────────────┴──────────────────┐
        │      Infrastructure Layer       │  ← Implements Domain interfaces
        │   Npgsql, JWT, Email, etc.      │
        └─────────────────────────────────┘
```

**Golden rules — NEVER break these:**

- Domain has ZERO external dependencies (no NuGet packages except primitives)
- Application references only Domain
- Infrastructure references Domain + Application (for interface implementations)
- API references Application + Infrastructure (only for DI registration)
- No business logic in Controllers or Infrastructure

---

## 📖 User Story

**As a registered user, I want to manage my personal tasks (create, view, edit, delete)
so that I can organize my daily work, knowing that only I can see my own tasks.**

### Acceptance Criteria

- [ ] User can register with email + password
- [ ] User can login and receive a JWT token
- [ ] Authenticated user can CREATE a task (title, description, status, due_date)
- [ ] Authenticated user can READ their tasks (list + single)
- [ ] Authenticated user can UPDATE their tasks
- [ ] Authenticated user can DELETE their tasks
- [ ] User cannot access another user's tasks (authorization)
- [ ] Public endpoint: GET /api/health (no auth required)

---

## 🔧 Tech Stack Decisions

| Concern | Choice | Rationale |
|---------|--------|-----------|
| Runtime | .NET 8 | Latest LTS, demonstrates currency |
| Database | PostgreSQL | Modern, bonus skill in JD |
| Data Access | Npgsql (raw) | Required by exercise (no EF/Dapper) |
| Auth | JWT Bearer | Industry standard, stateless |
| Testing | xUnit + Moq | .NET ecosystem standard |
| Frontend | React + Vite + TypeScript | Clean, modern, well-known |
| Styling | Tailwind CSS | Fast to write, professional result |
| API Docs | Swagger/OpenAPI | Required for demo |

---

## 🤖 Agent Roles

When working on this project, invoke the right agent:

- **`/architect`** — Design decisions, layer boundaries, interface contracts, ADRs
- **`/developer`** — Implementation, coding standards, patterns, code reviews
- **`/qa`** — Test strategy, TDD cycles, coverage gates, quality checks

---

## 📋 Development Phases

### Phase 1 — Domain (TDD first)

1. Define entities: `TaskItem`, `User`
2. Define repository interfaces: `ITaskRepository`, `IUserRepository`
3. Define domain exceptions
4. Write domain tests FIRST, then implement

### Phase 2 — Application Layer

1. Define use case interfaces
2. Write use case tests FIRST (mock repositories)
3. Implement: `CreateTaskUseCase`, `GetTasksUseCase`, `UpdateTaskUseCase`, `DeleteTaskUseCase`
4. Implement: `RegisterUserUseCase`, `LoginUserUseCase`

### Phase 3 — Infrastructure

1. PostgreSQL schema + migrations (raw SQL scripts)
2. Npgsql repository implementations
3. JWT token service
4. Password hashing service

### Phase 4 — API

1. Controllers (thin — only call use cases)
2. Middleware (exception handling, auth)
3. DI registration
4. Seeded data for demo

### Phase 5 — Frontend

1. Auth pages (register/login)
2. Task list + CRUD UI
3. API service layer (axios)
4. Protected routes

### Phase 6 — Presentation Prep

1. GenAI workflow documentation
2. README with setup instructions
3. Demo script

---

## 🚀 Quick Commands

```bash
# Backend
dotnet build backend/
dotnet test backend/TaskManager.Tests/
dotnet run --project backend/TaskManager.API/

# Frontend
cd frontend && npm run dev

# Database
docker-compose up -d postgres
psql -U taskmanager -d taskmanager_db -f scripts/schema.sql
psql -U taskmanager -d taskmanager_db -f scripts/seed.sql
```

---

## ⚠️ Constraints Checklist

Before committing any code, verify:

- [ ] No `using EntityFramework` anywhere
- [ ] No `using Dapper` anywhere  
- [ ] No `using MediatR` anywhere
- [ ] No business logic in controllers
- [ ] No direct DB access from Application layer
- [ ] All public methods have unit tests
- [ ] No hardcoded connection strings (use appsettings / env vars)
