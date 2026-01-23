using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(e => e.ServiceKey)
            .HasColumnName("service_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.OwnerTeam)
            .HasColumnName("owner_team")
            .HasMaxLength(200);

        builder.Property(e => e.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(256);

        builder.Property(e => e.SlackChannel)
            .HasColumnName("slack_channel")
            .HasMaxLength(100);

        builder.Property(e => e.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");

        // Audit fields
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
            .HasDatabaseName("ix_services_tenant_id");

        builder.HasIndex(e => new { e.TenantId, e.ServiceKey })
            .IsUnique()
            .HasDatabaseName("uq_services_tenant_key");

        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("ix_services_tenant_active");
    }
}
