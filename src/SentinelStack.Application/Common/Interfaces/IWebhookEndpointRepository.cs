using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository interface for WebhookEndpoint entities.
/// </summary>
public interface IWebhookEndpointRepository : IRepository<WebhookEndpoint>
{
    /// <summary>
    /// Gets a webhook endpoint by its endpoint key.
    /// Note: This method ignores tenant filtering for webhook receipt.
    /// </summary>
    Task<WebhookEndpoint?> GetByEndpointKeyAsync(string endpointKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active webhook endpoints for the current tenant.
    /// </summary>
    Task<IReadOnlyList<WebhookEndpoint>> GetActiveEndpointsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an endpoint key already exists.
    /// </summary>
    Task<bool> EndpointKeyExistsAsync(string endpointKey, CancellationToken cancellationToken = default);
}
