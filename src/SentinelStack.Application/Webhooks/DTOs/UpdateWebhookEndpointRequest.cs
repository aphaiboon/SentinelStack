using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Webhooks.DTOs;

/// <summary>
/// Request model for updating a webhook endpoint.
/// </summary>
public class UpdateWebhookEndpointRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public WebhookSourceType SourceType { get; init; }

    public Guid? DefaultServiceId { get; init; }

    public IncidentSeverity DefaultSeverity { get; init; }

    public bool? IsActive { get; init; }

    [MaxLength(200)]
    public string? TitleMapping { get; init; }

    [MaxLength(200)]
    public string? DescriptionMapping { get; init; }

    [MaxLength(200)]
    public string? SeverityMapping { get; init; }
}
