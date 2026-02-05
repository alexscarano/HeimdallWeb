using HeimdallWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for Technology entity.
/// Maps Domain.Entities.Technology to tb_technology table with snake_case columns.
/// </summary>
public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.ToTable("tb_technology");

        // Primary Key
        builder.HasKey(t => t.TechnologyId)
            .HasName("pk_tb_technology");

        builder.Property(t => t.TechnologyId)
            .HasColumnName("technology_id")
            .ValueGeneratedOnAdd();

        // Properties
        builder.Property(t => t.Name)
            .HasColumnName("technology_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Version)
            .HasColumnName("version")
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(t => t.Category)
            .HasColumnName("technology_category")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasColumnName("technology_description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.HistoryId)
            .HasColumnName("history_id")
            .IsRequired(false);

        // Relationships
        builder.HasOne(t => t.History)
            .WithMany(h => h.Technologies)
            .HasForeignKey(t => t.HistoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.HistoryId)
            .HasDatabaseName("ix_tb_technology_history_id");

        builder.HasIndex(t => t.Name)
            .HasDatabaseName("ix_tb_technology_name");

        builder.HasIndex(t => t.Category)
            .HasDatabaseName("ix_tb_technology_category");
    }
}
