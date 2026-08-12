namespace TenantTaskManager.Application.Tasks.GetTasks;

public sealed record TaskDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc);