using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository for tenant management operations.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Creates a new tenant.
    /// </summary>
    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by subdomain.
    /// AC #2: Subdomain lookup for tenant resolution.
    /// </summary>
    Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tenants with pagination.
    /// AC #5: List all tenants.
    /// </summary>
    Task<(List<Tenant> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tenant.
    /// </summary>
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends a tenant (FR68).
    /// </summary>
    Task SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a suspended tenant.
    /// </summary>
    Task ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}
