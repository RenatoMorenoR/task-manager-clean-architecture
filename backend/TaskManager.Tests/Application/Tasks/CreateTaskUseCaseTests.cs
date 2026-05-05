using TaskManager.Application.UseCases.Tasks;

namespace TaskManager.Tests.Application.Tasks;

public class CreateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateTaskUseCase _useCase;

    public CreateTaskUseCaseTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _useCase = new CreateTaskUseCase(
            _taskRepositoryMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ShouldReturnTaskDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTaskRequest("Test Task", "Description", DateTime.UtcNow.AddDays(1));

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem t, CancellationToken _) => t);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(request.Title);
        result.UserId.Should().Be(userId);
        
        _taskRepositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(t => t.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthenticated_ShouldThrowAuthenticationException()
    {
        // Arrange
        var request = new CreateTaskRequest("Test Task", "Description", DateTime.UtcNow.AddDays(1));
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
