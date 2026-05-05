using TaskManager.Application.DTOs.Auth;

namespace TaskManager.Application.Interfaces.UseCases;

public interface IRegisterUserUseCase
{
    Task<AuthResponse> ExecuteAsync(RegisterRequest request, CancellationToken ct = default);
}
