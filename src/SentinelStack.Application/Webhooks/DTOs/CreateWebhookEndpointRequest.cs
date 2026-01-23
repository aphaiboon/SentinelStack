using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Webhooks.DTOs;

/// <summary>
/// Request model for creating a webhook endpoint.
/// </summary>
public class CreateWebhookEndpointRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "EndpointKey must be lowercase alphanumeric with hyphens only")]
    public required string EndpointKey { get; init; }

    public WebhookSourceType SourceType { get; init; } = WebhookSourceType.Custom;

    public Guid? DefaultServiceId { get; init; }

    public IncidentSeverity DefaultSeverity { get; init; } = IncidentSeverity.Medium;

    /// <summary>
    /// JSON path for extracting incident title (e.g., "$.alert.title").
    /// </summary>
    [MaxLength(200)]
    public string? TitleMapping { get; init; }

    /// <summary>
    /// JSON path for extracting incident description (e.g., "$.alert.body").
    /// </summary>
    [MaxLength(200)]
    public string? DescriptionMapping { get; init; }

    /// <summary>
    /// JSON path for extracting severity (e.g., "$.alert.severity").
    /// </summary>
    [MaxLength(200)]
    public string? SeverityMapping { get; init; }
}
