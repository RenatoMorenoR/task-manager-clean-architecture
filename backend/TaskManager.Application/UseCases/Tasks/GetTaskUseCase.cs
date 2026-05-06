using TaskManager.Application.DTOs.Tasks;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.UseCases.Tasks;

public class GetTaskUseCase(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : IGetTaskUseCase
{
    public async Task<TaskDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId ?? throw new AuthenticationException("User not authenticated.");

        var task = await taskRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("TaskItem", id);

        task.EnsureOwnedBy(userId);

        return new TaskDto(
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
}
