namespace TenantTaskManager.Application.Tasks.CompleteTask;

public sealed class TaskNotFoundException(Guid taskId)
    : Exception($"Task '{taskId}' was not found.");