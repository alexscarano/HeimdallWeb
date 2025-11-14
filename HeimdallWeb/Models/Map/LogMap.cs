using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HeimdallWeb.Models.Map;

public class LogMap : IEntityTypeConfiguration<LogModel>
{
    public void Configure(EntityTypeBuilder<LogModel> builder)
    {
        builder.ToTable("tb_log");

        builder.HasKey(l => l.log_id).HasName("pk_tb_log");

        builder.Property(l => l.log_id)
            .HasColumnName("log_id");

        builder.Property(l => l.timestamp)
            .HasColumnName("timestamp")
            .IsRequired();

        builder.Property(l => l.level)
            .HasColumnName("level")
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("Info");

        builder.Property(l => l.source)
            .HasColumnName("source")
            .HasMaxLength(100);

        builder.Property(l => l.message)
            .HasColumnName("message")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(l => l.details)
            .HasColumnName("details")
            .HasColumnType("text");

        builder.Property(l => l.user_id)
            .HasColumnName("user_id");

        builder.Property(l => l.history_id)
            .HasColumnName("history_id");

        builder.Property(l => l.remote_ip)
            .HasMaxLength(32)
            .HasColumnName("remote_ip");

        // relationships
        builder.HasOne(l => l.User)
            .WithMany(u => u.LogModel)
            .HasForeignKey(l => l.user_id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.History)
            .WithMany(h => h.Logs)
            .HasForeignKey(l => l.history_id)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        // optional indexes for common queries
        builder.HasIndex(l => l.timestamp).HasDatabaseName("ix_tb_log_timestamp");
        builder.HasIndex(l => l.level).HasDatabaseName("ix_tb_log_level");
    }
}
