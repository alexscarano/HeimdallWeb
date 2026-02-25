using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for ScanProfile entity.
/// Maps to tb_scan_profile with snake_case columns.
/// Name has a unique constraint so each profile name is guaranteed distinct.
/// </summary>
public class ScanProfileConfiguration : IEntityTypeConfiguration<ScanProfile>
{
    public void Configure(EntityTypeBuilder<ScanProfile> builder)
    {
        builder.ToTable("tb_scan_profile");

        // Primary Key
        builder.HasKey(sp => sp.Id)
            .HasName("pk_tb_scan_profile");

        builder.Property(sp => sp.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Name — unique human-readable identifier for the profile
        builder.Property(sp => sp.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(50);

        // Description — optional explanation of when to use this profile
        builder.Property(sp => sp.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .HasDefaultValue(string.Empty);

        // ConfigJson — JSON payload with scanner list and timeout settings
        builder.Property(sp => sp.ConfigJson)
            .HasColumnName("config_json")
            .HasColumnType("text")
            .IsRequired();

        // IsSystem — when true the profile was seeded and must not be deleted
        builder.Property(sp => sp.IsSystem)
            .HasColumnName("is_system")
            .IsRequired()
            .HasDefaultValue(true);

        // Unique index on name — one profile record per profile name
        builder.HasIndex(sp => sp.Name)
            .IsUnique()
            .HasDatabaseName("ux_tb_scan_profile_name");
    }
}
