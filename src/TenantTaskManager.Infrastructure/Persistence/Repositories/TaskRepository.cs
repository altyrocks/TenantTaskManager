using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public Task<TaskItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Tasks.SingleOrDefaultAsync(
            task => task.Id == id,
            cancellationToken);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Add(task);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
