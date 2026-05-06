using FluentAssertions;
using Moq;
using TaskManager.Application.UseCases.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;
using TaskManager.Application.Interfaces;
using Xunit;

namespace TaskManager.Tests.Application.Tasks;

public class GetTasksUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetTasksUseCase _useCase;

    public GetTasksUseCaseTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _useCase = new GetTasksUseCase(
            _taskRepositoryMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tasks = new List<TaskItem>
        {
            TaskItem.Reconstruct(Guid.NewGuid(), userId, "Title 1", "", TaskItemStatus.Pending, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow),
            TaskItem.Reconstruct(Guid.NewGuid(), userId, "Title 2", "", TaskItemStatus.Completed, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow)
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(tasks);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotAuthenticated_ShouldThrowAuthenticationException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        var act = () => _useCase.ExecuteAsync();

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }
}
