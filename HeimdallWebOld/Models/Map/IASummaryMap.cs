using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map;

public class IASummaryMap : IEntityTypeConfiguration<IASummaryModel>
{
    public void Configure(EntityTypeBuilder<IASummaryModel> builder)
    {
        builder.ToTable("tb_ia_summary");

        builder.HasKey(t => t.ia_summary_id).HasName("pk_tb_ia_summary");

        builder.Property(t => t.ia_summary_id)
            .HasColumnName("ia_summary_id")
            .IsRequired();

        builder.Property(t => t.summary_text)
            .HasColumnName("summary_text")
            .HasMaxLength(1000);

        builder.Property(t => t.main_category)
            .HasColumnName("main_category")
            .HasMaxLength(100);

        builder.Property(t => t.overall_risk)
            .HasColumnName("overall_risk")
            .HasConversion<string>()
            .HasColumnType("ENUM('Baixo','Medio','Alto','Critico')")
            .HasMaxLength(10)
            .HasDefaultValue("Baixo");

        builder.Property(t => t.total_findings)
            .HasColumnName("total_findings")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.findings_critical)
            .HasColumnName("findings_critical")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.findings_high)
            .HasColumnName("findings_high")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.findings_medium)
            .HasColumnName("findings_medium")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.findings_low)
            .HasColumnName("findings_low")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.ia_notes)
            .HasColumnName("ia_notes")
            .HasColumnType("text");

        builder.Property(t => t.created_date)
            .HasColumnName("created_date")
            .IsRequired();

        builder.Property(t => t.history_id)
            .HasColumnName("history_id");

        builder.HasOne(t => t.History)
            .WithMany(h => h.IASummaries)
            .HasForeignKey(h => h.history_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
