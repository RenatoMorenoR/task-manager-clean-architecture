using TaskManager.Domain.Entities;

namespace TaskManager.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default);
    Task UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
