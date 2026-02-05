using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for Finding entity.
/// Maps Domain.Entities.Finding to tb_finding table with snake_case columns.
/// </summary>
public class FindingConfiguration : IEntityTypeConfiguration<Finding>
{
    public void Configure(EntityTypeBuilder<Finding> builder)
    {
        builder.ToTable("tb_finding");

        // Primary Key
        builder.HasKey(f => f.FindingId)
            .HasName("pk_tb_finding");

        builder.Property(f => f.FindingId)
            .HasColumnName("finding_id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(f => f.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Description)
            .HasColumnName("description")
            .HasColumnType("text")
            .IsRequired();

        // SeverityLevel Enum stored as integer (MySQL: TINYINT, PostgreSQL: smallint)
        builder.Property(f => f.Severity)
            .HasColumnName("severity")
            .IsRequired()
            .HasConversion<int>()
            .HasColumnType("smallint");

        builder.Property(f => f.Evidence)
            .HasColumnName("evidence")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(f => f.Recommendation)
            .HasColumnName("recommendation")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.HistoryId)
            .HasColumnName("history_id")
            .IsRequired(false);

        // Relationships
        builder.HasOne(f => f.History)
            .WithMany(h => h.Findings)
            .HasForeignKey(f => f.HistoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(f => f.HistoryId)
            .HasDatabaseName("ix_tb_finding_history_id");

        builder.HasIndex(f => f.Severity)
            .HasDatabaseName("ix_tb_finding_severity");

        builder.HasIndex(f => f.CreatedAt)
            .HasDatabaseName("ix_tb_finding_created_at");
    }
}
