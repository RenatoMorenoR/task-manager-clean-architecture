using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Tasks;

public class CreateTaskUseCase(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : ICreateTaskUseCase
{
    public async Task<TaskDto> ExecuteAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId ?? throw new AuthenticationException("User not authenticated.");

        var task = TaskItem.Create(
            userId,
            request.Title,
            request.Description,
            request.DueDate
        );

        var createdTask = await taskRepository.AddAsync(task, ct);

        return MapToDto(createdTask);
    }

    private static TaskDto MapToDto(TaskItem task) => new(
        task.Id,
        task.UserId,
        task.Title,
        task.Description,
        task.Status,
        task.DueDate,
        task.CreatedAt,
        task.UpdatedAt
    );
}
