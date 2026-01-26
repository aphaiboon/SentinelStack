namespace SentinelStack.Domain.Entities;

/// <summary>
/// SAML 2.0 Identity Provider configuration for tenant SSO authentication.
/// Enterprise tier only feature (FR79).
/// </summary>
public class SamlConfiguration
{
    /// <summary>
    /// Primary key
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Foreign key to Tenant entity - one SAML config per tenant
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// IdP Entity ID from metadata (e.g., "http://www.okta.com/exk123")
    /// </summary>
    public string IdpEntityId { get; private set; } = string.Empty;

    /// <summary>
    /// IdP Single Sign-On Service URL
    /// </summary>
    public string SsoServiceUrl { get; private set; } = string.Empty;

    /// <summary>
    /// X509 certificate for validating SAML assertion signatures (PEM format)
    /// </summary>
    public string SigningCertificate { get; private set; } = string.Empty;

    /// <summary>
    /// Whether SAML authentication is enabled for this tenant
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// Full IdP metadata XML (stored for reference)
    /// </summary>
    public string? IdpMetadataXml { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    // Navigation property
    public Tenant Tenant { get; private set; } = null!;

    // EF Core constructor
    private SamlConfiguration()
    {
    }

    public SamlConfiguration(
        Guid tenantId,
        string idpEntityId,
        string ssoServiceUrl,
        string signingCertificate)
    {
        if (string.IsNullOrWhiteSpace(idpEntityId))
            throw new ArgumentException("IdP Entity ID is required", nameof(idpEntityId));

        if (string.IsNullOrWhiteSpace(ssoServiceUrl))
            throw new ArgumentException("SSO Service URL is required", nameof(ssoServiceUrl));

        if (string.IsNullOrWhiteSpace(signingCertificate))
            throw new ArgumentException("Signing certificate is required", nameof(signingCertificate));

        Id = Guid.NewGuid();
        TenantId = tenantId;
        IdpEntityId = idpEntityId;
        SsoServiceUrl = ssoServiceUrl;
        SigningCertificate = signingCertificate;
        Enabled = true; // Enabled by default
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Disable()
    {
        Enabled = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Enable()
    {
        Enabled = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateMetadata(
        string idpEntityId,
        string ssoServiceUrl,
        string signingCertificate,
        string? idpMetadataXml = null)
    {
        if (string.IsNullOrWhiteSpace(idpEntityId))
            throw new ArgumentException("IdP Entity ID is required", nameof(idpEntityId));

        if (string.IsNullOrWhiteSpace(ssoServiceUrl))
            throw new ArgumentException("SSO Service URL is required", nameof(ssoServiceUrl));

        if (string.IsNullOrWhiteSpace(signingCertificate))
            throw new ArgumentException("Signing certificate is required", nameof(signingCertificate));

        IdpEntityId = idpEntityId;
        SsoServiceUrl = ssoServiceUrl;
        SigningCertificate = signingCertificate;
        IdpMetadataXml = idpMetadataXml;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
