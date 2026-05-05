using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces.UseCases;

public interface IGetTasksUseCase
{
    Task<IEnumerable<TaskDto>> ExecuteAsync(CancellationToken ct = default);
}
