using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private TaskItem() { }

    /// <summary>Creates a new TaskItem enforcing all domain invariants.</summary>
    public static TaskItem Create(Guid userId, string title, string description, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Title cannot be empty.");

        if (title.Length > 500)
            throw new DomainValidationException("Title cannot exceed 500 characters.");

        if (dueDate.Date < DateTime.UtcNow.Date)
            throw new DomainValidationException("Due date cannot be in the past.");

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Status = TaskItemStatus.Pending,
            DueDate = dueDate.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Reconstructs a TaskItem from persistent storage. Does NOT enforce creation invariants.</summary>
    public static TaskItem Reconstruct(
        Guid id, Guid userId, string title, string description,
        TaskItemStatus status, DateTime dueDate, DateTime createdAt, DateTime updatedAt)
    {
        return new TaskItem
        {
            Id = id,
            UserId = userId,
            Title = title,
            Description = description,
            Status = status,
            DueDate = dueDate,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Update(string title, string description, TaskItemStatus status, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainValidationException("Title cannot be empty.");

        if (dueDate.Date < DateTime.UtcNow.Date)
            throw new DomainValidationException("Due date cannot be in the past.");

        if (title.Length > 500)
            throw new DomainValidationException("Title cannot exceed 500 characters.");

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = status;
        DueDate = dueDate.ToUniversalTime();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Throws if the task is not owned by the given user.</summary>
    public void EnsureOwnedBy(Guid userId)
    {
        if (UserId != userId)
            throw new UnauthorizedTaskAccessException(Id, userId);
    }
}
