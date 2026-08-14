using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Infrastructure.Persistence;
using TenantTaskManager.Application.Abstractions.Authentication;
using TenantTaskManager.Infrastructure.Persistence.Repositories;

namespace TenantTaskManager.Infrastructure.Tests.Persistence.Repositories;

public sealed class UserRepositoryTests
{
    [Fact]
    public async Task GetAllForCurrentTenantAsync_ReturnsOnlyCurrentTenantUsers()
    {
        var currentTenantId = Guid.NewGuid();
        var currentUser = new UserAccount(
            currentTenantId,
            "current@example.com",
            "password-hash",
            UserRole.Admin);
        var otherUser = new UserAccount(
            Guid.NewGuid(),
            "other@example.com",
            "password-hash",
            UserRole.User);
        await using var dbContext = CreateDbContext(currentTenantId);
        dbContext.Users.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        var repository = new UserRepository(dbContext);

        var result = await repository.GetAllForCurrentTenantAsync();

        var user = Assert.Single(result);
        Assert.Equal(currentUser.Id, user.Id);
        Assert.Equal(currentTenantId, user.TenantId);
    }

    [Fact]
    public async Task GetByNormalizedEmailAsync_FindsUserOutsideCurrentTenant()
    {
        var currentTenantId = Guid.NewGuid();
        var user = new UserAccount(
            Guid.NewGuid(),
            "user@example.com",
            "password-hash",
            UserRole.User);
        await using var dbContext = CreateDbContext(currentTenantId);
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var repository = new UserRepository(dbContext);

        var visibleThroughTenantFilter = await dbContext.Users.ToListAsync();
        var result = await repository.GetByNormalizedEmailAsync(
            "USER@EXAMPLE.COM");

        Assert.Empty(visibleThroughTenantFilter);
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.TenantId, result.TenantId);
    }

    [Fact]
    public async Task GetByNormalizedEmailAsync_WithUnknownEmail_ReturnsNull()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid());
        var repository = new UserRepository(dbContext);

        var result = await repository.GetByNormalizedEmailAsync(
            "UNKNOWN@EXAMPLE.COM");

        Assert.Null(result);
    }

    private static AppDbContext CreateDbContext(Guid tenantId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options, new StubCurrentTenant(tenantId));
    }

    private sealed class StubCurrentTenant(Guid tenantId) : ICurrentTenant
    {
        public Guid TenantId { get; } = tenantId;
    }
}