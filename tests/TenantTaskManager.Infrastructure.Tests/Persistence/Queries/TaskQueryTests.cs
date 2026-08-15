using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Application.Abstractions.Authentication;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Infrastructure.Persistence;
using TenantTaskManager.Infrastructure.Persistence.Queries;

namespace TenantTaskManager.Infrastructure.Tests.Persistence.Queries;

public sealed class TaskQueryTests
{
    [Fact]
    public async Task GetAllAsync_ProjectsAndOrdersOnlyCurrentTenantTasks()
    {
        var tenantId = Guid.NewGuid();
        var olderOpenTask = new TaskItem(tenantId, "Older open task");
        var newerOpenTask = new TaskItem(tenantId, "Newer open task");
        var completedTask = new TaskItem(tenantId, "Completed task");
        completedTask.Complete();
        var otherTenantTask = new TaskItem(Guid.NewGuid(), "Hidden task");

        await using var dbContext = CreateDbContext(tenantId);
        dbContext.Tasks.AddRange(
            olderOpenTask,
            newerOpenTask,
            completedTask,
            otherTenantTask);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var query = new TaskQuery(dbContext);

        var result = await query.GetAllAsync();

        Assert.Equal(3, result.Count);
        Assert.Equal(newerOpenTask.Id, result[0].Id);
        Assert.Equal(olderOpenTask.Id, result[1].Id);
        Assert.Equal(completedTask.Id, result[2].Id);
        Assert.DoesNotContain(result, task => task.Id == otherTenantTask.Id);
        Assert.Empty(dbContext.ChangeTracker.Entries());
    }

    private static AppDbContext CreateDbContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new StubCurrentTenant(tenantId));
    }

    private sealed class StubCurrentTenant(Guid tenantId) : ICurrentTenant
    {
        public Guid TenantId { get; } = tenantId;
    }
}
