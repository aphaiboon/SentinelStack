namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service for dynamically loading and configuring SAML Identity Providers based on tenant configuration.
/// </summary>
public interface ISamlIdpConfigurationService
{
    /// <summary>
    /// Checks if SAML is configured and enabled for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if SAML is configured and enabled, false otherwise</returns>
    Task<bool> IsSamlConfiguredForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the tenant ID associated with an IdP Entity ID.
    /// Used for IdP-initiated flows to determine which tenant is authenticating.
    /// </summary>
    /// <param name="idpEntityId">IdP Entity ID from SAML assertion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tenant ID, or null if no tenant is configured with this IdP</returns>
    Task<Guid?> GetTenantIdByIdpEntityIdAsync(string idpEntityId, CancellationToken cancellationToken = default);
}
