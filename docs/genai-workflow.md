# GenAI Workflow — Complete Development Log

## Overview

This document is a first-person account of how GenAI tools were used throughout
the development of the TaskManager application. It demonstrates:

- **Fluency with AI tools** — knowing when and how to use them effectively
- **Prompt engineering** — crafting precise, context-rich prompts that yield useful output
- **Critical thinking** — evaluating, correcting, and improving AI-generated code
- **AI as a collaborator, not a replacement** — the human made every architectural decision

**Tool used:** Claude (claude-sonnet-4) via Claude.ai and Cursor IDE

---

## My Philosophy on GenAI in Development

Before showing the workflow, it's important to state the underlying approach:

> AI is a junior pair programmer with encyclopedic knowledge but no judgment.
> It knows syntax perfectly. It knows patterns. It does not know *your* constraints,
> *your* architecture, or *why* you made a decision three files ago.
> My job is to provide that context — and then verify everything it produces.

The biggest mistake I see is using AI to *generate* the architecture. I use AI to
*implement* architecture I've already decided. There's a critical difference:

| ❌ Wrong approach | ✅ My approach |
|-------------------|----------------|
| "Build me a .NET API" | "Implement this interface I designed, following these constraints" |
| Accept the first output | Review every line against architectural rules |
| AI decides layer boundaries | I decide layer boundaries, AI writes the code |
| Skip code review because "AI wrote it" | Apply stricter review — AI is confident even when wrong |

---

## Phase 1 — Architecture Design (AI as Sparring Partner)

The first use of AI was **not** to generate code. It was to stress-test my own
architectural decisions by asking challenging questions.

### Prompt 1.1 — Interface placement

```
I'm designing a Clean Architecture .NET 8 solution for a Task Management API.
Constraints:
  - NO Entity Framework, NO Dapper, NO MediatR (exercise requirement)
  - PostgreSQL via raw Npgsql
  - JWT authentication
  - CRUD for tasks, user registration, user login

I have three interface placement questions. Answer each with Clean Architecture
dependency rule reasoning, not just convention:

1. Should IPasswordHasher live in Domain or Application?
   Argue both sides before giving your recommendation.

2. Should ITokenService live in Domain or Application?
   Is token generation a domain concern or an application orchestration concern?

3. Where do DTOs live — Domain, Application, or API?
   What are the consequences of each choice?
```

**AI output — what it got right:**

- Correctly placed `IPasswordHasher` in Domain: password hashing is a security invariant, not orchestration
- Correctly argued DTOs belong in Application: they're transport objects for the Application boundary, not domain concepts

**AI output — what it got wrong:**

- Placed `ITokenService` in Application, arguing token generation is "orchestration"
- This was incorrect for our architecture: the Domain needs to express "this operation requires a verified identity" — the token is the mechanism for that

**My correction:**

```
I disagree with ITokenService in Application. In our architecture, the domain
needs to be able to say "generate a token for this User" — that's a domain
concern because the User entity is a domain concept. If ITokenService lives
in Application, we'd have to pass domain entities up to the Application layer
just to generate a token, which inverts the dependency.

ITokenService stays in Domain alongside IPasswordHasher. The Infrastructure
implementation (JwtTokenService) depends on Domain — not the other way around.
```

**AI response:** Agreed with my correction and updated its reasoning. This is the right pattern — AI concedes when presented with sound architectural argument.

---

### Prompt 1.2 — TDD approach for domain entities

```
I'm about to implement the TaskItem domain entity in C# .NET 8.
Before I write a single line of implementation, I need the unit tests.

The entity will have:
  - Static Create(userId, title, description, dueDate) factory method
  - Private setters on all properties
  - Static Reconstruct(...) method for DB hydration
  - Update(title, description, status, dueDate) instance method
  - EnsureOwnedBy(userId) instance method that throws if wrong owner

Business rules to enforce:
  - Title: required, not whitespace, max 500 chars
  - DueDate: cannot be in the past (compare to UTC date, not datetime)
  - EnsureOwnedBy: throws UnauthorizedTaskAccessException, NOT ArgumentException

Write the failing tests first (Red phase of TDD). Use xUnit and FluentAssertions.
Use [Theory] + [InlineData] where testing multiple invalid inputs.
Do NOT write the entity implementation — only the tests.
Tests must fail when I run them because the entity doesn't exist yet.
```

**AI output — what it got right:**

- Correct xUnit + FluentAssertions syntax
- Used `[Theory]` + `[InlineData]` for empty/null/whitespace title variants
- Tested that the constructor throws the right exception type

**AI output — what I improved:**

```csharp
// AI tested:
[InlineData("")]
[InlineData(null)]
// I added:
[InlineData("   ")]   // whitespace-only is also invalid — AI missed this
```

```csharp
// AI used Thread.Sleep for timestamp tests — fragile and slow
Thread.Sleep(100);
task.UpdatedAt.Should().BeAfter(original);

// I changed to:
task.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
// More robust, no sleep needed
```

```csharp
// AI didn't verify repository was NOT called when validation fails
// I added to every validation test:
_repo.Verify(r => r.CreateAsync(It.IsAny<TaskItem>(), default), Times.Never);
// Critical: confirms no partial side effects when validation throws
```

---

## Phase 2 — API Controller Scaffold

### Prompt 2.1 — TasksController

```
You are a Senior .NET 8 developer implementing Clean Architecture strictly.

Generate an ASP.NET Core 8 Web API controller for Task CRUD.

Hard constraints (violations make the code unacceptable):
  - Each action method must be ≤ 15 lines. No exceptions.
  - ZERO business logic in the controller. Not a single if statement.
  - No try/catch anywhere in the controller. Exceptions handled by GlobalExceptionMiddleware.
  - Use [Authorize] at the class level, not per-method.
  - CancellationToken must be present on every public async method.
  - Use C# 12 primary constructor syntax for dependency injection.
  - POST returns CreatedAtAction with Location header.
  - DELETE returns NoContent() (204), not Ok().

Use cases to inject (already exist, just inject them):
  CreateTaskUseCase, GetTasksUseCase, GetTaskByIdUseCase,
  UpdateTaskUseCase, DeleteTaskUseCase

Each use case has a single method: ExecuteAsync(...)

Include XML documentation comments on each action for Swagger.
Route: api/tasks
```

**AI output (representative):**

```csharp
[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController(
    CreateTaskUseCase createTask,
    GetTasksUseCase getTasks,
    GetTaskByIdUseCase getTaskById,
    UpdateTaskUseCase updateTask,
    DeleteTaskUseCase deleteTask) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var result = await createTask.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    // ...
}
```

**Issues found and corrected:**

| # | Issue | AI Code | My Fix |
|---|-------|---------|--------|
| 1 | Missing CancellationToken on GET | `getTasks.ExecuteAsync()` | `getTasks.ExecuteAsync(ct)` |
| 2 | DELETE returned Ok() | `return Ok()` | `return NoContent()` |
| 3 | Try/catch in one action | `try { } catch { return BadRequest(); }` | Removed entirely — middleware handles it |
| 4 | No ownership validation | Called use case directly | Already in use case — confirmed correct |
| 5 | Missing `[FromBody]` on PUT | `Update(Guid id, UpdateTaskRequest req` | Added `[FromBody]` attribute |

---

### Prompt 2.2 — Global Exception Middleware

```
Write an ASP.NET Core 8 middleware class named GlobalExceptionMiddleware.

It must:
  1. Catch ALL unhandled exceptions from the request pipeline
  2. Log the exception using ILogger with the request path
  3. Map specific domain exceptions to HTTP status codes:
       DomainValidationException       → 400
       NotFoundException               → 404
       UnauthorizedTaskAccessException → 403
       ConflictException               → 409
       AuthenticationException         → 401
       anything else                   → 500
  4. Return RFC 7807 Problem Details JSON (not a custom format)
  5. Use C# switch expression pattern matching, not if/else chains
  6. The 500 response must NOT expose the exception message to the client

Domain exception namespace: TaskManager.Domain.Exceptions
```

**AI output — what it got right:**

- Correct middleware signature (`RequestDelegate next`)
- Switch expression with pattern matching
- RFC 7807 structure with `type`, `title`, `status`, `instance`

**AI output — what I corrected:**

```csharp
// AI exposed exception message on 500 — security issue
_ => (500, ex.Message)  // ❌ Never expose internal errors to clients

// My fix:
_ => (500, "An unexpected error occurred.")  // ✅ Generic message, exception is logged
```

```csharp
// AI used JsonSerializer.Serialize with no options — not idiomatic for ASP.NET Core
await context.Response.WriteAsync(JsonSerializer.Serialize(problem));

// I kept this as-is for simplicity in demo context.
// Production would use: context.Response.WriteAsJsonAsync(problem)
// which respects the app's configured JsonOptions (camelCase, etc.)
// Noted this trade-off explicitly in code comments.
```

---

## Phase 3 — Npgsql Repository Implementation

### Prompt 3.1 — TaskRepository with raw SQL

```
Generate a C# .NET 8 repository class named TaskRepository implementing ITaskRepository.
Use raw Npgsql with NpgsqlDataSource (NOT NpgsqlConnection directly).
NO Entity Framework, NO Dapper.

ITaskRepository interface:
  Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct)
  Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct)
  Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct)
  Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ct)
  Task DeleteAsync(Guid id, CancellationToken ct)

TaskItem.Reconstruct() signature (for mapping from reader):
  static TaskItem Reconstruct(Guid id, Guid userId, string title, string description,
    TaskItemStatus status, DateTime dueDate, DateTime createdAt, DateTime updatedAt)

PostgreSQL table:
  tasks(id UUID, user_id UUID, title VARCHAR(500), description TEXT,
        status SMALLINT, due_date TIMESTAMPTZ, created_at TIMESTAMPTZ, updated_at TIMESTAMPTZ)

Rules:
  - ALL SQL must use parameterized queries — no string interpolation whatsoever
  - Use RETURNING clause on INSERT and UPDATE to avoid extra round-trips
  - Use 'await using' on ALL disposable resources (connection, command, reader)
  - Call .ToUniversalTime() on all DateTime values read from PostgreSQL
  - NpgsqlDataSource injected via constructor (primary constructor syntax)
```

**AI output — critical issue found (SQL injection):**

```csharp
// AI wrote this in one of the methods — a serious SQL injection vulnerability:
cmd.CommandText = $"SELECT * FROM tasks WHERE user_id = '{userId}'";
//                                                    ^^^^^^^^^^
// String interpolation with a GUID — technically safe here, but the pattern
// is wrong and would be catastrophic with a string parameter.

// My immediate correction:
cmd.CommandText = "SELECT * FROM tasks WHERE user_id = @userId";
cmd.Parameters.AddWithValue("@userId", userId);
```

**Why I flagged this even though GUIDs can't cause injection:**
The habit is the vulnerability. If a developer sees this pattern and copies it with a
string parameter, the application is compromised. Code review must eliminate the pattern
entirely, not just the specific instance.

**Other corrections:**

```csharp
// AI used NpgsqlConnection directly — not wrong, but not optimal
using var conn = new NpgsqlConnection(connectionString);

// Correct for modern Npgsql: use NpgsqlDataSource (connection pooling built-in)
await using var conn = await dataSource.OpenConnectionAsync(ct);
```

```csharp
// AI missed DateTime UTC handling
r.GetDateTime(5)  // Returns unspecified Kind

// My correction:
r.GetDateTime(5).ToUniversalTime()  // Explicitly UTC — consistent with domain model
```

---

### Prompt 3.2 — PostgreSQL schema design

```
Design a PostgreSQL 16 schema for a task management application.
Requirements:
  - Two tables: users and tasks
  - tasks.user_id is a foreign key to users.id with CASCADE DELETE
  - Use UUID primary keys via uuid-ossp extension
  - All datetime columns must be TIMESTAMPTZ (not TIMESTAMP)
  - tasks.status stored as SMALLINT with a CHECK constraint (0-3)
  - Add a database trigger to auto-update updated_at on UPDATE (for both tables)
  - Add appropriate indexes for query patterns:
      users: lookup by email
      tasks: lookup by user_id, filter by status, filter by due_date
  - Use IF NOT EXISTS on all CREATE statements (idempotent)

Output: raw SQL only, no ORM migrations.
```

**AI output — what it got right:**

- Correct `TIMESTAMPTZ` on all datetime columns
- `uuid_generate_v4()` default for primary keys
- `ON DELETE CASCADE` on the foreign key
- `CHECK (status BETWEEN 0 AND 3)` constraint
- Trigger function for `updated_at`

**AI output — what I improved:**

```sql
-- AI created the trigger with CREATE TRIGGER (fails if exists on re-run)
CREATE TRIGGER trg_tasks_updated_at ...

-- I changed to:
CREATE OR REPLACE TRIGGER trg_tasks_updated_at ...
-- Makes the script idempotent — can be run multiple times without error
```

```sql
-- AI missed the trigger function also needs OR REPLACE
CREATE FUNCTION update_updated_at_column()

-- My correction:
CREATE OR REPLACE FUNCTION update_updated_at_column()
```

---

## Phase 4 — TDD: Application Use Case Tests

### Prompt 4.1 — Delete use case — the three critical scenarios

```
Write xUnit unit tests for DeleteTaskUseCase using Moq and FluentAssertions.
This use case takes a task ID, verifies the task exists, verifies the caller
owns it, then deletes it.

Write exactly three tests covering these scenarios:
  1. Happy path: task exists and belongs to current user → DeleteAsync called once
  2. Task belongs to different user → UnauthorizedTaskAccessException thrown,
     DeleteAsync NEVER called
  3. Task does not exist → NotFoundException thrown, DeleteAsync NEVER called

For scenarios 2 and 3: explicitly verify that DeleteAsync was NOT called.
This is not optional — it confirms no partial side effects.

Use Mock<ITaskRepository> and Mock<ICurrentUserService>.
Use TaskItemBuilder.CreateForUser(userId) for test data.
```

**Why I wrote this prompt this way:**

The "verify it was NOT called" constraint was explicit because AI consistently omits it.
This verification is architecturally important: it proves the use case short-circuits
correctly and doesn't attempt a DB operation on invalid input.

**AI output — what it got right:**

- Three correctly structured tests
- `Times.Once` on the happy path
- `Times.Never` on the error paths (because I was explicit about it)

**AI output — small improvement I made:**

```csharp
// AI used default(CancellationToken) implicitly
_repo.Verify(r => r.DeleteAsync(task.Id, default));

// I kept this but added a comment:
// default(CancellationToken) is correct here — tests don't pass real cancellation tokens
// This matches how the use case calls the repository in unit tests
```

---

## Phase 5 — Frontend Scaffold

### Prompt 5.1 — React Query hooks for task management

```
Generate React Query (TanStack Query v5) custom hooks for task CRUD operations
in TypeScript. The hooks should wrap an existing tasksApi object with these methods:
  tasksApi.getAll()   → Task[]
  tasksApi.create(data: CreateTaskRequest)  → Task
  tasksApi.update(id, data: UpdateTaskRequest)  → Task
  tasksApi.delete(id)  → void

Requirements:
  - useTasks(): query for listing, queryKey = ['tasks']
  - useCreateTask(): mutation that invalidates ['tasks'] on success
  - useUpdateTask(): mutation with { id, data } parameter, invalidates on success
  - useDeleteTask(): mutation with id parameter, invalidates on success
  - All in a single file: src/hooks/useTasks.ts
  - TypeScript strict mode — no 'any'
  - Import types from '../types' (already defined)
```

**AI output — mostly correct. One fix:**

```typescript
// AI used the v4 mutation syntax (old API):
useMutation(mutationFn, { onSuccess })

// React Query v5 uses object syntax:
useMutation({ mutationFn, onSuccess })
```

This is a common AI mistake — training data contains more v4 examples than v5.
Always verify library version compatibility.

---

### Prompt 5.2 — Axios client with interceptors

```
Generate an Axios instance for a React + TypeScript app that:
  1. Sets baseURL from VITE_API_URL env variable, fallback to http://localhost:5000
  2. Sets Content-Type: application/json as default header
  3. Request interceptor: reads token from localStorage, adds Authorization header
  4. Response interceptor:
       - On 401: clears localStorage token, redirects to /login
       - All other errors: re-throws (let the calling code handle)

TypeScript only, no 'any'. Export as named export 'apiClient'.
File: src/api/client.ts
```

**AI output — correct. One observation I noted:**

```typescript
// AI correctly handled the 401 redirect:
if (error.response?.status === 401) {
  localStorage.removeItem('token');
  window.location.href = '/login';
}

// I noted for presentation: in a production app, this would use
// React Router's navigate() via a shared instance, not window.location.
// window.location causes a full page reload which loses React state.
// For this demo, the behavior is acceptable and simpler to explain.
```

---

## Phase 6 — Docker Configuration

### Prompt 6.1 — Multi-stage Dockerfile for .NET 8

```
Write a production multi-stage Dockerfile for a .NET 8 ASP.NET Core API.

Stage 1 (build):
  - Base: mcr.microsoft.com/dotnet/sdk:8.0-alpine
  - Copy solution file and ALL .csproj files first (for layer caching)
  - Run dotnet restore
  - Copy remaining source
  - Run dotnet test (tests must pass before building — fail the build if not)
  - Run dotnet publish with Release config, output to /app/publish

Stage 2 (runtime):
  - Base: mcr.microsoft.com/dotnet/aspnet:8.0-alpine
  - Create a non-root user and group, run as that user
  - Copy published output from stage 1
  - Add HEALTHCHECK using wget (available in alpine)
  - Expose port 8080
  - ENTRYPOINT to run the DLL

Project structure:
  Solution file at root
  Projects in: backend/TaskManager.Domain/, backend/TaskManager.Application/,
  backend/TaskManager.Infrastructure/, backend/TaskManager.API/, backend/TaskManager.Tests/
```

**AI output — what I corrected:**

```dockerfile
# AI used curl for the health check — not installed in .NET alpine by default
HEALTHCHECK CMD curl -f http://localhost:8080/api/health

# My correction: use wget (available in alpine by default)
HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=3 \
    CMD wget -qO- http://localhost:8080/api/health || exit 1
```

```dockerfile
# AI ran tests with --no-build which requires a prior build step in the same stage
RUN dotnet test --no-build

# My correction: run tests without --no-build in the build stage context
RUN dotnet test backend/TaskManager.Tests/ \
    --configuration Release \
    --no-restore \
    --logger "console;verbosity=minimal"
# --no-restore is fine because restore already ran; --no-build is not
```

```dockerfile
# AI created user with adduser but used wrong flags for alpine (busybox)
RUN useradd -m appuser  # ❌ Not available in alpine

# Correct for alpine:
RUN addgroup -S appgroup && adduser -S appuser -G appgroup  # ✅
```

---

## Summary: What AI Does Well vs. Where It Needs Oversight

### AI consistently does well

| Task | Notes |
|------|-------|
| Boilerplate and CRUD patterns | Saves significant time on repetitive code |
| Syntax and API surface | Knows C# idioms, LINQ, async patterns |
| Test structure scaffolding | Correct xUnit/Moq/FluentAssertions syntax |
| SQL query generation | Correct parameterization when explicitly instructed |
| Documentation comments | XML doc comments, README structure |
| Standard patterns | Repository, Factory Method, Middleware — all correct |

### AI consistently needs oversight

| Failure mode | Example from this project | Why it matters |
|--------------|--------------------------|----------------|
| **Security shortcuts** | String interpolation in SQL | Pattern is wrong even when "safe" with GUIDs |
| **Exposing internals** | `500 → ex.Message` | Internal errors must never reach the client |
| **Stale library syntax** | React Query v4 syntax in a v5 project | AI training data skews toward older versions |
| **Missing negative verifications** | No `Times.Never` on error path tests | Partial side effects go undetected |
| **Platform assumptions** | `curl` in alpine, `useradd` in alpine | Alpine Linux uses busybox — different toolchain |
| **Architecture drift** | `try/catch` in controllers | AI defaults to "safe" patterns that violate architectural rules |
| **Skipping CancellationToken** | Missing on some async calls | Token propagation is incomplete — cancellation doesn't work end-to-end |
| **Layer boundary violations** | DTOs suggested in Domain layer | AI doesn't have context of your specific architectural rules |

### My prompt engineering principles

1. **State constraints as hard rules, not preferences**
   > "No try/catch in controllers. This is a hard constraint, not a guideline."
   > Better than: "Prefer not to use try/catch in controllers."

2. **Provide negative examples**
   > "Do NOT use string interpolation in SQL — use parameterized queries ONLY."

3. **Specify what NOT to include**
   > "Do not write the entity implementation — only the tests."

4. **Give the AI the interface, not the problem**
   > Providing `ITaskRepository` and `TaskItem.Reconstruct()` signatures means
   > the AI implements against your contract, not invents its own.

5. **Ask for reasoning before output**
   > "Argue both sides before giving your recommendation."
   > Surfaces the AI's assumptions — you can correct them before it writes code.

6. **Review with a security lens first**
   > Before checking architecture or style, check for: SQL injection, exposed error messages,
   > auth bypass, unparameterized queries, hardcoded secrets.

7. **Verify library versions explicitly**
   > Always include the target library version in the prompt.
   > "Use TanStack Query **v5** syntax (object-form useMutation, not positional arguments)."
