using TenantTaskManager.Application.Tasks.GetTasks;

namespace TenantTaskManager.Application.Abstractions.Persistence;

public interface ITaskQuery
{
    Task<IReadOnlyList<TaskDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
