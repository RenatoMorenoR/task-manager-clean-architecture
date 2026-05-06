using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Auth;

public class LoginUserUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : ILoginUserUseCase
{
    public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);
        
        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password.");
        }

        var (token, expiresAt) = jwtTokenService.GenerateToken(user.Id, user.Email, user.Name);

        return new AuthResponse(
            token,
            user.Email,
            user.Name,
            expiresAt
        );
    }
}
