using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces.UseCases;

public interface ICreateTaskUseCase
{
    Task<TaskDto> ExecuteAsync(CreateTaskRequest request, CancellationToken ct = default);
}
