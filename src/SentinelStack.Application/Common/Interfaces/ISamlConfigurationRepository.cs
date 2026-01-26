using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository for SAML configuration persistence.
/// </summary>
public interface ISamlConfigurationRepository
{
    /// <summary>
    /// Get SAML configuration for a tenant
    /// </summary>
    Task<SamlConfiguration?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get SAML configuration by IdP Entity ID (for IdP-initiated flow)
    /// </summary>
    Task<SamlConfiguration?> GetByIdpEntityIdAsync(string idpEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update SAML configuration for a tenant
    /// </summary>
    Task<SamlConfiguration> CreateOrUpdateAsync(SamlConfiguration samlConfiguration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete SAML configuration for a tenant
    /// </summary>
    Task DeleteAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
