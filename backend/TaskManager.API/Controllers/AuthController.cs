using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces.UseCases;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IRegisterUserUseCase registerUseCase,
    ILoginUserUseCase loginUseCase) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var response = await registerUseCase.ExecuteAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await loginUseCase.ExecuteAsync(request, ct);
        return Ok(response);
    }
}
