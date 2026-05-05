namespace TaskManager.Tests.Domain;

public class TaskItemTests
{
    private readonly Guid _validUserId = Guid.NewGuid();
    private readonly string _validTitle = "Test Task";
    private readonly string _validDescription = "Test Description";
    private readonly DateTime _validDueDate = DateTime.UtcNow.AddDays(1);

    [Fact]
    public void Create_WithValidData_ShouldReturnTaskItem()
    {
        // Act
        var task = TaskItem.Create(_validUserId, _validTitle, _validDescription, _validDueDate);

        // Assert
        task.Id.Should().NotBeEmpty();
        task.UserId.Should().Be(_validUserId);
        task.Title.Should().Be(_validTitle);
        task.Description.Should().Be(_validDescription);
        task.Status.Should().Be(TaskItemStatus.Pending);
        task.DueDate.Should().BeCloseTo(_validDueDate, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldThrowDomainValidationException(string? invalidTitle)
    {
        // Act
        var act = () => TaskItem.Create(_validUserId, invalidTitle!, _validDescription, _validDueDate);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Title cannot be empty.");
    }

    [Fact]
    public void Create_WithLongTitle_ShouldThrowDomainValidationException()
    {
        // Arrange
        var longTitle = new string('a', 501);

        // Act
        var act = () => TaskItem.Create(_validUserId, longTitle, _validDescription, _validDueDate);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Title cannot exceed 500 characters.");
    }

    [Fact]
    public void Create_WithPastDueDate_ShouldThrowDomainValidationException()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var act = () => TaskItem.Create(_validUserId, _validTitle, _validDescription, pastDate);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Due date cannot be in the past.");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTaskItem()
    {
        // Arrange
        var task = TaskItem.Create(_validUserId, _validTitle, _validDescription, _validDueDate);
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newStatus = TaskItemStatus.Completed;
        var newDueDate = DateTime.UtcNow.AddDays(2);

        // Act
        task.Update(newTitle, newDescription, newStatus, newDueDate);

        // Assert
        task.Title.Should().Be(newTitle);
        task.Description.Should().Be(newDescription);
        task.Status.Should().Be(newStatus);
        task.DueDate.Should().BeCloseTo(newDueDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EnsureOwnedBy_WithCorrectUserId_ShouldNotThrow()
    {
        // Arrange
        var task = TaskItem.Create(_validUserId, _validTitle, _validDescription, _validDueDate);

        // Act
        var act = () => task.EnsureOwnedBy(_validUserId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureOwnedBy_WithWrongUserId_ShouldThrowUnauthorizedTaskAccessException()
    {
        // Arrange
        var task = TaskItem.Create(_validUserId, _validTitle, _validDescription, _validDueDate);
        var wrongUserId = Guid.NewGuid();

        // Act
        var act = () => task.EnsureOwnedBy(wrongUserId);

        // Assert
        act.Should().Throw<UnauthorizedTaskAccessException>();
    }
}
