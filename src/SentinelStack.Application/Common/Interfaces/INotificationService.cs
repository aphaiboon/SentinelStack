using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service interface for sending notifications across channels.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an incident notification.
    /// </summary>
    Task SendIncidentNotificationAsync(
        Incident incident,
        NotificationType notificationType,
        string recipient,
        string? customMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an incident notification to a user.
    /// </summary>
    Task SendIncidentNotificationToUserAsync(
        Incident incident,
        User user,
        NotificationType notificationType,
        string? customMessage = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an incident notification to multiple recipients.
    /// </summary>
    Task SendIncidentNotificationAsync(
        Incident incident,
        NotificationType notificationType,
        IEnumerable<string> recipients,
        string? customMessage = null,
        CancellationToken cancellationToken = default);
}
