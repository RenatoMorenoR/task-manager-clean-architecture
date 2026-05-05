using TaskManager.Application.UseCases.Tasks;

namespace TaskManager.Tests.Application.Tasks;

public class UpdateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateTaskUseCase _useCase;

    public UpdateTaskUseCaseTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _useCase = new UpdateTaskUseCase(
            _taskRepositoryMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidOwnership_ShouldUpdateTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TaskItem.Reconstruct(taskId, userId, "Old Title", "", TaskItemStatus.Pending, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
        var request = new UpdateTaskRequest("New Title", "New Desc", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(1));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var result = await _useCase.ExecuteAsync(taskId, request);

        // Assert
        result.Title.Should().Be(request.Title);
        result.Status.Should().Be("InProgress");
        _taskRepositoryMock.Verify(x => x.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongOwner_ShouldThrowUnauthorizedTaskAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TaskItem.Reconstruct(taskId, otherUserId, "Title", "", TaskItemStatus.Pending, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
        var request = new UpdateTaskRequest("New Title", "New Desc", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(1));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var act = () => _useCase.ExecuteAsync(taskId, request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedTaskAccessException>();
    }
}
