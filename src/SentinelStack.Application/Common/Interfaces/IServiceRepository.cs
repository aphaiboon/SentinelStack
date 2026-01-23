using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Service entities.
/// </summary>
public interface IServiceRepository : IRepository<Service>
{
    /// <summary>
    /// Gets a service by its unique key for the current tenant.
    /// </summary>
    Task<Service?> GetByKeyAsync(string serviceKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active services for the current tenant.
    /// </summary>
    Task<IReadOnlyList<Service>> GetActiveServicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a service key exists for the current tenant.
    /// </summary>
    Task<bool> KeyExistsAsync(string serviceKey, CancellationToken cancellationToken = default);
}
