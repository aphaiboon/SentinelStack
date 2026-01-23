using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Webhooks.DTOs;

/// <summary>
/// Data transfer object for WebhookEndpoint entity.
/// </summary>
public class WebhookEndpointDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string EndpointKey { get; init; }
    public required string WebhookUrl { get; init; }
    public bool IsActive { get; init; }
    public WebhookSourceType SourceType { get; init; }
    public string SourceTypeName => SourceType.ToString();
    public Guid? DefaultServiceId { get; init; }
    public IncidentSeverity DefaultSeverity { get; init; }
    public string DefaultSeverityName => DefaultSeverity.ToString();
    public string? TitleMapping { get; init; }
    public string? DescriptionMapping { get; init; }
    public string? SeverityMapping { get; init; }
    public DateTime? LastReceivedAtUtc { get; init; }
    public int TotalReceived { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public Guid? UpdatedBy { get; init; }
}

/// <summary>
/// Extended DTO that includes the secret key (only for creation/regeneration).
/// </summary>
public class WebhookEndpointWithSecretDto : WebhookEndpointDto
{
    public required string SecretKey { get; init; }
}
