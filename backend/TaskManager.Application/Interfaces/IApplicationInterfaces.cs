namespace TaskManager.Application.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, string name);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated => UserId.HasValue;
}
