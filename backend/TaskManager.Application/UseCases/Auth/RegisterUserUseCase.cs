using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.UseCases.Auth;

public class RegisterUserUseCase(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRegisterUserUseCase
{
    public async Task<AuthResponse> ExecuteAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
        {
            throw new ConflictException("Email already in use.");
        }

        if (request.Password.Length < 8)
        {
            throw new DomainValidationException("Password must be at least 8 characters.");
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var user = User.Create(request.Email, passwordHash, request.Name);

        await userRepository.AddAsync(user, ct);

        var (token, expiresAt) = jwtTokenService.GenerateToken(user.Id, user.Email, user.Name);

        return new AuthResponse(
            token,
            user.Email,
            user.Name,
            expiresAt
        );
    }
}
