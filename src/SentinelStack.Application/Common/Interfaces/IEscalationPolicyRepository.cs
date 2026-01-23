using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository interface for EscalationPolicy entities.
/// </summary>
public interface IEscalationPolicyRepository : IRepository<EscalationPolicy>
{
    /// <summary>
    /// Gets the default escalation policy for the current tenant.
    /// </summary>
    Task<EscalationPolicy?> GetDefaultPolicyAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the escalation policy for a specific service.
    /// </summary>
    Task<EscalationPolicy?> GetByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active policies for the current tenant.
    /// </summary>
    Task<IReadOnlyList<EscalationPolicy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a policy with its steps loaded.
    /// </summary>
    Task<EscalationPolicy?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
}
