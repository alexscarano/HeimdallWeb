using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map
{
    public class UserMap : IEntityTypeConfiguration<UserModel>
    {
        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.user_id);

            builder.Property(u => u.username)
            .IsRequired()
            .HasMaxLength(30);

            builder.Property(u => u.email)
            .IsRequired()
            .HasMaxLength(75);

            builder.Property(u => u.password)
            .IsRequired();

            builder.Property(u => u.created_at)
            .IsRequired();

            builder.HasMany(u => u.Histories)
           .WithOne(h => h.User)
           .HasForeignKey(h => h.user_id)
           .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
