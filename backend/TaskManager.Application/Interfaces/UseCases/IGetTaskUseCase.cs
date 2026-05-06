using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.Interfaces.UseCases;

public interface IGetTaskUseCase
{
    Task<TaskDto> ExecuteAsync(Guid id, CancellationToken ct = default);
}
