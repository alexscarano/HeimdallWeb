using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class HistoryMap : IEntityTypeConfiguration<HistoryModel>
    {
        public void Configure(EntityTypeBuilder<HistoryModel> builder) 
        {
            builder.ToTable("History");

            builder.HasKey(h => h.history_id);

            builder.Property(h => h.target)
            .IsRequired()
            .HasMaxLength(75);

            builder.HasMany(h => h.IASummaries)
            .WithOne(ia => ia.History)
            .HasForeignKey(ia => ia.history_id)
            .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(h => h.Technologies)
            .WithOne(t => t.History)
            .HasForeignKey(t => t.history_id)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
