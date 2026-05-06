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

public class DeleteTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteTaskUseCase _useCase;

    public DeleteTaskUseCaseTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _useCase = new DeleteTaskUseCase(
            _taskRepositoryMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WithValidOwnership_ShouldDeleteTask()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TaskItem.Reconstruct(taskId, userId, "Title", "", TaskItemStatus.Pending, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        await _useCase.ExecuteAsync(taskId);

        // Assert
        _taskRepositoryMock.Verify(x => x.DeleteAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongOwner_ShouldThrowUnauthorizedTaskAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = TaskItem.Reconstruct(taskId, otherUserId, "Title", "", TaskItemStatus.Pending, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        // Act
        var act = () => _useCase.ExecuteAsync(taskId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedTaskAccessException>();
    }
}
