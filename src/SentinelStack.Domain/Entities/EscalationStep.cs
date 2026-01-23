using SentinelStack.Domain.Common;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a step in an escalation policy.
/// </summary>
public class EscalationStep : BaseEntity
{
    /// <summary>
    /// The escalation policy this step belongs to.
    /// </summary>
    public Guid EscalationPolicyId { get; private set; }

    /// <summary>
    /// Order of this step in the escalation sequence (1-based).
    /// </summary>
    public int StepOrder { get; private set; }

    /// <summary>
    /// Minutes to wait before executing this step.
    /// </summary>
    public int DelayMinutes { get; private set; }

    /// <summary>
    /// The type of notification to send.
    /// </summary>
    public NotificationType NotificationType { get; private set; }

    /// <summary>
    /// Target user to notify (if applicable).
    /// </summary>
    public Guid? NotifyUserId { get; private set; }

    /// <summary>
    /// Target team/group to notify (if applicable).
    /// </summary>
    public string? NotifyTeam { get; private set; }

    /// <summary>
    /// Email addresses to notify (comma-separated).
    /// </summary>
    public string? NotifyEmails { get; private set; }

    /// <summary>
    /// Custom message template for this step.
    /// </summary>
    public string? MessageTemplate { get; private set; }

    /// <summary>
    /// Whether to repeat this step if not acknowledged.
    /// </summary>
    public bool RepeatIfUnacknowledged { get; private set; }

    /// <summary>
    /// Minutes between repeats (if RepeatIfUnacknowledged is true).
    /// </summary>
    public int? RepeatIntervalMinutes { get; private set; }

    /// <summary>
    /// Maximum number of repeats (null = unlimited).
    /// </summary>
    public int? MaxRepeats { get; private set; }

    /// <summary>
    /// Navigation property to the policy.
    /// </summary>
    public EscalationPolicy? EscalationPolicy { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EscalationStep() { }

    /// <summary>
    /// Creates a new escalation step.
    /// </summary>
    public EscalationStep(
        Guid escalationPolicyId,
        int stepOrder,
        int delayMinutes,
        NotificationType notificationType,
        Guid? notifyUserId = null,
        string? notifyTeam = null,
        string? notifyEmails = null,
        string? messageTemplate = null,
        bool repeatIfUnacknowledged = false,
        int? repeatIntervalMinutes = null,
        int? maxRepeats = null)
    {
        EscalationPolicyId = escalationPolicyId;
        StepOrder = stepOrder;
        DelayMinutes = delayMinutes;
        NotificationType = notificationType;
        NotifyUserId = notifyUserId;
        NotifyTeam = notifyTeam;
        NotifyEmails = notifyEmails;
        MessageTemplate = messageTemplate;
        RepeatIfUnacknowledged = repeatIfUnacknowledged;
        RepeatIntervalMinutes = repeatIntervalMinutes;
        MaxRepeats = maxRepeats;
    }

    /// <summary>
    /// Updates the step configuration.
    /// </summary>
    public void Update(
        int stepOrder,
        int delayMinutes,
        NotificationType notificationType,
        Guid? notifyUserId,
        string? notifyTeam,
        string? notifyEmails,
        string? messageTemplate,
        bool repeatIfUnacknowledged,
        int? repeatIntervalMinutes,
        int? maxRepeats)
    {
        StepOrder = stepOrder;
        DelayMinutes = delayMinutes;
        NotificationType = notificationType;
        NotifyUserId = notifyUserId;
        NotifyTeam = notifyTeam;
        NotifyEmails = notifyEmails;
        MessageTemplate = messageTemplate;
        RepeatIfUnacknowledged = repeatIfUnacknowledged;
        RepeatIntervalMinutes = repeatIntervalMinutes;
        MaxRepeats = maxRepeats;
    }
}
