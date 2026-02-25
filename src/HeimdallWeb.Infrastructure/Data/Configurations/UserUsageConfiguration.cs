using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for UserUsage entity.
/// Maps Domain.Entities.UserUsage to tb_user_usage table with snake_case columns.
/// </summary>
public class UserUsageConfiguration : IEntityTypeConfiguration<UserUsage>
{
    public void Configure(EntityTypeBuilder<UserUsage> builder)
    {
        builder.ToTable("tb_user_usage");

        // Primary Key
        builder.HasKey(uu => uu.UserUsageId)
            .HasName("pk_tb_user_usage");

        builder.Property(uu => uu.UserUsageId)
            .HasColumnName("user_usage_id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(uu => uu.Date)
            .HasColumnName("date")
            .HasColumnType("date") // PostgreSQL date type (no time component)
            .IsRequired();

        builder.Property(uu => uu.RequestCounts)
            .HasColumnName("request_counts")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(uu => uu.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Relationships
        builder.HasOne(uu => uu.User)
            .WithMany(u => u.UserUsages)
            .HasForeignKey(uu => uu.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(uu => uu.UserId)
            .HasDatabaseName("ix_tb_user_usage_user_id");

        builder.HasIndex(uu => uu.Date)
            .HasDatabaseName("ix_tb_user_usage_date");

        // Composite Index for queries by user and date
        builder.HasIndex(uu => new { uu.UserId, uu.Date })
            .HasDatabaseName("ix_tb_user_usage_user_id_date")
            .IsUnique(); // One record per user per day
    }
}
