using Microsoft.AspNetCore.Identity;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Infrastructure.Authentication;

public sealed class AspNetCorePasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private readonly object _user = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(_user, password);
    }

    public bool Verify(string passwordHash, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var result = _passwordHasher.VerifyHashedPassword(
            _user,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}