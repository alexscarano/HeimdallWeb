using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for RiskWeight entity.
/// Maps to tb_risk_weights with snake_case columns.
/// Category has a unique constraint so weights can be looked up by scanner category name.
/// </summary>
public class RiskWeightConfiguration : IEntityTypeConfiguration<RiskWeight>
{
    public void Configure(EntityTypeBuilder<RiskWeight> builder)
    {
        builder.ToTable("tb_risk_weights");

        // Primary Key
        builder.HasKey(rw => rw.Id)
            .HasName("pk_tb_risk_weights");

        builder.Property(rw => rw.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Category — unique identifier for the scanner category (e.g. "SSL", "Headers")
        builder.Property(rw => rw.Category)
            .HasColumnName("category")
            .IsRequired()
            .HasMaxLength(50);

        // Weight — multiplier applied to base severity points
        builder.Property(rw => rw.Weight)
            .HasColumnName("weight")
            .HasColumnType("decimal(5,2)")
            .IsRequired()
            .HasDefaultValue(1.0m);

        // IsActive — when false, the category is excluded from scoring
        builder.Property(rw => rw.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        // Unique index on category — one weight record per scanner category
        builder.HasIndex(rw => rw.Category)
            .IsUnique()
            .HasDatabaseName("ux_tb_risk_weights_category");
    }
}
