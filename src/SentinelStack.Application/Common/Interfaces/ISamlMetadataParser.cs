namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service for parsing SAML IdP metadata XML
/// </summary>
public interface ISamlMetadataParser
{
    /// <summary>
    /// Parse IdP metadata XML and extract configuration
    /// </summary>
    SamlMetadataResult ParseMetadata(string metadataXml);
}

public record SamlMetadataResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }
    public string IdpEntityId { get; init; } = string.Empty;
    public string SsoServiceUrl { get; init; } = string.Empty;
    public string SigningCertificate { get; init; } = string.Empty;
}
