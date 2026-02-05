using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for ScanHistory entity.
/// Maps Domain.Entities.ScanHistory to tb_history table with snake_case columns.
/// CRITICAL: raw_json_result uses PostgreSQL JSONB with GIN index.
/// </summary>
public class ScanHistoryConfiguration : IEntityTypeConfiguration<ScanHistory>
{
    public void Configure(EntityTypeBuilder<ScanHistory> builder)
    {
        builder.ToTable("tb_history");

        // Primary Key
        builder.HasKey(h => h.HistoryId)
            .HasName("pk_tb_history");

        builder.Property(h => h.HistoryId)
            .HasColumnName("history_id")
            .ValueGeneratedOnAdd();

        // ScanTarget Value Object Conversion
        builder.Property(h => h.Target)
            .HasColumnName("target")
            .IsRequired()
            .HasMaxLength(75)
            .HasConversion(
                target => target.Value,
                value => ScanTarget.Create(value));

        // JSONB for PostgreSQL (JSON for MySQL compatibility)
        builder.Property(h => h.RawJsonResult)
            .HasColumnName("raw_json_result")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(h => h.Summary)
            .HasColumnName("summary")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(h => h.HasCompleted)
            .HasColumnName("has_completed")
            .IsRequired()
            .HasDefaultValue(false);

        // ScanDuration Value Object Conversion (stored as interval/time)
        builder.Property(h => h.Duration)
            .HasColumnName("duration")
            .HasColumnType("interval") // PostgreSQL interval type (MySQL: time)
            .IsRequired(false)
            .HasConversion(
                duration => duration != null ? duration.Value : (TimeSpan?)null,
                value => value.HasValue ? ScanDuration.Create(value.Value) : null);

        builder.Property(h => h.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired();

        builder.Property(h => h.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Relationships
        builder.HasOne(h => h.User)
            .WithMany(u => u.ScanHistories)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Findings)
            .WithOne(f => f.History)
            .HasForeignKey("HistoryId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.Technologies)
            .WithOne(t => t.History)
            .HasForeignKey("HistoryId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.IASummaries)
            .WithOne(ia => ia.History)
            .HasForeignKey("HistoryId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.AuditLogs)
            .WithOne(l => l.History)
            .HasForeignKey("HistoryId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(h => h.UserId)
            .HasDatabaseName("ix_tb_history_user_id");

        builder.HasIndex(h => h.Target)
            .HasDatabaseName("ix_tb_history_target");

        builder.HasIndex(h => h.CreatedDate)
            .HasDatabaseName("ix_tb_history_created_date");

        builder.HasIndex(h => h.HasCompleted)
            .HasDatabaseName("ix_tb_history_has_completed");

        // GIN Index for JSONB queries (PostgreSQL-specific)
        builder.HasIndex(h => h.RawJsonResult)
            .HasDatabaseName("ix_tb_history_raw_json_gin")
            .HasMethod("gin"); // PostgreSQL GIN index for fast JSONB queries
    }
}
