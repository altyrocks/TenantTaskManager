using Microsoft.EntityFrameworkCore;
using TenantTaskManager.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TenantTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(task => task.Id);

        builder.Property(task => task.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(task => task.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(task => new
        {
            task.TenantId,
            task.IsCompleted,
            task.CreatedAtUtc
        });
    }
}