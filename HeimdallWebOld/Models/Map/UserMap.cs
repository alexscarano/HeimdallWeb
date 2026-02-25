using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class UserMap : IEntityTypeConfiguration<UserModel>
    {
        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.ToTable("tb_user");

            builder.HasKey(x => x.user_id).HasName("pk_tb_user");

            builder.Property(x => x.user_id)
                .HasColumnName("user_id");

            builder.Property(u => u.profile_image)
                .HasColumnName("profile_image")
                .HasMaxLength(255);

            builder.Property(u => u.username)
                .IsRequired()
                .HasColumnName("username")
                .HasMaxLength(30);

            builder.Property(u => u.email)
                .IsRequired()
                .HasColumnName("email")
                .HasMaxLength(75);

            builder.Property(u => u.password)
                .IsRequired()
                .HasColumnName("password")
                .HasMaxLength(255);

            builder.Property(u => u.user_type)
                .HasColumnName("user_type")
                .IsRequired()
                .HasDefaultValue((int)HeimdallWeb.Enums.UserType.Default);

            builder.Property(u => u.is_active)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(u => u.created_at)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(u => u.updated_at)
                .HasColumnName("updated_at");

            // relationships
            builder.HasMany(u => u.Histories)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.UserUsages)
                .WithOne(uu => uu.User)
                .HasForeignKey(uu => uu.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.LogModel)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.user_id)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(u => u.username).IsUnique().HasDatabaseName("ux_tb_user_username");
            builder.HasIndex(u => u.email).IsUnique().HasDatabaseName("ux_tb_user_email");
        }
    }
}
