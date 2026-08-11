using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TenantTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(user => user.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder.HasIndex(user => user.TenantId);
    }
}