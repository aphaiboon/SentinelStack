using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100);

        builder.Property(e => e.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(e => e.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(e => e.MfaEnabled)
            .HasColumnName("mfa_enabled")
            .IsRequired();

        builder.Property(e => e.LastLoginAtUtc)
            .HasColumnName("last_login_at_utc");

        builder.Property(e => e.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .IsRequired();

        builder.Property(e => e.LockedUntilUtc)
            .HasColumnName("locked_until_utc");

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
            .HasDatabaseName("ix_users_tenant_id");

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasDatabaseName("uq_users_tenant_email");

        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("ix_users_tenant_active");
    }
}
