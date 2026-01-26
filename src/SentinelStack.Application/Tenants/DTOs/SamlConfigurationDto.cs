namespace SentinelStack.Application.Tenants.DTOs;

public record SamlConfigurationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string IdpEntityId { get; init; } = string.Empty;
    public string SsoServiceUrl { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public record ConfigureSamlRequest
{
    public string IdpMetadataXml { get; init; } = string.Empty;
}
