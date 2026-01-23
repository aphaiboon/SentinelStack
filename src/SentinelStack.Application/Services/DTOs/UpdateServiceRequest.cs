using System.ComponentModel.DataAnnotations;

namespace SentinelStack.Application.Services.DTOs;

/// <summary>
/// Request model for updating an existing service.
/// </summary>
public class UpdateServiceRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [MaxLength(100)]
    public string? OwnerTeam { get; init; }

    [EmailAddress]
    [MaxLength(254)]
    public string? ContactEmail { get; init; }

    [MaxLength(100)]
    public string? SlackChannel { get; init; }

    public bool? IsActive { get; init; }
}
