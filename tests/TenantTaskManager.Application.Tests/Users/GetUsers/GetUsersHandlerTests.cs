using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Users.GetUsers;
using TenantTaskManager.Application.Abstractions.Persistence;

namespace TenantTaskManager.Application.Tests.Users.GetUsers;

public sealed class GetUsersHandlerTests
{
    [Fact]
    public async Task HandleAsync_DoesNotExposeTenantOrPasswordHash()
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            "admin@example.com",
            "password-hash",
            UserRole.Admin);
        var handler = new GetUsersHandler(new StubUserRepository([user]));

        var result = await handler.HandleAsync();

        var item = Assert.Single(result);
        Assert.Equal(user.Email, item.Email);
        Assert.Equal("Admin", item.Role);
        Assert.DoesNotContain(
            typeof(UserDto).GetProperties(),
            property => property.Name is "TenantId" or "PasswordHash");
    }

    private sealed class StubUserRepository(
        IReadOnlyList<UserAccount> users) : IUserRepository
    {
        public Task<IReadOnlyList<UserAccount>> GetAllForCurrentTenantAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(users);

        public Task<UserAccount?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}