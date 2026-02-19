using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for RiskSnapshot entity.
/// Maps to tb_risk_snapshot with snake_case columns.
/// </summary>
public class RiskSnapshotConfiguration : IEntityTypeConfiguration<RiskSnapshot>
{
    public void Configure(EntityTypeBuilder<RiskSnapshot> builder)
    {
        builder.ToTable("tb_risk_snapshot");

        // Primary Key
        builder.HasKey(s => s.Id)
            .HasName("pk_tb_risk_snapshot");

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(s => s.MonitoredTargetId)
            .HasColumnName("monitored_target_id")
            .IsRequired();

        builder.Property(s => s.ScanHistoryId)
            .HasColumnName("scan_history_id")
            .IsRequired();

        builder.Property(s => s.Score)
            .HasColumnName("score")
            .IsRequired();

        builder.Property(s => s.Grade)
            .HasColumnName("grade")
            .IsRequired()
            .HasMaxLength(1);

        builder.Property(s => s.FindingsCount)
            .HasColumnName("findings_count")
            .IsRequired();

        builder.Property(s => s.CriticalCount)
            .HasColumnName("critical_count")
            .IsRequired();

        builder.Property(s => s.HighCount)
            .HasColumnName("high_count")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasOne(s => s.MonitoredTarget)
            .WithMany(t => t.RiskSnapshots)
            .HasForeignKey(s => s.MonitoredTargetId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ScanHistory)
            .WithMany()
            .HasForeignKey(s => s.ScanHistoryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(s => s.MonitoredTargetId)
            .HasDatabaseName("ix_tb_risk_snapshot_monitored_target_id");

        builder.HasIndex(s => s.CreatedAt)
            .IsDescending(true)
            .HasDatabaseName("ix_tb_risk_snapshot_created_at");
    }
}
