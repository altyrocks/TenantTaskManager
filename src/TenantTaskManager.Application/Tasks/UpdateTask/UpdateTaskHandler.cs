using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tasks.UpdateTask;

public sealed class UpdateTaskHandler(ITaskRepository taskRepository)
{
    public async Task HandleAsync(
        UpdateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = await taskRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new TaskNotFoundException(command.Id);

        task.UpdateTitle(command.Title);
        await taskRepository.SaveChangesAsync(cancellationToken);
    }
}