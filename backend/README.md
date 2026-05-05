# TaskManager Backend

This is the `.NET 8` API layer of the TaskManager application, built following strict Clean Architecture and SOLID principles.

## Architecture

The backend is separated into 4 distinct layers to enforce dependency inversion and separation of concerns:

- **Domain (`TaskManager.Domain`)**: Contains the core business entities (`TaskItem`, `User`), domain exceptions, and repository interfaces. This layer has zero external dependencies.
- **Application (`TaskManager.Application`)**: Contains Use Cases (e.g., `CreateTaskUseCase`), DTOs, and application interfaces. Uses dependency injection to orchestrate domain logic without touching the database directly.
- **Infrastructure (`TaskManager.Infrastructure`)**: Implements the interfaces defined in the Domain layer. Uses `Npgsql` for raw SQL PostgreSQL access, `BCrypt` for password hashing, and handles JWT token generation.
- **API (`TaskManager.API`)**: The presentation layer. Contains ASP.NET Core Controllers, global exception handling middleware (RFC 7807 Problem Details format), and configures the DI container.

## Requirements & Constraints

This backend was built specifically without any ORMs (Entity Framework, Dapper) or mediator patterns (MediatR), showcasing proficiency in raw SQL (`NpgsqlDataSource`), proper parameterization, and manual Dependency Injection orchestration.

## Running Locally

```bash
cd TaskManager.API
dotnet restore
dotnet build
dotnet run
```

For hot-reload during development:
```bash
dotnet watch run
```

*Note: Requires a running instance of PostgreSQL on port 5432 with the database schema applied from the `scripts/` directory.*

## Testing

The testing suite (`TaskManager.Tests`) covers unit and integration scenarios using `xUnit`, `Moq`, and `FluentAssertions`.

```bash
dotnet test
```

Tests are grouped by layer:
- **Domain**: Pure unit tests verifying business invariants.
- **Application**: Unit tests mocking infrastructure dependencies.
- **Infrastructure**: Integration tests against a real DB.
- **API**: Full integration tests using `WebApplicationFactory`.
