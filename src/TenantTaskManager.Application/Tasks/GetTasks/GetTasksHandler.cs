using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tasks.GetTasks;

public sealed class GetTasksHandler(ITaskRepository taskRepository)
{
    public async Task<IReadOnlyList<TaskDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var tasks = await taskRepository.GetAllAsync(cancellationToken);

        return tasks
            .Select(task => new TaskDto(
                task.Id,
                task.Title,
                task.IsCompleted,
                task.CreatedAtUtc,
                task.CompletedAtUtc))
            .ToList();
    }
}