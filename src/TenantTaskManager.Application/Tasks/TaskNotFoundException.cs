namespace TenantTaskManager.Application.Tasks;

public sealed class TaskNotFoundException(Guid taskId)
    : Exception($"Task '{taskId}' was not found.");