using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for IASummary entity.
/// Maps Domain.Entities.IASummary to tb_ia_summary table with snake_case columns.
/// </summary>
public class IASummaryConfiguration : IEntityTypeConfiguration<IASummary>
{
    public void Configure(EntityTypeBuilder<IASummary> builder)
    {
        builder.ToTable("tb_ia_summary");

        // Primary Key
        builder.HasKey(ia => ia.IASummaryId)
            .HasName("pk_tb_ia_summary");

        builder.Property(ia => ia.IASummaryId)
            .HasColumnName("ia_summary_id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(ia => ia.SummaryText)
            .HasColumnName("summary_text")
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(ia => ia.MainCategory)
            .HasColumnName("main_category")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(ia => ia.OverallRisk)
            .HasColumnName("overall_risk")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(ia => ia.TotalFindings)
            .HasColumnName("total_findings")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ia => ia.FindingsCritical)
            .HasColumnName("findings_critical")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ia => ia.FindingsHigh)
            .HasColumnName("findings_high")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ia => ia.FindingsMedium)
            .HasColumnName("findings_medium")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ia => ia.FindingsLow)
            .HasColumnName("findings_low")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ia => ia.IANotes)
            .HasColumnName("ia_notes")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(ia => ia.CreatedDate)
            .HasColumnName("created_date")
            .IsRequired();

        builder.Property(ia => ia.HistoryId)
            .HasColumnName("history_id")
            .IsRequired(false);

        // Relationships
        builder.HasOne(ia => ia.History)
            .WithMany(h => h.IASummaries)
            .HasForeignKey(ia => ia.HistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ia => ia.HistoryId)
            .HasDatabaseName("ix_tb_ia_summary_history_id");

        builder.HasIndex(ia => ia.MainCategory)
            .HasDatabaseName("ix_tb_ia_summary_main_category");

        builder.HasIndex(ia => ia.OverallRisk)
            .HasDatabaseName("ix_tb_ia_summary_overall_risk");

        builder.HasIndex(ia => ia.CreatedDate)
            .HasDatabaseName("ix_tb_ia_summary_created_date");
    }
}
