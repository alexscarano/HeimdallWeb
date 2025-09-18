using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class FindingMap : IEntityTypeConfiguration<FindingModel>
    {
        public void Configure(EntityTypeBuilder<FindingModel> builder)
        {
            builder.HasKey(f => f.finding_id);

            builder.Property(f => f.type)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.description)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(f => f.severity)
                .HasConversion<string>()
                .IsRequired()
                .HasColumnType("ENUM('Low','Medium','High','Critical')")
                .HasMaxLength(10);

            builder.Property(f => f.evidence)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(f => f.recommendation)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasOne(f => f.History)
                .WithMany(h => h.Findings)
                .HasForeignKey(f => f.history_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

