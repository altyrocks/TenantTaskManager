using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Application.Tasks.GetTasks;

namespace TenantTaskManager.Infrastructure.Persistence.Queries;

public sealed class TaskQuery(AppDbContext dbContext) : ITaskQuery
{
    public async Task<IReadOnlyList<TaskDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Tasks
            .AsNoTracking()
            .OrderBy(task => task.IsCompleted)
            .ThenByDescending(task => task.CreatedAtUtc)
            .Select(task => new TaskDto(
                task.Id,
                task.Title,
                task.IsCompleted,
                task.CreatedAtUtc,
                task.CompletedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
