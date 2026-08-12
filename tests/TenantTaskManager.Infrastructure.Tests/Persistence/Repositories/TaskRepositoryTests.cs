using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Infrastructure.Persistence;
using TenantTaskManager.Application.Abstractions.Authentication;
using TenantTaskManager.Infrastructure.Persistence.Repositories;

namespace TenantTaskManager.Infrastructure.Tests.Persistence.Repositories;

public sealed class TaskRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCurrentTenantTasks()
    {
        var currentTenantId = Guid.NewGuid();
        var currentTenantTask = new TaskItem(currentTenantId, "Visible task");
        var otherTenantTask = new TaskItem(Guid.NewGuid(), "Hidden task");

        await using var dbContext = CreateDbContext(currentTenantId);

        dbContext.Tasks.AddRange(currentTenantTask, otherTenantTask);

        await dbContext.SaveChangesAsync();

        var repository = new TaskRepository(dbContext);

        var result = await repository.GetAllAsync();

        var task = Assert.Single(result);

        Assert.Equal(currentTenantTask.Id, task.Id);
        Assert.Equal(currentTenantId, task.TenantId);
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