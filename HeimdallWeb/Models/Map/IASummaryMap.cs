using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class IASummaryMap : IEntityTypeConfiguration<IASummaryModel>
    {
        public void Configure(EntityTypeBuilder<IASummaryModel> builder)
        {
            builder.Property(t => t.summary_text).HasMaxLength(1000);

            builder.Property(t => t.main_category).HasMaxLength(100);

            builder.Property(t => t.overall_risk)
                .HasConversion<string>()
                .HasColumnType("ENUM('Baixo','Medio','Alto','Critico')")
                .HasMaxLength(10)
                .HasDefaultValue("Baixo");

            builder.HasKey(t => t.ia_summary_id);

            builder.HasOne(t => t.History)
            .WithMany(h => h.IASummaries)
            .HasForeignKey(h => h.history_id);
        }
    }
}
