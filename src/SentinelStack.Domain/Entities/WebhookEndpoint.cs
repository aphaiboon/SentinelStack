using SentinelStack.Domain.Common;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a configured webhook endpoint for receiving external alerts.
/// </summary>
public class WebhookEndpoint : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this endpoint belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Name of the webhook endpoint.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description of what this webhook receives.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Unique key used in the webhook URL path.
    /// </summary>
    public string EndpointKey { get; private set; } = string.Empty;

    /// <summary>
    /// Secret key for validating webhook signatures.
    /// </summary>
    public string SecretKey { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this endpoint is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The source type (e.g., Datadog, PagerDuty, custom).
    /// </summary>
    public WebhookSourceType SourceType { get; private set; }

    /// <summary>
    /// Default service to associate incoming incidents with.
    /// </summary>
    public Guid? DefaultServiceId { get; private set; }

    /// <summary>
    /// Default severity for incoming incidents.
    /// </summary>
    public IncidentSeverity DefaultSeverity { get; private set; }

    /// <summary>
    /// JSON path expression for extracting incident title.
    /// </summary>
    public string? TitleMapping { get; private set; }

    /// <summary>
    /// JSON path expression for extracting incident description.
    /// </summary>
    public string? DescriptionMapping { get; private set; }

    /// <summary>
    /// JSON path expression for extracting severity.
    /// </summary>
    public string? SeverityMapping { get; private set; }

    /// <summary>
    /// Last time a webhook was received.
    /// </summary>
    public DateTime? LastReceivedAtUtc { get; private set; }

    /// <summary>
    /// Total number of webhooks received.
    /// </summary>
    public int TotalReceived { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private WebhookEndpoint() { }

    /// <summary>
    /// Creates a new webhook endpoint.
    /// </summary>
    public WebhookEndpoint(
        Guid tenantId,
        string name,
        string endpointKey,
        string secretKey,
        WebhookSourceType sourceType,
        string? description = null,
        Guid? defaultServiceId = null,
        IncidentSeverity defaultSeverity = IncidentSeverity.Medium)
    {
        TenantId = tenantId;
        Name = name;
        EndpointKey = endpointKey.ToLowerInvariant();
        SecretKey = secretKey;
        SourceType = sourceType;
        Description = description;
        DefaultServiceId = defaultServiceId;
        DefaultSeverity = defaultSeverity;
        IsActive = true;
        TotalReceived = 0;
    }

    /// <summary>
    /// Updates the endpoint configuration.
    /// </summary>
    public void Update(
        string name,
        string? description,
        WebhookSourceType sourceType,
        Guid? defaultServiceId,
        IncidentSeverity defaultSeverity)
    {
        Name = name;
        Description = description;
        SourceType = sourceType;
        DefaultServiceId = defaultServiceId;
        DefaultSeverity = defaultSeverity;
    }

    /// <summary>
    /// Sets the field mappings for payload extraction.
    /// </summary>
    public void SetMappings(string? titleMapping, string? descriptionMapping, string? severityMapping)
    {
        TitleMapping = titleMapping;
        DescriptionMapping = descriptionMapping;
        SeverityMapping = severityMapping;
    }

    /// <summary>
    /// Regenerates the secret key.
    /// </summary>
    public void RegenerateSecretKey(string newSecretKey)
    {
        SecretKey = newSecretKey;
    }

    /// <summary>
    /// Records a webhook receipt.
    /// </summary>
    public void RecordReceipt()
    {
        LastReceivedAtUtc = DateTime.UtcNow;
        TotalReceived++;
    }

    /// <summary>
    /// Activates the endpoint.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the endpoint.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }
}
