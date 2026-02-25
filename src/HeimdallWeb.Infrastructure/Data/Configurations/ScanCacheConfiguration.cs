using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for ScanCache entity.
/// Maps to tb_scan_cache (UNLOGGED table) with snake_case columns and JSONB storage for ResultJson.
/// UNLOGGED: Faster writes, no WAL overhead. Data is truncated on crash (acceptable for cache).
/// </summary>
public class ScanCacheConfiguration : IEntityTypeConfiguration<ScanCache>
{
    public void Configure(EntityTypeBuilder<ScanCache> builder)
    {
        // Note: Table is created as UNLOGGED in migration for performance
        builder.ToTable("tb_scan_cache");

        // Primary Key
        builder.HasKey(c => c.Id)
            .HasName("pk_tb_scan_cache");

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(c => c.CacheKey)
            .HasColumnName("cache_key")
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(c => c.ResultJson)
            .HasColumnName("result_json")
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        // Ignore computed property (not stored in DB)
        builder.Ignore(c => c.IsExpired);

        // Indexes
        builder.HasIndex(c => c.CacheKey)
            .IsUnique()
            .HasDatabaseName("ux_tb_scan_cache_cache_key");

        builder.HasIndex(c => c.ExpiresAt)
            .HasDatabaseName("ix_tb_scan_cache_expires_at");

        // GIN index on result_json for JSONB query performance
        builder.HasIndex(c => c.ResultJson)
            .HasDatabaseName("ix_tb_scan_cache_result_json_gin")
            .HasAnnotation("Npgsql:IndexMethod", "gin");
    }
}
