using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EntityId)
            .HasColumnName("entity_id");

        builder.Property(e => e.UserId)
            .HasColumnName("user_id");

        builder.Property(e => e.UserEmail)
            .HasColumnName("user_email")
            .HasMaxLength(256);

        builder.Property(e => e.OccurredAtUtc)
            .HasColumnName("occurred_at_utc")
            .IsRequired();

        builder.Property(e => e.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45); // IPv6 max length

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(e => e.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("jsonb");

        builder.Property(e => e.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("jsonb");

        builder.Property(e => e.Notes)
            .HasColumnName("notes")
            .HasMaxLength(1000);

        builder.Property(e => e.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(50);

        // Indexes for audit log queries
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_audit_logs_tenant_id");

        builder.HasIndex(e => e.OccurredAtUtc)
            .HasDatabaseName("ix_audit_logs_occurred_at");

        builder.HasIndex(e => new { e.TenantId, e.OccurredAtUtc })
            .HasDatabaseName("ix_audit_logs_tenant_occurred");

        builder.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId })
            .HasDatabaseName("ix_audit_logs_tenant_entity");

        builder.HasIndex(e => new { e.TenantId, e.UserId })
            .HasDatabaseName("ix_audit_logs_tenant_user");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_audit_logs_correlation");

        // Note: Audit logs are append-only. No updates or deletes allowed.
        // This is enforced at the application layer.
    }
}
