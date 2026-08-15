using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Tasks;
using TenantTaskManager.Application.Tasks.UpdateTask;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tests.Tasks.UpdateTask;

public sealed class UpdateTaskHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithExistingTask_UpdatesAndSavesTask()
    {
        var task = new TaskItem(Guid.NewGuid(), "Original title");
        var repository = new StubTaskRepository(task);
        var handler = new UpdateTaskHandler(repository);

        await handler.HandleAsync(new UpdateTaskCommand(task.Id, "Updated title"));

        Assert.Equal("Updated title", task.Title);
        Assert.True(repository.SaveChangesCalled);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownTask_ThrowsTaskNotFoundException()
    {
        var repository = new StubTaskRepository(null);
        var handler = new UpdateTaskHandler(repository);

        await Assert.ThrowsAsync<TaskNotFoundException>(() =>
            handler.HandleAsync(
                new UpdateTaskCommand(Guid.NewGuid(), "Updated title")));

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
