using System.Text.Json;
using LinaSys.Core.Domain.AggregatesModel.AuditAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinaSys.Core.Infrastructure.Persistence.EntityConfigurations;

/// <summary>
/// Entity configuration for AuditLog.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs", "core");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.UserId)
            .HasMaxLength(450);

        builder.Property(a => a.UserName)
            .HasMaxLength(256);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45); // Supports IPv6

        builder.Property(a => a.UserAgent)
            .HasMaxLength(500);

        builder.Property(a => a.AdditionalData);

        // Configure JSON columns for old and new values
        builder.Property(a => a.OldValues)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
            .HasColumnType("nvarchar(max)");

        // Indexes for common queries
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.EntityId);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
