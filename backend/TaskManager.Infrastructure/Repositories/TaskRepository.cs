using Npgsql;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository(NpgsqlDataSource dataSource) : ITaskRepository
{
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var cmd = dataSource.CreateCommand("SELECT id, user_id, title, description, status, due_date, created_at, updated_at FROM tasks WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapTask(reader);
        }
        return null;
    }

    public async Task<IEnumerable<TaskItem>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        using var cmd = dataSource.CreateCommand("SELECT id, user_id, title, description, status, due_date, created_at, updated_at FROM tasks WHERE user_id = @userId ORDER BY created_at DESC");
        cmd.Parameters.AddWithValue("userId", userId);

        var tasks = new List<TaskItem>();
        using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<TaskItem> AddAsync(TaskItem task, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO tasks (id, user_id, title, description, status, due_date, created_at, updated_at)
            VALUES (@id, @userId, @title, @description, @status, @dueDate, @createdAt, @updatedAt)
            RETURNING id, user_id, title, description, status, due_date, created_at, updated_at";

        using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", task.Id);
        cmd.Parameters.AddWithValue("userId", task.UserId);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", task.Description);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", task.DueDate);
        cmd.Parameters.AddWithValue("createdAt", task.CreatedAt);
        cmd.Parameters.AddWithValue("updatedAt", task.UpdatedAt);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return MapTask(reader);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        const string sql = @"
            UPDATE tasks SET 
                title = @title, 
                description = @description, 
                status = @status, 
                due_date = @dueDate, 
                updated_at = @updatedAt
            WHERE id = @id";

        using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", task.Id);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", task.Description);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", task.DueDate);
        cmd.Parameters.AddWithValue("updatedAt", task.UpdatedAt);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using var cmd = dataSource.CreateCommand("DELETE FROM tasks WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static TaskItem MapTask(NpgsqlDataReader reader) => TaskItem.Reconstruct(
        reader.GetGuid(0),
        reader.GetGuid(1),
        reader.GetString(2),
        reader.GetString(3),
        (TaskItemStatus)reader.GetInt16(4),
        reader.GetDateTime(5),
        reader.GetDateTime(6),
        reader.GetDateTime(7)
    );
}
