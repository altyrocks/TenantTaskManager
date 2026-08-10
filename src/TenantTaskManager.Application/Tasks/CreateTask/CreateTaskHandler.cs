using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Abstractions.Persistence;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Application.Tasks.CreateTask;

public sealed class CreateTaskHandler(
    ICurrentTenant currentTenant,
    ITaskRepository taskRepository)
{
    public async Task<Guid> HandleAsync(
        CreateTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = new TaskItem(currentTenant.TenantId, command.Title);

        await taskRepository.AddAsync(task, cancellationToken);

        return task.Id;
    }
}