namespace SentinelStack.Domain.Common;

/// <summary>
/// Interface for entities that belong to a tenant.
/// All tenant-scoped data must implement this interface.
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// The tenant ID this entity belongs to.
    /// </summary>
    Guid TenantId { get; }
}
