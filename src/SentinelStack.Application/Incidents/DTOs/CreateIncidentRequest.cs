using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Incidents.DTOs;

/// <summary>
/// Request model for creating a new incident.
/// </summary>
public class CreateIncidentRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }

    [Required]
    public IncidentSeverity Severity { get; init; }

    public Guid? ServiceId { get; init; }

    public Guid? AssignedToId { get; init; }
}
