using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Common.Settings;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Notification service that routes notifications to appropriate channels.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;
    private readonly ApplicationSettings _settings;

    public NotificationService(
        IEmailService emailService,
        IOptions<ApplicationSettings> settings,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendIncidentNotificationAsync(
        Incident incident,
        NotificationType notificationType,
        string recipient,
        string? customMessage = null,
        CancellationToken cancellationToken = default)
    {
        await SendIncidentNotificationAsync(incident, notificationType, new[] { recipient }, customMessage, cancellationToken);
    }

    public async Task SendIncidentNotificationToUserAsync(
        Incident incident,
        User user,
        NotificationType notificationType,
        string? customMessage = null,
        CancellationToken cancellationToken = default)
    {
        switch (notificationType)
        {
            case NotificationType.Email:
                await SendEmailNotificationAsync(incident, user.Email, customMessage, cancellationToken);
                break;

            case NotificationType.Slack:
                _logger.LogWarning("Slack notifications not yet implemented");
                break;

            case NotificationType.Sms:
                _logger.LogWarning("SMS notifications not yet implemented");
                break;

            case NotificationType.Webhook:
                _logger.LogWarning("Webhook notifications should use the outgoing webhook system");
                break;

            case NotificationType.Push:
                _logger.LogWarning("Push notifications not yet implemented");
                break;

            case NotificationType.PhoneCall:
                _logger.LogWarning("Phone call notifications not yet implemented");
                break;

            default:
                _logger.LogWarning("Unknown notification type: {NotificationType}", notificationType);
                break;
        }
    }

    public async Task SendIncidentNotificationAsync(
        Incident incident,
        NotificationType notificationType,
        IEnumerable<string> recipients,
        string? customMessage = null,
        CancellationToken cancellationToken = default)
    {
        var recipientList = recipients.ToList();

        switch (notificationType)
        {
            case NotificationType.Email:
                foreach (var recipient in recipientList)
                {
                    await SendEmailNotificationAsync(incident, recipient, customMessage, cancellationToken);
                }
                break;

            case NotificationType.Slack:
                _logger.LogWarning("Slack notifications not yet implemented. Would notify: {Recipients}", string.Join(", ", recipientList));
                break;

            case NotificationType.Sms:
                _logger.LogWarning("SMS notifications not yet implemented. Would notify: {Recipients}", string.Join(", ", recipientList));
                break;

            case NotificationType.Webhook:
                _logger.LogWarning("Webhook notifications should use the outgoing webhook system");
                break;

            case NotificationType.Push:
                _logger.LogWarning("Push notifications not yet implemented");
                break;

            case NotificationType.PhoneCall:
                _logger.LogWarning("Phone call notifications not yet implemented");
                break;

            default:
                _logger.LogWarning("Unknown notification type: {NotificationType}", notificationType);
                break;
        }
    }

    private async Task SendEmailNotificationAsync(
        Incident incident,
        string recipient,
        string? customMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var templateName = GetTemplateForIncidentStatus(incident.Status);
            var templateData = CreateTemplateData(incident, customMessage);

            await _emailService.SendTemplatedEmailAsync(recipient, templateName, templateData, cancellationToken);

            _logger.LogInformation(
                "Email notification sent for incident {IncidentId} to {Recipient}",
                incident.Id,
                recipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email notification for incident {IncidentId} to {Recipient}",
                incident.Id,
                recipient);
        }
    }

    private static string GetTemplateForIncidentStatus(IncidentStatus status)
    {
        return status switch
        {
            IncidentStatus.Open => "IncidentCreated",
            IncidentStatus.Acknowledged => "IncidentAcknowledged",
            IncidentStatus.Resolved or IncidentStatus.Closed => "IncidentResolved",
            _ => "IncidentEscalation"
        };
    }

    private object CreateTemplateData(Incident incident, string? customMessage)
    {
        return new
        {
            incident.Title,
            incident.Description,
            Severity = incident.Severity.ToString(),
            Status = incident.Status.ToString(),
            ServiceName = "N/A", // Would need to load service
            IncidentUrl = $"{_settings.BaseUrl}/incidents/{incident.Id}",
            AcknowledgedBy = "User", // Would need to load user
            AcknowledgedAt = incident.AcknowledgedAtUtc?.ToString("g") ?? "N/A",
            ResolvedBy = "User", // Would need to load user
            ResolvedAt = incident.ResolvedAtUtc?.ToString("g") ?? "N/A",
            Duration = CalculateDuration(incident),
            EscalationStep = 1,
            OpenSince = incident.CreatedAtUtc.ToString("g"),
            CustomMessage = customMessage
        };
    }

    private static string CalculateDuration(Incident incident)
    {
        if (incident.ResolvedAtUtc == null)
        {
            return "Ongoing";
        }

        var duration = incident.ResolvedAtUtc.Value - incident.CreatedAtUtc;
        if (duration.TotalMinutes < 60)
        {
            return $"{(int)duration.TotalMinutes} minutes";
        }

        if (duration.TotalHours < 24)
        {
            return $"{(int)duration.TotalHours} hours";
        }

        return $"{(int)duration.TotalDays} days";
    }
}
