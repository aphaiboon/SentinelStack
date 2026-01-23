using System.ComponentModel.DataAnnotations;

namespace SentinelStack.Application.Services.DTOs;

/// <summary>
/// Request model for creating a new service.
/// </summary>
public class CreateServiceRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "ServiceKey must be lowercase alphanumeric with hyphens only")]
    public required string ServiceKey { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [MaxLength(100)]
    public string? OwnerTeam { get; init; }

    [EmailAddress]
    [MaxLength(254)]
    public string? ContactEmail { get; init; }
}
