using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Persistence.Configurations;

public class SamlConfigurationConfiguration : IEntityTypeConfiguration<SamlConfiguration>
{
    public void Configure(EntityTypeBuilder<SamlConfiguration> builder)
    {
        builder.ToTable("saml_configurations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(s => s.IdpEntityId)
            .HasColumnName("idp_entity_id")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(s => s.SsoServiceUrl)
            .HasColumnName("sso_service_url")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(s => s.SigningCertificate)
            .HasColumnName("signing_certificate")
            .IsRequired();

        builder.Property(s => s.Enabled)
            .HasColumnName("enabled")
            .IsRequired();

        builder.Property(s => s.IdpMetadataXml)
            .HasColumnName("idp_metadata_xml");

        builder.Property(s => s.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(s => s.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        // Foreign key relationship to Tenant
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_saml_configurations_tenants_tenant_id");

        // Unique index: one SAML config per tenant
        builder.HasIndex(s => s.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_saml_configurations_tenant_id");
    }
}
