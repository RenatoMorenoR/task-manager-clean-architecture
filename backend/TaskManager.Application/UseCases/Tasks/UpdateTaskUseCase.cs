using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Tasks;

public class UpdateTaskUseCase(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : IUpdateTaskUseCase
{
    public async Task<TaskDto> ExecuteAsync(Guid taskId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId ?? throw new AuthenticationException("User not authenticated.");

        var task = await taskRepository.GetByIdAsync(taskId, ct) 
            ?? throw new NotFoundException("Task", taskId);

        task.EnsureOwnedBy(userId);

        task.Update(
            request.Title,
            request.Description,
            request.Status,
            request.DueDate
        );

        await taskRepository.UpdateAsync(task, ct);

        return new TaskDto(
            task.Id,
            task.UserId,
            task.Title,
            task.Description,
            task.Status.ToString(),
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}
