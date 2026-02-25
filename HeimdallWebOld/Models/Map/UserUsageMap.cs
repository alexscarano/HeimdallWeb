using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map;

public class UserUsageMap : IEntityTypeConfiguration<UserUsageModel>
{
    public void Configure(EntityTypeBuilder<UserUsageModel> builder)
    {
        builder.ToTable("tb_user_usage");

        builder.HasKey(x => x.user_usage_id).HasName("pk_tb_user_usage");

        builder.Property(x => x.user_usage_id)
            .HasColumnName("user_usage_id");

        builder.Property(x => x.date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(x => x.request_counts)
            .HasColumnName("request_counts")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.user_id)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserUsages)
            .HasForeignKey(x => x.user_id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
