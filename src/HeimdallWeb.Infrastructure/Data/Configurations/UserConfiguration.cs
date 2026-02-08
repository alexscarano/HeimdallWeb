using HeimdallWeb.Domain.Entities;
using HeimdallWeb.Domain.Enums;
using HeimdallWeb.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for User entity.
/// Maps Domain.Entities.User to tb_user table with snake_case columns.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("tb_user");

        // Primary Key
        builder.HasKey(u => u.UserId)
            .HasName("pk_tb_user");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.PublicId)
            .HasColumnName("public_id")
            .HasColumnType("uuid")
            .IsRequired();

        // Properties
        builder.Property(u => u.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(30);

        // EmailAddress Value Object Conversion
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(75)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.Create(value));

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password")
            .IsRequired()
            .HasMaxLength(255);

        // UserType Enum stored as integer
        builder.Property(u => u.UserType)
            .HasColumnName("user_type")
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(UserType.Default);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.Property(u => u.ProfileImage)
            .HasColumnName("profile_image")
            .HasMaxLength(255)
            .IsRequired(false);

        // Relationships
        builder.HasMany(u => u.ScanHistories)
            .WithOne(h => h.User)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserUsages)
            .WithOne(uu => uu.User)
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AuditLogs)
            .WithOne(l => l.User)
            .HasForeignKey("UserId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ux_tb_user_username");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ux_tb_user_email");

        builder.HasIndex(u => u.CreatedAt)
            .HasDatabaseName("ix_tb_user_created_at");

        builder.HasIndex(u => u.PublicId)
            .IsUnique()
            .HasDatabaseName("ux_tb_user_public_id");
    }
}
