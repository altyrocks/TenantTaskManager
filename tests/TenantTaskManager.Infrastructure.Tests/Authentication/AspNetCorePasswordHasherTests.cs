using TenantTaskManager.Infrastructure.Authentication;

namespace TenantTaskManager.Infrastructure.Tests.Authentication;

public sealed class AspNetCorePasswordHasherTests
{
    private readonly AspNetCorePasswordHasher _passwordHasher = new();

    [Fact]
    public void Hash_ReturnsValueDifferentFromPassword()
    {
        const string password = "Correct-Horse-Battery-Staple-42";

        var hash = _passwordHasher.Hash(password);

        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        const string password = "Correct-Horse-Battery-Staple-42";

        var hash = _passwordHasher.Hash(password);

        var isValid = _passwordHasher.Verify(hash, password);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = _passwordHasher.Hash("Correct-Horse-Battery-Staple-42");

        var isValid = _passwordHasher.Verify(hash, "incorrect-password");

        Assert.False(isValid);
    }

    [Fact]
    public void Hash_ForSamePassword_UsesDifferentSalt()
    {
        const string password = "Correct-Horse-Battery-Staple-42";

        var firstHash = _passwordHasher.Hash(password);
        var secondHash = _passwordHasher.Hash(password);

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(_passwordHasher.Verify(firstHash, password));
        Assert.True(_passwordHasher.Verify(secondHash, password));
    }
}