using Npgsql;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Infrastructure.Repositories;

public class UserRepository(NpgsqlDataSource dataSource) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var cmd = dataSource.CreateCommand("SELECT id, email, password_hash, name, created_at, updated_at FROM users WHERE id = @id");
        cmd.Parameters.AddWithValue("id", id);
        
        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var cmd = dataSource.CreateCommand("SELECT id, email, password_hash, name, created_at, updated_at FROM users WHERE email = @email");
        cmd.Parameters.AddWithValue("email", email.ToLowerInvariant());

        using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO users (id, email, password_hash, name, created_at, updated_at)
            VALUES (@id, @email, @password_hash, @name, @created_at, @updated_at)
            RETURNING id, email, password_hash, name, created_at, updated_at";

        using var cmd = dataSource.CreateCommand(sql);
        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("name", user.Name);
        cmd.Parameters.AddWithValue("created_at", user.CreatedAt);
        cmd.Parameters.AddWithValue("updated_at", user.UpdatedAt);

        using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return MapUser(reader);
    }

    private static User MapUser(NpgsqlDataReader reader) => User.Reconstruct(
        reader.GetGuid(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.GetDateTime(4),
        reader.GetDateTime(5)
    );
}
