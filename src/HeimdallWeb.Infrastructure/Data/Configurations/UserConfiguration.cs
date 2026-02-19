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

        // Sprint 5: Google OAuth and password reset fields
        builder.Property(u => u.AuthProvider)
            .HasColumnName("auth_provider")
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Local");

        builder.Property(u => u.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(u => u.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(u => u.PasswordResetExpires)
            .HasColumnName("password_reset_expires")
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

        // Sprint 5: Indexes for new lookup columns
        builder.HasIndex(u => u.ExternalId)
            .HasDatabaseName("ix_tb_user_external_id");

        builder.HasIndex(u => u.PasswordResetToken)
            .HasDatabaseName("ix_tb_user_password_reset_token");
    }
}
