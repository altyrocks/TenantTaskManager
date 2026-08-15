using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Application.Abstractions.Persistence;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
