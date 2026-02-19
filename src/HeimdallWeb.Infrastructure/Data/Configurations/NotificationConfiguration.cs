using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for Notification entity.
/// Maps Domain.Entities.Notification to tb_notification table with snake_case columns.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("tb_notification");

        // Primary Key
        builder.HasKey(n => n.Id)
            .HasName("pk_tb_notification");

        builder.Property(n => n.Id)
            .HasColumnName("notification_id")
            .ValueGeneratedOnAdd();

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasColumnName("body")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasColumnName("is_read")
            .HasDefaultValue(false);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        // Indexes
        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("ix_tb_notification_user_id_is_read");

        builder.HasIndex(n => n.CreatedAt)
            .HasDatabaseName("ix_tb_notification_created_at");
    }
}
