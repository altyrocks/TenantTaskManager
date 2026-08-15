using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tasks.GetTasks;

public sealed class GetTasksHandler(ITaskQuery taskQuery)
{
    public async Task<IReadOnlyList<TaskDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        return await taskQuery.GetAllAsync(cancellationToken);
    }
}
