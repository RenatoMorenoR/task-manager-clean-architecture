namespace TaskManager.Application.Interfaces.UseCases;

public interface IDeleteTaskUseCase
{
    Task ExecuteAsync(Guid taskId, CancellationToken ct = default);
}
