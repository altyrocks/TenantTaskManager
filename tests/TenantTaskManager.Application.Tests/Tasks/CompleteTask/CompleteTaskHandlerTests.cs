using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Tasks;
using TenantTaskManager.Application.Tasks.CompleteTask;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tests.Tasks.CompleteTask;

public sealed class CompleteTaskHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingTask_CompletesAndSavesTask()
    {
        var task = new TaskItem(Guid.NewGuid(), "Prepare report");
        var repository = new StubTaskRepository(task);
        var handler = new CompleteTaskHandler(repository);

        await handler.HandleAsync(task.Id);

        Assert.True(task.IsCompleted);
        Assert.NotNull(task.CompletedAtUtc);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownTask_ThrowsTaskNotFoundException()
    {
        var repository = new StubTaskRepository(null);
        var handler = new CompleteTaskHandler(repository);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            handler.HandleAsync(Guid.NewGuid()));

        Assert.False(repository.SaveChangesCalled);
    }

    private sealed class StubTaskRepository(TaskItem? task) : ITaskRepository
    {
        public bool SaveChangesCalled { get; private set; }

        public Task<TaskItem?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(task);

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }

        public Task AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

    }
}
