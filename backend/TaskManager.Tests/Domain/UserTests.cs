namespace TaskManager.Tests.Domain;

public class UserTests
{
    private readonly string _validEmail = "test@example.com";
    private readonly string _validPasswordHash = "hashedpassword";
    private readonly string _validName = "Test User";

    [Fact]
    public void Create_WithValidData_ShouldReturnUser()
    {
        // Act
        var user = User.Create(_validEmail, _validPasswordHash, _validName);

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be(_validEmail);
        user.Name.Should().Be(_validName);
        user.PasswordHash.Should().Be(_validPasswordHash);
    }

    [Fact]
    public void Create_ShouldNormalizeEmailToLowercase()
    {
        // Arrange
        var mixedCaseEmail = "Test@Example.COM";

        // Act
        var user = User.Create(mixedCaseEmail, _validPasswordHash, _validName);

        // Assert
        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ShouldThrowDomainValidationException(string? invalidEmail)
    {
        // Act
        var act = () => User.Create(invalidEmail!, _validPasswordHash, _validName);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Email is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidPasswordHash_ShouldThrowDomainValidationException(string? invalidHash)
    {
        // Act
        var act = () => User.Create(_validEmail, invalidHash!, _validName);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Password hash is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainValidationException(string? invalidName)
    {
        // Act
        var act = () => User.Create(_validEmail, _validPasswordHash, invalidName!);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Name is required.");
    }
}
