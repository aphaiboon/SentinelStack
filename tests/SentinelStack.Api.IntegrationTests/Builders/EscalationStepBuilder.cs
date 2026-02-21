using Bogus;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test data builder for EscalationStep entity using Bogus.
/// </summary>
public class EscalationStepBuilder
{
    private Guid _escalationPolicyId = Guid.NewGuid();
    private int _stepOrder = 1;
    private int _delayMinutes = 0;
    private NotificationType _notificationType = NotificationType.Email;
    private Guid? _notifyUserId;
    private string? _notifyTeam;
    private string? _notifyEmails;
    private string? _messageTemplate;
    private bool _repeatIfUnacknowledged = false;
    private int? _repeatIntervalMinutes;
    private int? _maxRepeats;

    private static readonly Faker _faker = new();

    /// <summary>
    /// Creates a new EscalationStepBuilder with default values.
    /// </summary>
    public static EscalationStepBuilder Default() => new();

    /// <summary>
    /// Sets the escalation policy ID for the step.
    /// </summary>
    public EscalationStepBuilder ForPolicy(Guid escalationPolicyId)
    {
        _escalationPolicyId = escalationPolicyId;
        return this;
    }

    /// <summary>
    /// Sets the step order.
    /// </summary>
    public EscalationStepBuilder WithOrder(int stepOrder)
    {
        _stepOrder = stepOrder;
        return this;
    }

    /// <summary>
    /// Sets the delay in minutes.
    /// </summary>
    public EscalationStepBuilder WithDelay(int delayMinutes)
    {
        _delayMinutes = delayMinutes;
        return this;
    }

    /// <summary>
    /// Sets the notification type.
    /// </summary>
    public EscalationStepBuilder WithNotificationType(NotificationType notificationType)
    {
        _notificationType = notificationType;
        return this;
    }

    /// <summary>
    /// Sets the user to notify.
    /// </summary>
    public EscalationStepBuilder NotifyUser(Guid userId)
    {
        _notifyUserId = userId;
        return this;
    }

    /// <summary>
    /// Sets the team to notify.
    /// </summary>
    public EscalationStepBuilder NotifyTeam(string team)
    {
        _notifyTeam = team;
        return this;
    }

    /// <summary>
    /// Sets the emails to notify.
    /// </summary>
    public EscalationStepBuilder NotifyEmails(params string[] emails)
    {
        _notifyEmails = string.Join(",", emails);
        return this;
    }

    /// <summary>
    /// Sets the message template.
    /// </summary>
    public EscalationStepBuilder WithMessageTemplate(string messageTemplate)
    {
        _messageTemplate = messageTemplate;
        return this;
    }

    /// <summary>
    /// Configures the step to repeat if unacknowledged.
    /// </summary>
    public EscalationStepBuilder WithRepeat(int intervalMinutes, int? maxRepeats = null)
    {
        _repeatIfUnacknowledged = true;
        _repeatIntervalMinutes = intervalMinutes;
        _maxRepeats = maxRepeats;
        return this;
    }

    /// <summary>
    /// Builds the EscalationStep entity with configured or default values.
    /// </summary>
    public EscalationStep Build()
    {
        return new EscalationStep(
            escalationPolicyId: _escalationPolicyId,
            stepOrder: _stepOrder,
            delayMinutes: _delayMinutes,
            notificationType: _notificationType,
            notifyUserId: _notifyUserId,
            notifyTeam: _notifyTeam,
            notifyEmails: _notifyEmails,
            messageTemplate: _messageTemplate,
            repeatIfUnacknowledged: _repeatIfUnacknowledged,
            repeatIntervalMinutes: _repeatIntervalMinutes,
            maxRepeats: _maxRepeats
        );
    }
}
