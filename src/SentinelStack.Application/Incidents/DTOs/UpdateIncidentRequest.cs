using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Incidents.DTOs;

/// <summary>
/// Request model for updating an existing incident.
/// </summary>
public class UpdateIncidentRequest
{
    [MaxLength(200)]
    public string? Title { get; init; }

    [MaxLength(4000)]
    public string? Description { get; init; }

    public IncidentSeverity? Severity { get; init; }

    public Guid? ServiceId { get; init; }

    public Guid? AssignedToId { get; init; }
}
