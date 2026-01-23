using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Infrastructure.BackgroundJobs;

/// <summary>
/// Background job service for processing incident escalations.
/// </summary>
public class EscalationJobService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EscalationJobService> _logger;

    public EscalationJobService(
        IServiceScopeFactory scopeFactory,
        ILogger<EscalationJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Processes pending escalations for unacknowledged incidents.
    /// This job runs on a schedule to check for incidents that need to be escalated.
    /// </summary>
    public async Task ProcessPendingEscalationsAsync()
    {
        _logger.LogInformation("Starting escalation job processing");

        using var scope = _scopeFactory.CreateScope();
        var incidentRepository = scope.ServiceProvider.GetRequiredService<IIncidentRepository>();
        var escalationPolicyRepository = scope.ServiceProvider.GetRequiredService<IEscalationPolicyRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        try
        {
            // Get all open (unacknowledged) incidents
            var openIncidents = await incidentRepository.GetAllAsync(CancellationToken.None);
            var unacknowledgedIncidents = openIncidents
                .Where(i => i.Status == IncidentStatus.Open)
                .ToList();

            _logger.LogInformation("Found {Count} unacknowledged incidents", unacknowledgedIncidents.Count);

            foreach (var incident in unacknowledgedIncidents)
            {
                // Check if incident has an associated service
                if (incident.ServiceId == null)
                {
                    _logger.LogDebug("Incident {IncidentId} has no service, skipping escalation", incident.Id);
                    continue;
                }

                // Get escalation policy for this service
                var policies = await escalationPolicyRepository.GetAllAsync(CancellationToken.None);
                var policy = policies.FirstOrDefault(p =>
                    p.ServiceId == incident.ServiceId && p.IsActive);

                // Fall back to default policy if no service-specific policy
                if (policy == null)
                {
                    policy = policies.FirstOrDefault(p => p.IsDefault && p.IsActive);
                }

                if (policy == null)
                {
                    _logger.LogDebug("No escalation policy found for service {ServiceId}", incident.ServiceId);
                    continue;
                }

                // Determine if escalation is needed based on incident age and policy rules
                var incidentAge = dateTimeProvider.UtcNow - incident.CreatedAtUtc;
                var currentStep = GetCurrentEscalationStep(policy.Steps, incidentAge);

                if (currentStep != null && !HasBeenEscalatedToStep(incident, currentStep))
                {
                    _logger.LogInformation(
                        "Escalating incident {IncidentId} to step {StepOrder}",
                        incident.Id,
                        currentStep.StepOrder);

                    // Build recipient from step configuration
                    var recipient = GetRecipientFromStep(currentStep);

                    if (!string.IsNullOrEmpty(recipient))
                    {
                        // Send escalation notification
                        await notificationService.SendIncidentNotificationAsync(
                            incident,
                            currentStep.NotificationType,
                            recipient,
                            $"Escalation Step {currentStep.StepOrder}: Incident has been open for {FormatDuration(incidentAge)}",
                            CancellationToken.None);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Escalation step {StepOrder} has no configured recipient",
                            currentStep.StepOrder);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing escalations");
            throw;
        }

        _logger.LogInformation("Completed escalation job processing");
    }

    private static EscalationStep? GetCurrentEscalationStep(
        IEnumerable<EscalationStep> steps,
        TimeSpan incidentAge)
    {
        return steps
            .Where(s => incidentAge.TotalMinutes >= s.DelayMinutes)
            .OrderByDescending(s => s.StepOrder)
            .FirstOrDefault();
    }

    private static string? GetRecipientFromStep(EscalationStep step)
    {
        // Priority: NotifyEmails > NotifyTeam > NotifyUserId
        if (!string.IsNullOrEmpty(step.NotifyEmails))
        {
            return step.NotifyEmails;
        }

        if (!string.IsNullOrEmpty(step.NotifyTeam))
        {
            return step.NotifyTeam;
        }

        if (step.NotifyUserId.HasValue)
        {
            return step.NotifyUserId.Value.ToString();
        }

        return null;
    }

    private static bool HasBeenEscalatedToStep(
        Incident incident,
        EscalationStep step)
    {
        // This would check against a tracking table or incident metadata
        // For now, we'll use a simple heuristic based on incident updates
        // In a production system, you would track escalation history
        return false;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes} minutes";
        if (duration.TotalHours < 24)
            return $"{(int)duration.TotalHours} hours";
        return $"{(int)duration.TotalDays} days";
    }
}
