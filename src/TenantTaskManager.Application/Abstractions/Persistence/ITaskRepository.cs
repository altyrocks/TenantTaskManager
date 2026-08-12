using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Application.Abstractions.Persistence;

public interface ITaskRepository
{
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken = default);
}