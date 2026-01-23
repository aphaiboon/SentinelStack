namespace SentinelStack.Domain.Enums;

/// <summary>
/// Types of webhook sources for automatic payload parsing.
/// </summary>
public enum WebhookSourceType
{
    /// <summary>
    /// Custom/generic webhook format.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Datadog alert webhooks.
    /// </summary>
    Datadog = 1,

    /// <summary>
    /// PagerDuty event webhooks.
    /// </summary>
    PagerDuty = 2,

    /// <summary>
    /// AWS CloudWatch alarm webhooks.
    /// </summary>
    CloudWatch = 3,

    /// <summary>
    /// Prometheus AlertManager webhooks.
    /// </summary>
    Prometheus = 4,

    /// <summary>
    /// Grafana alert webhooks.
    /// </summary>
    Grafana = 5,

    /// <summary>
    /// New Relic alert webhooks.
    /// </summary>
    NewRelic = 6,

    /// <summary>
    /// Azure Monitor alert webhooks.
    /// </summary>
    AzureMonitor = 7,

    /// <summary>
    /// GitHub webhooks for repository events.
    /// </summary>
    GitHub = 8,

    /// <summary>
    /// Generic JSON webhook.
    /// </summary>
    GenericJson = 9
}
