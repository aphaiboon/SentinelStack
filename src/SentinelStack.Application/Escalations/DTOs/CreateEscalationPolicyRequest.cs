using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Escalations.DTOs;

/// <summary>
/// Request model for creating an escalation policy.
/// </summary>
public class CreateEscalationPolicyRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public Guid? ServiceId { get; init; }

    public bool IsDefault { get; init; }

    public List<CreateEscalationStepRequest> Steps { get; init; } = new();
}

/// <summary>
/// Request model for creating an escalation step.
/// </summary>
public class CreateEscalationStepRequest
{
    [Range(1, 100)]
    public int StepOrder { get; init; }

    [Range(0, 10080)] // Up to 1 week in minutes
    public int DelayMinutes { get; init; }

    public NotificationType NotificationType { get; init; }

    public Guid? NotifyUserId { get; init; }

    [MaxLength(100)]
    public string? NotifyTeam { get; init; }

    [MaxLength(500)]
    public string? NotifyEmails { get; init; }

    [MaxLength(1000)]
    public string? MessageTemplate { get; init; }

    public bool RepeatIfUnacknowledged { get; init; }

    [Range(1, 1440)] // Up to 24 hours
    public int? RepeatIntervalMinutes { get; init; }

    [Range(1, 100)]
    public int? MaxRepeats { get; init; }
}
