using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a tenant organization in the multi-tenant platform.
/// Implements AR27: Dual Multi-Tenancy Model (SharedSchema vs SchemaPerTenant).
/// </summary>
public class Tenant
{
    /// <summary>
    /// Unique identifier for the tenant
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Display name of the tenant organization
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Subdomain for tenant-specific routing (e.g., "acme" for acme.sentinelstack.io)
    /// Must be DNS-safe: lowercase, alphanumeric, hyphens, 3-63 characters
    /// </summary>
    public string Subdomain { get; private set; } = string.Empty;

    /// <summary>
    /// Subscription tier determining features and pricing
    /// </summary>
    public TenantTier Tier { get; private set; }

    /// <summary>
    /// Database isolation strategy (auto-set based on tier)
    /// </summary>
    public IsolationModel IsolationModel { get; private set; }

    /// <summary>
    /// Current status of the tenant account
    /// </summary>
    public TenantStatus Status { get; private set; }

    /// <summary>
    /// PostgreSQL schema name for SchemaPerTenant isolation (Healthcare tier)
    /// Format: "tenant_{guid_without_hyphens}"
    /// </summary>
    public string? SchemaName { get; private set; }

    /// <summary>
    /// Optional dedicated connection string for database-per-tenant model (future)
    /// </summary>
    public string? ConnectionString { get; private set; }

    /// <summary>
    /// Timestamp when tenant was created
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when tenant was suspended (null if active)
    /// </summary>
    public DateTime? SuspendedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when tenant was last updated
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    private Tenant() { } // EF Core requires parameterless constructor

    /// <summary>
    /// Creates a new tenant with auto-configured isolation model.
    /// AC #3, #4: Healthcare → SchemaPerTenant, others → SharedSchema
    /// </summary>
    public Tenant(string name, string subdomain, TenantTier tier)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain cannot be empty", nameof(subdomain));

        // Convert to lowercase first
        subdomain = subdomain.ToLowerInvariant();

        // AC #1: Validate subdomain is DNS-safe (lowercase, alphanumeric, hyphens)
        if (!IsValidSubdomain(subdomain))
            throw new ArgumentException(
                "Subdomain must be lowercase alphanumeric with hyphens, 3-63 characters (DNS-safe)",
                nameof(subdomain));

        Id = Guid.NewGuid();
        Name = name;
        Subdomain = subdomain;
        Tier = tier;
        Status = TenantStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;

        // AC #3, #4: Auto-set isolation model based on tier
        IsolationModel = tier == TenantTier.Healthcare
            ? IsolationModel.SchemaPerTenant
            : IsolationModel.SharedSchema;

        // Generate schema name for SchemaPerTenant isolation
        if (IsolationModel == IsolationModel.SchemaPerTenant)
        {
            SchemaName = $"tenant_{Id:N}"; // N format removes hyphens
        }
    }

    /// <summary>
    /// Validates subdomain format (DNS-safe, 3-63 characters).
    /// Pattern: ^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$
    /// </summary>
    private static bool IsValidSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
            return false;

        if (subdomain.Length < 3 || subdomain.Length > 63)
            return false;

        // Must start and end with alphanumeric
        if (!char.IsLetterOrDigit(subdomain[0]) || !char.IsLetterOrDigit(subdomain[^1]))
            return false;

        // Must be lowercase alphanumeric with hyphens
        return subdomain.All(c => char.IsLower(c) || char.IsDigit(c) || c == '-');
    }

    /// <summary>
    /// Suspends the tenant account.
    /// FR68: Suspended tenants cannot access platform, data retained.
    /// </summary>
    public void Suspend()
    {
        if (Status == TenantStatus.Suspended)
            return; // Already suspended

        Status = TenantStatus.Suspended;
        SuspendedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates a suspended tenant account.
    /// </summary>
    public void Activate()
    {
        if (Status == TenantStatus.Active)
            return; // Already active

        Status = TenantStatus.Active;
        SuspendedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates tenant name.
    /// </summary>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty", nameof(name));

        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft-deletes the tenant (GDPR compliance, data retention).
    /// </summary>
    public void Delete()
    {
        Status = TenantStatus.Deleted;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
