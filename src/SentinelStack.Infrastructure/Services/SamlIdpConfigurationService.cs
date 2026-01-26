using Microsoft.Extensions.Logging;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Service for dynamically loading and configuring SAML Identity Providers based on tenant configuration.
/// </summary>
public class SamlIdpConfigurationService : ISamlIdpConfigurationService
{
    private readonly ISamlConfigurationRepository _samlConfigRepository;
    private readonly ILogger<SamlIdpConfigurationService> _logger;

    public SamlIdpConfigurationService(
        ISamlConfigurationRepository samlConfigRepository,
        ILogger<SamlIdpConfigurationService> logger)
    {
        _samlConfigRepository = samlConfigRepository;
        _logger = logger;
    }

    public async Task<bool> IsSamlConfiguredForTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var samlConfig = await _samlConfigRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        return samlConfig != null && samlConfig.Enabled;
    }

    public async Task<Guid?> GetTenantIdByIdpEntityIdAsync(string idpEntityId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idpEntityId))
        {
            return null;
        }

        _logger.LogDebug("Looking up tenant for IdP Entity ID: {IdpEntityId}", idpEntityId);

        var samlConfig = await _samlConfigRepository.GetByIdpEntityIdAsync(idpEntityId, cancellationToken);
        return samlConfig?.TenantId;
    }
}
