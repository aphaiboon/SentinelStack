namespace SentinelStack.Domain.Enums;

/// <summary>
/// Types of notifications that can be sent during escalation.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Send an email notification.
    /// </summary>
    Email = 0,

    /// <summary>
    /// Send a Slack notification (Beta feature).
    /// </summary>
    Slack = 1,

    /// <summary>
    /// Send an SMS notification (future feature).
    /// </summary>
    Sms = 2,

    /// <summary>
    /// Send a webhook notification.
    /// </summary>
    Webhook = 3,

    /// <summary>
    /// Send a push notification (future feature).
    /// </summary>
    Push = 4,

    /// <summary>
    /// Make a phone call (future feature).
    /// </summary>
    PhoneCall = 5
}
