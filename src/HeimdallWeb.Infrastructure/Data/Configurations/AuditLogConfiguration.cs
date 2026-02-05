using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for AuditLog entity.
/// Maps Domain.Entities.AuditLog to tb_log table with snake_case columns.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("tb_log");

        // Primary Key
        builder.HasKey(l => l.LogId)
            .HasName("pk_tb_log");

        builder.Property(l => l.LogId)
            .HasColumnName("log_id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(l => l.Timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        // LogEventCode Enum stored as string
        builder.Property(l => l.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(45);

        builder.Property(l => l.Level)
            .HasColumnName("level")
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("Info");

        builder.Property(l => l.Source)
            .HasColumnName("source")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(l => l.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(l => l.Details)
            .HasColumnName("details")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired(false);

        builder.Property(l => l.HistoryId)
            .HasColumnName("history_id")
            .IsRequired(false);

        builder.Property(l => l.RemoteIp)
            .HasColumnName("remote_ip")
            .HasMaxLength(45)
            .IsRequired(false);

        // Relationships
        builder.HasOne(l => l.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(l => l.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.History)
            .WithMany(h => h.AuditLogs)
            .HasForeignKey(l => l.HistoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(l => l.Timestamp)
            .HasDatabaseName("ix_tb_log_timestamp");

        builder.HasIndex(l => l.Level)
            .HasDatabaseName("ix_tb_log_level");

        builder.HasIndex(l => l.UserId)
            .HasDatabaseName("ix_tb_log_user_id");

        builder.HasIndex(l => l.HistoryId)
            .HasDatabaseName("ix_tb_log_history_id");

        builder.HasIndex(l => l.Code)
            .HasDatabaseName("ix_tb_log_code");
    }
}
