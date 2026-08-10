using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Application.Abstractions.Authentication;
using TenantTaskManager.Domain.Entities;

namespace TenantTaskManager.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentTenant currentTenant) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Tenant>()
            .HasQueryFilter(tenant => tenant.Id == currentTenant.TenantId);

        modelBuilder.Entity<TaskItem>()
            .HasQueryFilter(task => task.TenantId == currentTenant.TenantId);
    }
}