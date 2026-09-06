namespace TenantTaskManager.Domain.Entities;

public sealed class TaskItem
{
    public const int MaximumTitleLength = 200;

    private TaskItem()
    {
    }

    public TaskItem(Guid tenantId, string title)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("A tenant ID is required.", nameof(tenantId));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Title = ValidateAndNormalizeTitle(title);
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public bool IsCompleted { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public void Complete()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        CompletedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateTitle(string title)
    {
        Title = ValidateAndNormalizeTitle(title);
    }

    private static string ValidateAndNormalizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A task title is required.", nameof(title));
        }

        var normalizedTitle = title.Trim();

        if (normalizedTitle.Length > MaximumTitleLength)
        {
            throw new ArgumentException(
                $"A task title cannot exceed {MaximumTitleLength} characters.",
                nameof(title));
        }

        return normalizedTitle;
    }
}