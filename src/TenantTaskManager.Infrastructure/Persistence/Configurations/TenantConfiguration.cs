using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TenantTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(tenant => tenant.Id);

        builder.Property(tenant => tenant.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(tenant => tenant.Name)
            .IsUnique();
    }
}