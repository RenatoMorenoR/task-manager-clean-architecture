# Product Requirements Document (PRD)

**Project:** TaskManager Application
**Version:** 1.0
**Date:** May 2026
**Status:** APPROVED

---

## Table of Contents

- [1. Executive Summary](#1-executive-summary)
- [2. User Story](#2-user-story)
- [3. Actors & Roles](#3-actors--roles)
- [4. Functional Requirements](#4-functional-requirements)
  - [4.1 Authentication](#41-authentication)
  - [4.2 Task Management](#42-task-management)
- [5. Data Model](#5-data-model)
  - [5.1 Task Entity](#51-task-entity)
  - [5.2 User Entity](#52-user-entity)
- [6. Validation Rules](#6-validation-rules)
- [7. Non-Functional Requirements](#7-non-functional-requirements)
- [8. API Endpoints Summary](#8-api-endpoints-summary)
- [9. Acceptance Criteria](#9-acceptance-criteria)
- [10. Demo Credentials & Setup](#10-demo-credentials--setup)
- [11. Out of Scope (v1.0)](#11-out-of-scope-v10)

---

## 1. Executive Summary

TaskManager is a personal task management web application that allows authenticated users to create, read, update, and delete their own tasks. The application is designed as a technical interview exercise showcasing Clean Architecture, Test-Driven Development, raw SQL data access, and modern full-stack development capabilities.

The system must demonstrate:
- A working RESTful API built with .NET 8 and ASP.NET Core
- A responsive React frontend that consumes the API
- PostgreSQL as the data store (raw Npgsql, no ORM)
- JWT-based authentication with per-user data isolation
- Clean Architecture with clear separation of concerns
- Comprehensive test coverage following TDD methodology

---

## 2. User Story

> *"As a registered user, I want to manage my personal tasks — create, view, edit, and delete them — so that I can organize my daily work, knowing that only I can see my own tasks."*

This story was chosen deliberately because:

1. The domain is instantly understandable — reviewers can focus on architecture, not domain explanation
2. The auth requirement emerges naturally — *"only I can see my tasks"* mandates JWT + ownership checks
3. The task fields (`title`, `description`, `status`, `due_date`) match the exercise specification exactly
4. Authorization is non-trivial — it's not just "is the user logged in?" but "does this user own this task?"

---

## 3. Actors & Roles

| Actor | Description |
|-------|-------------|
| **Anonymous User** | Can access registration and login endpoints only. All other endpoints return 401. |
| **Authenticated User** | Can perform full CRUD on their own tasks. Cannot read, modify, or delete another user's tasks. |

---

## 4. Functional Requirements

### 4.1 Authentication

| ID | Requirement | Acceptance Criteria | Priority |
|----|-------------|---------------------|----------|
| FR-01 | User Registration | `POST /api/auth/register` accepts `{ email, password, name }`. Creates account and returns JWT. Email must be unique — returns 409 on conflict. Password min 8 characters — returns 400 if too short. | Must Have |
| FR-02 | User Login | `POST /api/auth/login` accepts `{ email, password }`. Returns JWT on success. Returns 401 on wrong credentials. | Must Have |
| FR-03 | Token Expiration | JWT tokens expire after 24 hours. Requests with expired tokens return 401. | Must Have |
| FR-04 | Protected Routes | All `/api/tasks` endpoints require a valid Bearer token. Requests without a token return 401. | Must Have |

### 4.2 Task Management

| ID | Feature | Acceptance Criteria | Priority |
|----|---------|---------------------|----------|
| FR-05 | Create Task | `POST /api/tasks` creates a task for the authenticated user. Title required. Due date required, cannot be in the past. Returns 201 with `Location` header pointing to the new resource. | Must Have |
| FR-06 | List Tasks | `GET /api/tasks` returns only the current user's tasks, ordered by `created_at` descending. Never returns another user's tasks. | Must Have |
| FR-07 | Get Single Task | `GET /api/tasks/{id}` returns the task if owned by the authenticated user. Returns 403 if the task exists but belongs to another user. Returns 404 if not found. | Must Have |
| FR-08 | Update Task | `PUT /api/tasks/{id}` updates title, description, status, and due date. Returns 403 if not the owner. Returns 404 if not found. | Must Have |
| FR-09 | Delete Task | `DELETE /api/tasks/{id}` permanently removes the task. Returns 204 No Content. Returns 403 if not the owner. | Must Have |
| FR-10 | Health Check | `GET /api/health` returns 200 with current timestamp. No authentication required. Used by Docker health checks. | Must Have |

---

## 5. Data Model

### 5.1 Task Entity

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `id` | UUID | Primary Key | Auto-generated (uuid_generate_v4) |
| `user_id` | UUID | FK → users.id, ON DELETE CASCADE | Owner of the task |
| `title` | VARCHAR(500) | NOT NULL | Task title, cannot be empty |
| `description` | TEXT | DEFAULT '' | Optional task details |
| `status` | SMALLINT | NOT NULL, CHECK (0–3) | 0=Pending, 1=InProgress, 2=Completed, 3=Cancelled |
| `due_date` | TIMESTAMPTZ | NOT NULL | Task deadline (cannot be past on creation) |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | Auto-set on insert |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | Auto-updated by DB trigger |

**Indexes:** `idx_tasks_user_id`, `idx_tasks_status`, `idx_tasks_due_date`

### 5.2 User Entity

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `id` | UUID | Primary Key | Auto-generated (uuid_generate_v4) |
| `email` | VARCHAR(255) | NOT NULL, UNIQUE | Stored lowercased |
| `password_hash` | TEXT | NOT NULL | BCrypt hash (cost factor 12) |
| `name` | VARCHAR(255) | NOT NULL | Display name |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | Auto-set on insert |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | Auto-updated by DB trigger |

**Indexes:** `idx_users_email`

### Domain model diagram

```
User
├── id: Guid
├── email: string (unique, lowercase)
├── passwordHash: string
├── name: string
├── createdAt: DateTime (UTC)
└── updatedAt: DateTime (UTC)

TaskItem                          1         *
├── id: Guid          ←──────── User.id ──────
├── userId: Guid
├── title: string (1–500 chars)
├── description: string
├── status: TaskItemStatus [Pending|InProgress|Completed|Cancelled]
├── dueDate: DateTime (UTC, not past on creation)
├── createdAt: DateTime (UTC)
└── updatedAt: DateTime (UTC)
```

---

## 6. Validation Rules

| Field | Rule | Error |
|-------|------|-------|
| `title` | Required, 1–500 characters, not whitespace-only | 400 — "Title cannot be empty." |
| `due_date` (on create) | Required, must not be before today's date | 400 — "Due date cannot be in the past." |
| `status` (on update) | Must be one of: `Pending`, `InProgress`, `Completed`, `Cancelled` | 400 |
| `email` | Required, valid email format, unique in system | 400 / 409 |
| `password` | Required, minimum 8 characters | 400 — "Password must be at least 8 characters." |
| `name` | Required, not whitespace-only | 400 — "Name cannot be empty." |

---

## 7. Non-Functional Requirements

| Category | Requirement |
|----------|-------------|
| **Security** | All task endpoints require JWT Bearer authentication. Ownership enforced on every mutating operation (update, delete) and read operation (get by id). |
| **Security** | All SQL queries use parameterized statements — no string interpolation. Passwords hashed with BCrypt, minimum work factor 12. |
| **Security** | No secrets in source code. JWT key, DB credentials injected via environment variables. |
| **Data isolation** | A user can never retrieve, modify, or delete another user's tasks, even if they know the UUID. Returns 403, not 404, to avoid confirming task existence. |
| **Performance** | PostgreSQL indexes on `tasks.user_id` and `users.email`. `NpgsqlDataSource` provides built-in connection pooling (registered as singleton). |
| **Testability** | Domain and Application layers fully testable with no database or HTTP server. 80%+ overall test coverage target. |
| **Portability** | Full stack runs with a single `docker compose up` command. No local .NET SDK or Node.js required. |
| **Documentation** | Swagger/OpenAPI available at `/swagger`. README includes complete local and Docker setup instructions. |
| **Code quality** | No warnings in compiler output. Controllers under 15 lines per action. No business logic outside Domain or Application layers. |

---

## 8. API Endpoints Summary

| Method | Endpoint | Auth | Request Body | Success Status |
|--------|----------|------|--------------|----------------|
| POST | `/api/auth/register` | No | `{ email, password, name }` | 200 |
| POST | `/api/auth/login` | No | `{ email, password }` | 200 |
| GET | `/api/health` | No | — | 200 |
| GET | `/api/tasks` | JWT | — | 200 |
| POST | `/api/tasks` | JWT | `{ title, description, dueDate }` | 201 + Location |
| GET | `/api/tasks/{id}` | JWT | — | 200 |
| PUT | `/api/tasks/{id}` | JWT | `{ title, description, status, dueDate }` | 200 |
| DELETE | `/api/tasks/{id}` | JWT | — | 204 |

**AuthResponse (register + login):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "name": "John Doe",
  "expiresAt": "2026-05-06T12:00:00Z"
}
```

**TaskDto:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11",
  "title": "Review Clean Architecture book",
  "description": "Read chapters 5-8 and take notes",
  "status": "InProgress",
  "dueDate": "2026-05-10T00:00:00Z",
  "createdAt": "2026-05-04T10:00:00Z",
  "updatedAt": "2026-05-05T08:30:00Z"
}
```

---

## 9. Acceptance Criteria

### Authentication scenarios

```
GIVEN I am a new visitor
WHEN I POST /api/auth/register with valid email, password (≥8 chars), and name
THEN I receive 200 with a JWT token valid for 24 hours

GIVEN I am a registered user
WHEN I POST /api/auth/login with correct credentials
THEN I receive 200 with a fresh JWT token

GIVEN I am a registered user
WHEN I POST /api/auth/login with wrong password
THEN I receive 401 with error message "Invalid email or password."

GIVEN I try to register with an email already in use
WHEN I POST /api/auth/register
THEN I receive 409 Conflict
```

### Task management scenarios

```
GIVEN I am authenticated
WHEN I POST /api/tasks with a valid title and future due date
THEN I receive 201 with the created task and a Location header

GIVEN I am authenticated
WHEN I POST /api/tasks with an empty title
THEN I receive 400 with message "Title cannot be empty."

GIVEN I am authenticated
WHEN I POST /api/tasks with a past due date
THEN I receive 400 with message "Due date cannot be in the past."

GIVEN I am authenticated
WHEN I GET /api/tasks
THEN I receive 200 with an array containing only MY tasks, newest first

GIVEN I am authenticated and own Task A
WHEN I PUT /api/tasks/{A.id} with new title and status "Completed"
THEN I receive 200 with the updated task

GIVEN I am authenticated and own Task A
WHEN I DELETE /api/tasks/{A.id}
THEN I receive 204 and the task no longer exists

GIVEN I am authenticated as User A
WHEN I try to GET, PUT, or DELETE a task belonging to User B
THEN I receive 403 Forbidden (not 404)

GIVEN I am not authenticated
WHEN I request any /api/tasks endpoint
THEN I receive 401 Unauthorized
```

---

## 10. Demo Credentials & Setup

| Item | Value |
|------|-------|
| Demo email | `demo@taskmanager.com` |
| Demo password | `Demo1234!` |
| API URL | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| Frontend | http://localhost:3000 |
| Start command | `make up` or `docker compose -f docker-compose.yml up -d` |

The demo account is pre-seeded with 5 tasks across different statuses:

| Task | Status |
|------|--------|
| Review Clean Architecture book | InProgress |
| Prepare technical interview presentation | Pending |
| Write unit tests for domain layer | Completed |
| Configure Docker environment | Completed |
| Implement JWT authentication | Pending |

**Reset demo data at any time:**
```bash
make db-reset
```

---

## 11. Out of Scope (v1.0)

The following features are intentionally excluded from this version:

- Task sharing between users
- File attachments on tasks
- Email notifications or due-date reminders
- Task categories, labels, or tags
- Pagination (fewer than 100 tasks expected in demo)
- Password reset / forgot password flow
- OAuth / social login (Google, GitHub)
- Admin role with cross-user visibility
- Soft delete (tasks are permanently removed on DELETE)
- Audit log / change history
