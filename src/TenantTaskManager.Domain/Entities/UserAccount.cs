using TenantTaskManager.Domain.Users;

namespace TenantTaskManager.Domain.Entities;

public sealed class UserAccount
{
    private UserAccount()
    {
    }

    public UserAccount(
        Guid tenantId,
        string email,
        string passwordHash,
        UserRole role)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("A tenant ID is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("An email address is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("A password hash is required.", nameof(passwordHash));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        Id = Guid.NewGuid();
        TenantId = tenantId;
        Email = email.Trim();
        NormalizedEmail = Email.ToUpperInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid TenantId { get; private set; }

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
}