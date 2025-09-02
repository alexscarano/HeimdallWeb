using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class IASummaryMap : IEntityTypeConfiguration<IASummaryModel>
    {
        public void Configure(EntityTypeBuilder<IASummaryModel> builder)
        {
            builder.Property(t => t.category).HasMaxLength(100);

            builder.HasKey(t => t.ia_summary_id);

            builder.HasOne(t => t.History)
            .WithMany(h => h.IASummaries)
            .HasForeignKey(h => h.history_id);
        }
    }
}
