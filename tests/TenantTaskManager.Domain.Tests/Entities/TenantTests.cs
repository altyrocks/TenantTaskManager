using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Domain.Tests.Entities;

public sealed class TenantTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesTenant()
    {
        var beforeCreation = DateTimeOffset.UtcNow;

        var tenant = new Tenant("Acme");

        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal("Acme", tenant.Name);
        Assert.InRange(tenant.CreatedAtUtc, beforeCreation, DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithBlankName_ThrowsArgumentException(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Tenant(name));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_TrimsName()
    {
        var tenant = new Tenant("  Acme  ");

        Assert.Equal("Acme", tenant.Name);
    }
}