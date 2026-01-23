using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Escalations.DTOs;

/// <summary>
/// Data transfer object for EscalationPolicy entity.
/// </summary>
public class EscalationPolicyDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public bool IsDefault { get; init; }
    public Guid? ServiceId { get; init; }
    public IReadOnlyList<EscalationStepDto> Steps { get; init; } = new List<EscalationStepDto>();
    public DateTime CreatedAtUtc { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public Guid? UpdatedBy { get; init; }
}

/// <summary>
/// Data transfer object for EscalationStep entity.
/// </summary>
public class EscalationStepDto
{
    public Guid Id { get; init; }
    public int StepOrder { get; init; }
    public int DelayMinutes { get; init; }
    public NotificationType NotificationType { get; init; }
    public string NotificationTypeName => NotificationType.ToString();
    public Guid? NotifyUserId { get; init; }
    public string? NotifyTeam { get; init; }
    public string? NotifyEmails { get; init; }
    public string? MessageTemplate { get; init; }
    public bool RepeatIfUnacknowledged { get; init; }
    public int? RepeatIntervalMinutes { get; init; }
    public int? MaxRepeats { get; init; }
}
