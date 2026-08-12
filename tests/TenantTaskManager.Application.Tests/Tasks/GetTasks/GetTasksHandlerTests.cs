using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tests.Tasks.GetTasks;

public sealed class GetTasksHandlerTests
{
    [Fact]
    public async Task HandleAsync_MapsTasksWithoutExposingTenantId()
    {
        var task = new TaskItem(Guid.NewGuid(), "Prepare report");
        var handler = new GetTasksHandler(new StubTaskRepository([task]));

        var result = await handler.HandleAsync();

        var item = Assert.Single(result);
        Assert.Equal(task.Id, item.Id);
        Assert.Equal(task.Title, item.Title);
        Assert.Equal(task.IsCompleted, item.IsCompleted);
        Assert.Equal(task.CreatedAtUtc, item.CreatedAtUtc);
        Assert.Equal(task.CompletedAtUtc, item.CompletedAtUtc);
        Assert.DoesNotContain(
            typeof(TaskDto).GetProperties(),
            property => property.Name == "TenantId");
    }

    private sealed class StubTaskRepository(
        IReadOnlyList<TaskItem> tasks) : ITaskRepository
    {
        public Task<IReadOnlyList<TaskItem>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tasks);

        public Task AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}