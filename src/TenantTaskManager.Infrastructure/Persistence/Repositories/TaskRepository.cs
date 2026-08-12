using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(AppDbContext dbContext) : ITaskRepository
{
    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .OrderBy(task => task.IsCompleted)
            .ThenByDescending(task => task.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        dbContext.Tasks.Add(task);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}