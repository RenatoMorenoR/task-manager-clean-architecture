using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces.UseCases;

public interface IUpdateTaskUseCase
{
    Task<TaskDto> ExecuteAsync(Guid taskId, UpdateTaskRequest request, CancellationToken ct = default);
}
