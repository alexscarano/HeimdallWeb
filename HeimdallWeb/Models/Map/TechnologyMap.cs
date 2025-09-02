using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class TechnologyMap : IEntityTypeConfiguration<TechnologyModel>
    {
        public void Configure(EntityTypeBuilder<TechnologyModel> builder)
        {
            builder.HasKey(t => t.technology_id);

            builder.Property(t => t.version).HasMaxLength(30);

            builder.Property(t => t.technology_name)
            .IsRequired()
            .HasMaxLength(35);

            builder.HasOne(t => t.History)
            .WithMany(h => h.Technologies)
            .HasForeignKey(h => h.history_id);
        }
    }
}
