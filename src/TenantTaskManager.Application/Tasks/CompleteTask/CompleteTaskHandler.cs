using TenantTaskManager.Application.Tasks;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tasks.CompleteTask;

public sealed class CompleteTaskHandler(ITaskRepository taskRepository)
{
    public async Task HandleAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new TaskNotFoundException(taskId);

        task.Complete();
        await taskRepository.SaveChangesAsync(cancellationToken);
    }
}