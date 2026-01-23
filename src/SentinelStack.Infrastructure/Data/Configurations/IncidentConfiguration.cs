using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Infrastructure.Data.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("incidents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(e => e.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ServiceId)
            .HasColumnName("service_id");

        builder.Property(e => e.AcknowledgedBy)
            .HasColumnName("acknowledged_by");

        builder.Property(e => e.AcknowledgedAtUtc)
            .HasColumnName("acknowledged_at_utc");

        builder.Property(e => e.ResolvedAtUtc)
            .HasColumnName("resolved_at_utc");

        builder.Property(e => e.ResolvedBy)
            .HasColumnName("resolved_by");

        builder.Property(e => e.ArchivedAtUtc)
            .HasColumnName("archived_at_utc");

        builder.Property(e => e.DeletedAtUtc)
            .HasColumnName("deleted_at_utc");

        // Audit fields from AuditableEntity
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(e => e.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(e => e.UpdatedBy)
            .HasColumnName("updated_by");

        // Indexes
        builder.HasIndex(e => e.TenantId)
            .HasDatabaseName("ix_incidents_tenant_id");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("ix_incidents_status");

        builder.HasIndex(e => e.Severity)
            .HasDatabaseName("ix_incidents_severity");

        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_incidents_tenant_status");

        builder.HasIndex(e => new { e.TenantId, e.CreatedAtUtc })
            .HasDatabaseName("ix_incidents_tenant_created");

        // Soft delete filter
        builder.HasQueryFilter(e => e.DeletedAtUtc == null);
    }
}
