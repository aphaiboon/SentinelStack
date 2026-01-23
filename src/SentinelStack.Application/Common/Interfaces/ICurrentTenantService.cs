namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current tenant context.
/// </summary>
public interface ICurrentTenantService
{
    /// <summary>
    /// Gets the current tenant ID from the request context.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the current tenant's connection string for schema-per-tenant isolation.
    /// Returns null for shared-schema tenants.
    /// </summary>
    string? TenantConnectionString { get; }

    /// <summary>
    /// Indicates whether the current tenant uses schema-per-tenant isolation (healthcare).
    /// </summary>
    bool IsIsolatedTenant { get; }
}
