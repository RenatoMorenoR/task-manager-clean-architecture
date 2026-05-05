using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Tasks;

public class GetTasksUseCase(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : IGetTasksUseCase
{
    public async Task<IEnumerable<TaskDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var userId = currentUserService.UserId ?? throw new AuthenticationException("User not authenticated.");

        var tasks = await taskRepository.GetByUserIdAsync(userId, ct);

        return tasks.Select(MapToDto).OrderByDescending(x => x.CreatedAt);
    }

    private static TaskDto MapToDto(TaskItem task) => new(
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
