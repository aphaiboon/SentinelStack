namespace SentinelStack.Application.Webhooks.DTOs;

/// <summary>
/// Incoming webhook payload wrapper.
/// </summary>
public class WebhookPayloadRequest
{
    /// <summary>
    /// Raw JSON payload from the webhook.
    /// </summary>
    public required object Payload { get; init; }

    /// <summary>
    /// Signature header value for validation (if provided).
    /// </summary>
    public string? Signature { get; init; }
}

/// <summary>
/// Result of processing a webhook payload.
/// </summary>
public class WebhookProcessingResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public Guid? IncidentId { get; init; }
    public string? Error { get; init; }
}
