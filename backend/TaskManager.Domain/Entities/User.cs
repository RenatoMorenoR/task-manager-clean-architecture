using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { }

    public static User Create(string email, string passwordHash, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email is required.");
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainValidationException("Invalid email format.");
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Name is required.");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User Reconstruct(Guid id, string email, string passwordHash, string name, DateTime createdAt, DateTime updatedAt)
    {
        return new User
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            Name = name,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }
}
