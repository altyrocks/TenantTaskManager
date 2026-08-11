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
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantName);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(adminPassword);

        await dbContext.Database.MigrateAsync(cancellationToken);

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

        var normalizedEmail = adminEmail.Trim().ToUpperInvariant();
        var adminExists = await dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);

        if (!adminExists)
        {
            var admin = new UserAccount(
                tenant.Id,
                adminEmail,
                passwordHasher.Hash(adminPassword),
                UserRole.Admin);
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}