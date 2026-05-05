using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Tasks;

public class DeleteTaskUseCase(
    ITaskRepository taskRepository,
    ICurrentUserService currentUserService) : IDeleteTaskUseCase
{
    public async Task ExecuteAsync(Guid taskId, CancellationToken ct = default)
    {
        var userId = currentUserService.UserId ?? throw new AuthenticationException("User not authenticated.");

        var task = await taskRepository.GetByIdAsync(taskId, ct) 
            ?? throw new NotFoundException("Task", taskId);

        task.EnsureOwnedBy(userId);

        await taskRepository.DeleteAsync(taskId, ct);
    }
}
