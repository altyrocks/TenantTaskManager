namespace TenantTaskManager.Desktop.Models;

public sealed record TaskItemDto(
    Guid Id,
    string Title,
    bool IsCompleted,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? CompletedAtUtc)
{
    public string Status => IsCompleted ? "Complete" : "Open";

    public string CreatedDisplay =>
        $"Created {CreatedAtUtc.LocalDateTime:g}";
}
