using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for MonitoredTarget entity.
/// Maps to tb_monitored_target with snake_case columns.
/// </summary>
public class MonitoredTargetConfiguration : IEntityTypeConfiguration<MonitoredTarget>
{
    public void Configure(EntityTypeBuilder<MonitoredTarget> builder)
    {
        builder.ToTable("tb_monitored_target");

        // Primary Key
        builder.HasKey(t => t.Id)
            .HasName("pk_tb_monitored_target");

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.Url)
            .HasColumnName("url")
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(t => t.Frequency)
            .HasColumnName("frequency")
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.LastCheck)
            .HasColumnName("last_check")
            .IsRequired(false);

        builder.Property(t => t.NextCheck)
            .HasColumnName("next_check")
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.RiskSnapshots)
            .WithOne(s => s.MonitoredTarget)
            .HasForeignKey(s => s.MonitoredTargetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_tb_monitored_target_user_id");

        builder.HasIndex(t => t.NextCheck)
            .HasDatabaseName("ix_tb_monitored_target_next_check");

        builder.HasIndex(t => new { t.UserId, t.Url })
            .IsUnique()
            .HasDatabaseName("ux_tb_monitored_target_user_url");
    }
}
