using TaskManager.Application.UseCases.Auth;

namespace TaskManager.Tests.Application.Auth;

public class LoginUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly LoginUserUseCase _useCase;

    public LoginUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _useCase = new LoginUserUseCase(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = User.Create(request.Email, "hashed_password", "Test User");
        var token = "jwt_token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenServiceMock.Setup(x => x.GenerateToken(user.Id, user.Email, user.Name))
            .Returns((token, DateTime.UtcNow.AddHours(24)));

        // Act
        var response = await _useCase.ExecuteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Token.Should().Be(token);
        response.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongEmail_ShouldThrowAuthenticationException()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@example.com", "Password123!");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongPassword_ShouldThrowAuthenticationException()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword");
        var user = User.Create(request.Email, "hashed_password", "Test User");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>()
            .WithMessage("Invalid email or password.");
    }
}
