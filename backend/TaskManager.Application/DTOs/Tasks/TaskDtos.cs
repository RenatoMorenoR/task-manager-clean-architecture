using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string Description,
    DateTime DueDate
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    TaskItemStatus Status,
    DateTime DueDate
);

public record TaskDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Description,
    string Status,
    DateTime DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
