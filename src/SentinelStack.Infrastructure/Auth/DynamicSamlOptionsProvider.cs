using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using SentinelStack.Application.Common.Interfaces;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;

namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// Provides dynamic SAML configuration based on tenant's stored SAML settings.
/// Implements the IOptionsMonitor pattern to allow runtime IdP configuration.
/// </summary>
public class DynamicSamlOptionsProvider
{
    private readonly ISamlConfigurationRepository _samlConfigRepository;
    private readonly ILogger<DynamicSamlOptionsProvider> _logger;

    public DynamicSamlOptionsProvider(
        ISamlConfigurationRepository samlConfigRepository,
        ILogger<DynamicSamlOptionsProvider> logger)
    {
        _samlConfigRepository = samlConfigRepository;
        _logger = logger;
    }

    /// <summary>
    /// Configures SAML IdP for a specific tenant at runtime.
    /// Called by Sustainsys.Saml2 middleware when processing SAML requests.
    /// </summary>
    public async Task<IdentityProvider?> GetIdpForTenantAsync(Guid tenantId)
    {
        var samlConfig = await _samlConfigRepository.GetByTenantIdAsync(tenantId);

        if (samlConfig == null || !samlConfig.Enabled)
        {
            _logger.LogWarning("No enabled SAML configuration found for tenant {TenantId}", tenantId);
            return null;
        }

        try
        {
            // Create IdP configuration from stored settings
            // Sustainsys.Saml2 IdentityProvider requires SPOptions parameter
            // For dynamic configuration, we pass null and configure properties directly
            var idp = new IdentityProvider(
                new EntityId(samlConfig.IdpEntityId),
                spOptions: null)
            {
                SingleSignOnServiceUrl = new Uri(samlConfig.SsoServiceUrl),
                AllowUnsolicitedAuthnResponse = true, // Enable IdP-initiated SSO (AC #1)
                WantAuthnRequestsSigned = false // SP doesn't sign requests (typical for SaaS)
            };

            // Add signing certificate for assertion validation
            var certificate = LoadCertificateFromBase64(samlConfig.SigningCertificate);
            if (certificate != null)
            {
                idp.SigningKeys.AddConfiguredKey(certificate);

                _logger.LogInformation(
                    "Configured SAML IdP {IdpEntityId} for tenant {TenantId}",
                    samlConfig.IdpEntityId,
                    tenantId);

                return idp;
            }
            else
            {
                _logger.LogError(
                    "Failed to load signing certificate for tenant {TenantId}",
                    tenantId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to configure IdP for tenant {TenantId}: {Error}",
                tenantId,
                ex.Message);
            return null;
        }
    }

    private X509Certificate2? LoadCertificateFromBase64(string certificateBase64)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(certificateBase64))
            {
                return null;
            }

            // Remove whitespace and newlines
            var cleanedCert = certificateBase64
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(" ", "");

            // Convert from Base64 to bytes
            var certBytes = Convert.FromBase64String(cleanedCert);

            // Create X509Certificate2
            return new X509Certificate2(certBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate from Base64: {Error}", ex.Message);
            return null;
        }
    }
}
