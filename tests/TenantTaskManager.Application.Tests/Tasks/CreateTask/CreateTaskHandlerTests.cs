using TenantTaskManager.Application.Abstractions.Authentication;
using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Application.Tasks.CreateTask;
using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Application.Tests.Tasks.CreateTask;

public sealed class CreateTaskHandlerTests
{
    [Fact]
    public async Task HandleAsync_UsesTenantFromCurrentContext()
    {
        var tenantId = Guid.NewGuid();
        var repository = new RecordingTaskRepository();
        var handler = new CreateTaskHandler(
            new StubCurrentTenant(tenantId),
            repository);

        var taskId = await handler.HandleAsync(
            new CreateTaskCommand("Prepare report"));

        Assert.NotNull(repository.AddedTask);
        Assert.Equal(tenantId, repository.AddedTask.TenantId);
        Assert.Equal("Prepare report", repository.AddedTask.Title);
        Assert.Equal(repository.AddedTask.Id, taskId);
    }

    [Fact]
    public async Task HandleAsync_PassesCancellationTokenToRepository()
    {
        var repository = new RecordingTaskRepository();
        var handler = new CreateTaskHandler(
            new StubCurrentTenant(Guid.NewGuid()),
            repository);
        using var cancellationSource = new CancellationTokenSource();

        await handler.HandleAsync(
            new CreateTaskCommand("Prepare report"),
            cancellationSource.Token);

        Assert.Equal(cancellationSource.Token, repository.CancellationToken);
    }

    [Fact]
    public async Task HandleAsync_WithBlankTitle_DoesNotCallRepository()
    {
        var repository = new RecordingTaskRepository();
        var handler = new CreateTaskHandler(
            new StubCurrentTenant(Guid.NewGuid()),
            repository);

        await Assert.ThrowsAsync<ArgumentException>(
            () => handler.HandleAsync(new CreateTaskCommand(" ")));

        Assert.Null(repository.AddedTask);
    }

    private sealed class StubCurrentTenant(Guid tenantId) : ICurrentTenant
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class RecordingTaskRepository : ITaskRepository
    {
        public TaskItem? AddedTask { get; private set; }

        public CancellationToken CancellationToken { get; private set; }

        public Task AddAsync(
            TaskItem task,
            CancellationToken cancellationToken = default)
        {
            AddedTask = task;
            CancellationToken = cancellationToken;

            return Task.CompletedTask;
        }
    }
}