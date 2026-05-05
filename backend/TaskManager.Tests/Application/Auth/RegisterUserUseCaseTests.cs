using TaskManager.Application.UseCases.Auth;

namespace TaskManager.Tests.Application.Auth;

public class RegisterUserUseCaseTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly RegisterUserUseCase _useCase;

    public RegisterUserUseCaseTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _useCase = new RegisterUserUseCase(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Test User");
        var passwordHash = "hashed_password";
        var token = "jwt_token";

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns(passwordHash);

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        _jwtTokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(token);

        // Act
        var response = await _useCase.ExecuteAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Email.Should().Be(request.Email.ToLower());
        response.Name.Should().Be(request.Name);
        response.Token.Should().Be(token);

        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => u.Email == request.Email.ToLower()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new RegisterRequest("test@example.com", "Password123!", "Test User");
        var existingUser = User.Create(request.Email, "hash", "Existing");

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email already in use.");
    }
}
