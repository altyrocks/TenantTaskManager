using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Domain.Tests.Entities;

public sealed class UserAccountTests
{
    [Fact]
    public void Constructor_WithValidValues_CreatesUser()
    {
        var tenantId = Guid.NewGuid();
        var beforeCreation = DateTimeOffset.UtcNow;

        var user = new UserAccount(
            tenantId,
            "admin@example.com",
            "password-hash",
            UserRole.Admin);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(tenantId, user.TenantId);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("ADMIN@EXAMPLE.COM", user.NormalizedEmail);
        Assert.Equal("password-hash", user.PasswordHash);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.InRange(user.CreatedAtUtc, beforeCreation, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => new UserAccount(
            Guid.Empty,
            "admin@example.com",
            "password-hash",
            UserRole.Admin));

        Assert.Equal("tenantId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithBlankEmail_ThrowsArgumentException(string email)
    {
        var exception = Assert.Throws<ArgumentException>(() => new UserAccount(
            Guid.NewGuid(),
            email,
            "password-hash",
            UserRole.User));

        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WithBlankPasswordHash_ThrowsArgumentException(
        string passwordHash)
    {
        var exception = Assert.Throws<ArgumentException>(() => new UserAccount(
            Guid.NewGuid(),
            "user@example.com",
            passwordHash,
            UserRole.User));

        Assert.Equal("passwordHash", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithUnknownRole_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new UserAccount(
            Guid.NewGuid(),
            "user@example.com",
            "password-hash",
            (UserRole)999));
    }

    [Fact]
    public void Constructor_TrimsAndNormalizesEmail()
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            "  User@Example.com  ",
            "password-hash",
            UserRole.User);

        Assert.Equal("User@Example.com", user.Email);
        Assert.Equal("USER@EXAMPLE.COM", user.NormalizedEmail);
    }
}