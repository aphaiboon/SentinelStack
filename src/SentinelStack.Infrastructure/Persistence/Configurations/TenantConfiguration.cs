using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core entity configuration for Tenant.
/// Implements AR27: Dual Multi-Tenancy Model.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // Table name: snake_case (AR37)
        builder.ToTable("tenants");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // GUID generated in domain

        // Required fields
        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        // AC #1: Subdomain unique constraint and index
        builder.Property(t => t.Subdomain)
            .HasColumnName("subdomain")
            .HasMaxLength(63)
            .IsRequired();

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("ix_tenants_subdomain");

        // Enum fields
        builder.Property(t => t.Tier)
            .HasColumnName("tier")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.IsolationModel)
            .HasColumnName("isolation_model")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Optional fields for SchemaPerTenant isolation
        builder.Property(t => t.SchemaName)
            .HasColumnName("schema_name")
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.ConnectionString)
            .HasColumnName("connection_string")
            .HasMaxLength(500)
            .IsRequired(false);

        // Timestamps
        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.SuspendedAtUtc)
            .HasColumnName("suspended_at")
            .IsRequired(false);

        builder.Property(t => t.UpdatedAtUtc)
            .HasColumnName("updated_at")
            .IsRequired(false);

        // Index for status (fast lookups for active tenants)
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tenants_status");
    }
}
