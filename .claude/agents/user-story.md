# User Story — TaskManager Application

## The Informal User Story

> *"As a professional who juggles multiple responsibilities daily, I want a simple and secure place to capture, track, and complete my personal tasks — so I can focus on what matters, confident that my to-do list is private, always up to date, and accessible from anywhere."*

---

## Why This Story

This story was chosen deliberately for the interview context, not arbitrarily.

**Domain simplicity is a feature, not a limitation.** When the domain is instantly
understandable, the interviewers can focus their attention entirely on the architecture,
the code quality, and the engineering decisions — which is exactly what the exercise
evaluates. A complex domain (e-commerce, logistics, healthcare) would bury the
architectural signal in domain noise.

Beyond simplicity, the story earns its place because it generates genuine engineering challenges:

| Challenge | Why it matters in this exercise |
|-----------|----------------------------------|
| **Authentication** | Forces JWT implementation, token lifecycle, and secure password storage |
| **Authorization** | "Only I can see my tasks" is not just "is logged in" — it's per-resource ownership, requiring domain-level enforcement |
| **Domain invariants** | A task with an empty title or past due date must be impossible to create — enforced in the entity, not the controller |
| **Data isolation** | Two users can never see each other's data, even with the correct UUID |
| **Clean boundaries** | The ownership rule lives in the Domain layer, not leaked into Infrastructure or API |

---

## Actors

| Actor | Description |
|-------|-------------|
| **Anonymous Visitor** | No account. Can only access `/api/auth/register` and `/api/auth/login`. Every other endpoint returns `401 Unauthorized`. |
| **Registered User** | Has an account and a valid JWT token. Can create, read, update, and delete **their own** tasks. Cannot touch another user's tasks under any circumstance. |

---

## Epic Breakdown

The single user story decomposes into three epics:

```
Epic 1: Identity & Access
  └── Story 1.1: Register an account
  └── Story 1.2: Log in and receive a token
  └── Story 1.3: Token expiration and re-authentication

Epic 2: Task Lifecycle
  └── Story 2.1: Capture a new task
  └── Story 2.2: View my task list
  └── Story 2.3: Inspect a single task
  └── Story 2.4: Update a task
  └── Story 2.5: Change task status
  └── Story 2.6: Delete a task

Epic 3: Data Security
  └── Story 3.1: Tasks are private by default
  └── Story 3.2: Cross-user access is explicitly blocked
```

---

## Story 1.1 — Register an Account

**As a** new visitor,
**I want to** create an account with my email and a password,
**so that** I have a private workspace where only I can manage my tasks.

### Acceptance Criteria

```
GIVEN I am a new visitor with no account
WHEN I submit a registration form with a valid email, a password of at least
     8 characters, and my display name
THEN my account is created
AND I immediately receive a JWT token valid for 24 hours
AND I am taken directly to my (empty) task list

GIVEN I try to register with an email that already exists in the system
WHEN I submit the registration form
THEN I receive a 409 Conflict error
AND the error message tells me the email is already registered
AND no duplicate account is created

GIVEN I submit a password shorter than 8 characters
WHEN I submit the registration form
THEN I receive a 400 Bad Request
AND my account is NOT created

GIVEN I submit an invalid email format (e.g. "notanemail")
WHEN I submit the registration form
THEN I receive a 400 Bad Request
```

### Implementation notes

- Password stored as BCrypt hash (work factor 12) — never in plain text, never logged
- Email normalized to lowercase before storage and all future lookups
- JWT returned in response body for demo simplicity — production would use `HttpOnly` cookie to prevent XSS

---

## Story 1.2 — Log In and Receive a Token

**As a** registered user,
**I want to** log in with my email and password,
**so that** I can access my task list from any device.

### Acceptance Criteria

```
GIVEN I am a registered user
WHEN I submit my correct email and password
THEN I receive a JWT token valid for 24 hours
AND I am taken to my task list

GIVEN I submit an email that does not exist in the system
WHEN I attempt to log in
THEN I receive 401 Unauthorized
AND the message is "Invalid email or password." (intentionally vague)

GIVEN I submit the correct email but wrong password
WHEN I attempt to log in
THEN I receive 401 Unauthorized
AND the message is identical: "Invalid email or password."
```

### Security note

The error message is **identical** whether the email doesn't exist or the password is wrong.
This is intentional — revealing which one is wrong allows an attacker to enumerate registered
email addresses. Same response, same HTTP status, same latency.

---

## Story 1.3 — Token Expiration

**As a** logged-in user,
**I want to** be automatically logged out after 24 hours,
**so that** my account is protected if I forget to log out on a shared device.

### Acceptance Criteria

```
GIVEN my JWT token has expired (after 24 hours)
WHEN I make any request to a protected endpoint
THEN I receive 401 Unauthorized
AND the frontend clears my local token
AND I am redirected to the login page
```

---

## Story 2.1 — Capture a New Task

**As a** registered user,
**I want to** quickly add a new task with a title, optional description, and a due date,
**so that** I don't forget something I need to do.

### Acceptance Criteria

```
GIVEN I am authenticated
WHEN I submit a new task with a non-empty title (max 500 chars), an optional
     description, and a due date that is today or in the future
THEN the task is created with status "Pending"
AND I receive the created task with its generated UUID
AND the response includes a Location header pointing to GET /api/tasks/{id}
AND the task appears at the top of my task list

GIVEN I submit a task with an empty or whitespace-only title
WHEN I try to create the task
THEN I receive 400 Bad Request: "Title cannot be empty."
AND the task is NOT created
AND the database INSERT is never attempted

GIVEN I submit a task with a due date in the past
WHEN I try to create the task
THEN I receive 400 Bad Request: "Due date cannot be in the past."
AND the task is NOT created

GIVEN I submit a title longer than 500 characters
WHEN I try to create the task
THEN I receive 400 Bad Request
```

### Domain rule

`TaskItem.Create()` enforces all invariants at the entity level. If the title is empty
or the due date is past, a `DomainValidationException` is thrown **before** any
repository call. No controller, service, or middleware can bypass this — it is physically
impossible to construct an invalid `TaskItem`.

---

## Story 2.2 — View My Task List

**As a** registered user,
**I want to** see all my tasks in one place, newest first,
**so that** I always have a full picture of what needs to be done.

### Acceptance Criteria

```
GIVEN I am authenticated and have tasks in the system
WHEN I request GET /api/tasks
THEN I receive all my tasks ordered by created_at descending
AND none of another user's tasks appear in the list (SQL filters by user_id)
AND each task includes: id, title, description, status, dueDate, createdAt, updatedAt

GIVEN I am authenticated but have no tasks yet
WHEN I request GET /api/tasks
THEN I receive an empty array []  (not 404)

GIVEN I am not authenticated
WHEN I request GET /api/tasks
THEN I receive 401 Unauthorized
```

---

## Story 2.3 — Inspect a Single Task

**As a** registered user,
**I want to** open a specific task to see its full details,
**so that** I can read the description and check the status.

### Acceptance Criteria

```
GIVEN I am authenticated and own Task A
WHEN I request GET /api/tasks/{A.id}
THEN I receive the full task details with 200 OK

GIVEN I am authenticated as User A
WHEN I request a task UUID that belongs to User B
THEN I receive 403 Forbidden
AND I do NOT receive 404

GIVEN I request a UUID that does not exist in the database
WHEN I make the request
THEN I receive 404 Not Found
```

### Design decision: 403, not 404, for cross-user access

Returning 404 when a resource exists but belongs to someone else is "security through
obscurity." Returning 403 is semantically more accurate: the resource **exists**, but
the caller is **not authorized**. Both prevent data leakage equally in practice.
The 403 approach is more honest and easier to debug during development.

---

## Story 2.4 — Update a Task

**As a** registered user,
**I want to** edit a task's title, description, status, and due date,
**so that** my task list stays accurate as things change.

### Acceptance Criteria

```
GIVEN I am authenticated and own Task A
WHEN I submit a PUT request with updated title, description, status, and due date
THEN the task is updated in the database
AND I receive the updated task with a refreshed updatedAt timestamp

GIVEN I submit an update with an empty title
WHEN I try to update the task
THEN I receive 400 Bad Request
AND the task is NOT modified (entity.Update() throws before any DB call)

GIVEN I am authenticated as User A
WHEN I try to update a task belonging to User B
THEN I receive 403 Forbidden
AND the task is NOT modified

GIVEN the task UUID does not exist
WHEN I try to update it
THEN I receive 404 Not Found
```

---

## Story 2.5 — Change Task Status

**As a** registered user,
**I want to** mark a task as In Progress, Completed, or Cancelled,
**so that** I can track where each task stands in its lifecycle.

### Task status lifecycle

```
                    ┌─────────────────────────────────┐
                    │                                 │
  ┌─────────────┐   │   ┌────────────────┐            │
  │   Pending   │───┼──▶│   InProgress   │            │
  └──────┬──────┘   │   └───────┬────────┘            │
         │          │           │                     │
         │          └───────────┼─────────────────────┘
         │                      │
         │              ┌───────▼────────┐
         └─────────────▶│   Completed    │
                         └────────────────┘
         │
         │              ┌────────────────┐
         └─────────────▶│   Cancelled    │
                         └────────────────┘
```

Valid values: `Pending` | `InProgress` | `Completed` | `Cancelled`

No forward/backward restrictions in v1.0 — any status can transition to any other status.

### Acceptance Criteria

```
GIVEN a task with any status
WHEN I update it with a valid status value
THEN the status changes and updatedAt is refreshed

GIVEN I submit a status value outside the valid enum
WHEN I try to update the task
THEN I receive 400 Bad Request
```

---

## Story 2.6 — Delete a Task

**As a** registered user,
**I want to** permanently remove a task I no longer need,
**so that** my task list stays clean and focused.

### Acceptance Criteria

```
GIVEN I am authenticated and own Task A
WHEN I send DELETE /api/tasks/{A.id}
THEN I receive 204 No Content
AND the task is permanently removed from the database
AND a subsequent GET /api/tasks/{A.id} returns 404

GIVEN I am authenticated as User A
WHEN I try to delete a task belonging to User B
THEN I receive 403 Forbidden
AND User B's task is NOT deleted

GIVEN the task UUID does not exist
WHEN I try to delete it
THEN I receive 404 Not Found
```

---

## Stories 3.1 & 3.2 — Data Privacy and Cross-User Isolation

**As a** registered user,
**I want to** be certain that no one else can see or modify my tasks,
**so that** I can use the system with sensitive personal and professional information.

### How isolation is enforced — layered defense

Isolation is not enforced in one place. It is enforced at every layer independently
so that a bug in one layer does not compromise the entire guarantee.

**Layer 1 — SQL query (Infrastructure)**
```sql
-- List: only returns rows where user_id matches the authenticated user
SELECT * FROM tasks WHERE user_id = @userId ORDER BY created_at DESC

-- Get/Update/Delete: retrieves by primary key first, then layer 2 checks ownership
SELECT * FROM tasks WHERE id = @id
```

**Layer 2 — Domain entity (Domain)**
```csharp
// Applied on every GetById, Update, and Delete use case
task.EnsureOwnedBy(currentUser.UserId);
// Throws UnauthorizedTaskAccessException if IDs don't match
```

**Layer 3 — HTTP response mapping (API middleware)**
```
UnauthorizedTaskAccessException  →  403 Forbidden
```

No layer trusts the others. Even if the SQL accidentally returned the wrong row,
the domain check would catch it. Defense in depth.

---

## Complete Flow Diagrams

### Registration + First Task

```
User                  Frontend              API                    DB
 │                        │                  │                      │
 ├─ Fill register form ──▶│                  │                      │
 │                        ├─ POST /register ▶│                      │
 │                        │                  ├─ Validate email/pass  │
 │                        │                  ├─ BCrypt hash password  │
 │                        │                  ├─ User.Create()         │
 │                        │                  ├─ INSERT user ─────────▶│
 │                        │                  │◀─ user row ────────────│
 │                        │                  ├─ Generate JWT          │
 │                        │◀─ 200 + token ───│                      │
 ├◀─ Redirect to /tasks ──│                  │                      │
 │                        │                  │                      │
 ├─ Fill task form ───────│                  │                      │
 │                        ├─ POST /tasks ────▶│                      │
 │                        │  Bearer token    ├─ Validate JWT          │
 │                        │                  ├─ Extract userId        │
 │                        │                  ├─ TaskItem.Create()     │
 │                        │                  │  (validates title/date)│
 │                        │                  ├─ INSERT task ─────────▶│
 │                        │                  │◀─ task row ────────────│
 │                        │◀─ 201 + task ────│                      │
 ├◀─ Task in list ────────│                  │                      │
```

### Cross-User Unauthorized Access Attempt

```
Attacker (User A)       API                   Domain              DB
      │                   │                      │                  │
      ├─ GET /tasks/xyz ──▶│                      │                  │
      │  (valid token)    ├─ Validate JWT          │                  │
      │                   ├─ userId = A            │                  │
      │                   ├─ SELECT task xyz ─────────────────────────▶│
      │                   │◀─ task (owner = B) ─────────────────────────│
      │                   ├─ task.EnsureOwnedBy(A) ▶│                 │
      │                   │                      ├─ A ≠ B → throw     │
      │                   │◀─ UnauthorizedTaskAccessException ──────────│
      │                   ├─ Map → 403 Forbidden  │                  │
      │◀─ 403 Forbidden ──│                      │                  │
      │  (task exists,    │                      │                  │
      │   but not yours)  │                      │                  │
```

### Validation Failure — Empty Title

```
User                  Frontend              API                 Domain
 │                        │                  │                     │
 ├─ Submit empty title ──▶│                  │                     │
 │                        ├─ POST /tasks ────▶│                     │
 │                        │                  ├─ Call CreateTaskUseCase
 │                        │                  ├─ TaskItem.Create() ──▶│
 │                        │                  │                     ├─ title is empty
 │                        │                  │                     ├─ throw DomainValidationException
 │                        │                  │◀─ Exception ──────────│
 │                        │                  ├─ GlobalExceptionMiddleware
 │                        │                  ├─ Map → 400 Bad Request
 │                        │◀─ 400 + message ─│
 ├◀─ Show error ──────────│                  │
 │  "Title cannot         │                  │  NOTE: DB never called.
 │   be empty."           │                  │  Repository.CreateAsync()
 │                        │                  │  was never reached.
```

---

## Demo Script for the Interview Presentation

> Walk through this sequence when presenting to the technical panel.
> Total time: approximately 5–7 minutes.

**Step 1 — Start the application**
```bash
make up
# Wait for: taskmanager_api  Started
# Wait for: taskmanager_frontend  Started
```

**Step 2 — Open Swagger UI**
```
Navigate to: http://localhost:5000/swagger
```
*"The API is fully documented via Swagger. I can use it to demo all endpoints."*

**Step 3 — Register a user**
```
POST /api/auth/register
Body: { "email": "demo@taskmanager.com", "password": "Demo1234!", "name": "Demo User" }
```
*"Registration hashes the password with BCrypt, stores the user in PostgreSQL, and returns a JWT signed with HMAC-SHA256."*

**Step 4 — Authorize in Swagger**
```
Click 🔒 Authorize → Bearer <paste token>
```

**Step 5 — Create 3 tasks**
```
POST /api/tasks → "Review Clean Architecture book"    dueDate: +3 days
POST /api/tasks → "Prepare interview presentation"    dueDate: +5 days
POST /api/tasks → "Write domain unit tests"           dueDate: tomorrow
```
*"Each task is created with Pending status. Notice the 201 + Location header on each response."*

**Step 6 — List tasks**
```
GET /api/tasks
```
*"Three tasks returned, newest first. The userId on each matches the logged-in user — enforced at the SQL level."*

**Step 7 — Update a task**
```
PUT /api/tasks/{id}
Body: { "title": "Write domain unit tests", "description": "TaskItem.Create, Update, EnsureOwnedBy", "status": "Completed", "dueDate": "..." }
```
*"Status is now Completed. The updatedAt timestamp has changed — updated by a PostgreSQL trigger, not application code."*

**Step 8 — Delete a task**
```
DELETE /api/tasks/{id}  →  204 No Content
GET    /api/tasks/{id}  →  404 Not Found
```
*"Hard delete. The task is gone. 404 confirms it."*

**Step 9 — Demonstrate authorization (the key scenario)**
```
POST /api/auth/register  →  { "email": "attacker@test.com", "password": "Test1234!", "name": "Attacker" }
POST /api/auth/login     →  get attacker token
Authorize with attacker token
GET /api/tasks/{id of demo user's task}  →  403 Forbidden
```
*"The task exists, but it belongs to the demo user. The domain layer — specifically TaskItem.EnsureOwnedBy() — throws before we return any data. The attacker gets 403, not 200 and not 404."*

**Step 10 — Show the frontend**
```
Navigate to: http://localhost:3000
Log in with demo@taskmanager.com / Demo1234!
```
*"The React frontend consumes the same API. CRUD operations work through the UI."*
