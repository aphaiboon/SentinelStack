using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Incidents.DTOs;

/// <summary>
/// Data transfer object for Incident entity.
/// </summary>
public class IncidentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public IncidentSeverity Severity { get; init; }
    public IncidentStatus Status { get; init; }
    public Guid? ServiceId { get; init; }
    public string? ServiceName { get; init; }
    public Guid? AssignedToId { get; init; }
    public DateTime? AcknowledgedAtUtc { get; init; }
    public Guid? AcknowledgedBy { get; init; }
    public DateTime? ResolvedAtUtc { get; init; }
    public Guid? ResolvedBy { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public Guid? UpdatedBy { get; init; }
}
