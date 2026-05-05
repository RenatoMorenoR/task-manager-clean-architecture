using TaskManager.Application.DTOs.Auth;

namespace TaskManager.Application.Interfaces.UseCases;

public interface ILoginUserUseCase
{
    Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default);
}
