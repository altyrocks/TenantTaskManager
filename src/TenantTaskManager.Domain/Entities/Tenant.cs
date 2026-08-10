namespace TenantTaskManager.Domain.Entities;

public sealed class Tenant
{
    private Tenant()
    {
    }

    public Tenant(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A tenant name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }
}