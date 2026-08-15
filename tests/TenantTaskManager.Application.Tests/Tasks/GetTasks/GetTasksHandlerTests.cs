using TenantTaskManager.Application.Tasks.GetTasks;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tests.Tasks.GetTasks;

public sealed class GetTasksHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsQueryResultsWithoutExposingTenantId()
    {
        var task = new TaskDto(
            Guid.NewGuid(),
            "Prepare report",
            false,
            DateTimeOffset.UtcNow,
            null);
        var handler = new GetTasksHandler(new StubTaskQuery([task]));

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

    private sealed class StubTaskQuery(
        IReadOnlyList<TaskDto> tasks) : ITaskQuery
    {
        public Task<IReadOnlyList<TaskDto>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tasks);
    }
}
