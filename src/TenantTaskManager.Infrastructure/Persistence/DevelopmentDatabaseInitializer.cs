using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Users;
using TenantTaskManager.Domain.Entities;
using TenantTaskManager.Application.Abstractions.Authentication;

namespace TenantTaskManager.Infrastructure.Persistence;

public sealed class DevelopmentDatabaseInitializer(
    AppDbContext dbContext,
    IPasswordHasher passwordHasher)
{
    public async Task InitializeAsync(
        string tenantName,
        string adminEmail,
        string adminPassword,
        string userEmail,
        string userPassword,
        string secondTenantName,
        string secondUserEmail,
        string secondUserPassword,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantName);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(userPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(secondTenantName);
        ArgumentException.ThrowIfNullOrWhiteSpace(secondUserEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(secondUserPassword);

        await dbContext.Database.MigrateAsync(cancellationToken);

        var tenant = await EnsureTenantAsync(tenantName, cancellationToken);
        await EnsureUserAsync(
            tenant.Id,
            adminEmail,
            adminPassword,
            UserRole.Admin,
            cancellationToken);
        await EnsureUserAsync(
            tenant.Id,
            userEmail,
            userPassword,
            UserRole.User,
            cancellationToken);

        var secondTenant = await EnsureTenantAsync(
            secondTenantName,
            cancellationToken);
        await EnsureUserAsync(
            secondTenant.Id,
            secondUserEmail,
            secondUserPassword,
            UserRole.User,
            cancellationToken);
    }

    private async Task<Tenant> EnsureTenantAsync(
        string tenantName,
        CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                item => item.Name == tenantName,
                cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant(tenantName);
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return tenant;
    }

    private async Task EnsureUserAsync(
        Guid tenantId,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var userExists = await dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (!userExists)
        {
            var user = new UserAccount(
                tenantId,
                email,
                passwordHasher.Hash(password),
                role);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}