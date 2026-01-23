using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Webhooks.Commands;

/// <summary>
/// Command to process an incoming webhook payload and create an incident.
/// </summary>
public class ProcessWebhookCommand
{
    private readonly IWebhookEndpointRepository _webhookRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessWebhookCommand> _logger;

    public ProcessWebhookCommand(
        IWebhookEndpointRepository webhookRepository,
        IIncidentRepository incidentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessWebhookCommand> logger)
    {
        _webhookRepository = webhookRepository;
        _incidentRepository = incidentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<WebhookProcessingResult> ExecuteAsync(
        string endpointKey,
        string payload,
        string? signature,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get endpoint (bypassing tenant filter for webhook receipt)
            var endpoint = await _webhookRepository.GetByEndpointKeyAsync(endpointKey, cancellationToken);
            if (endpoint == null)
            {
                _logger.LogWarning("Webhook received for unknown endpoint: {EndpointKey}", endpointKey);
                return new WebhookProcessingResult
                {
                    Success = false,
                    Error = "Unknown endpoint"
                };
            }

            if (!endpoint.IsActive)
            {
                _logger.LogWarning("Webhook received for inactive endpoint: {EndpointKey}", endpointKey);
                return new WebhookProcessingResult
                {
                    Success = false,
                    Error = "Endpoint is inactive"
                };
            }

            // Validate signature if provided
            if (!string.IsNullOrEmpty(signature))
            {
                if (!ValidateSignature(payload, signature, endpoint.SecretKey))
                {
                    _logger.LogWarning("Invalid signature for webhook endpoint: {EndpointKey}", endpointKey);
                    return new WebhookProcessingResult
                    {
                        Success = false,
                        Error = "Invalid signature"
                    };
                }
            }

            // Parse the payload
            var parsedPayload = ParsePayload(payload, endpoint);

            // Create incident
            var incident = new Incident(
                endpoint.TenantId,
                parsedPayload.Title,
                parsedPayload.Severity,
                parsedPayload.Description,
                endpoint.DefaultServiceId);

            await _incidentRepository.AddAsync(incident, cancellationToken);

            // Update endpoint stats
            endpoint.RecordReceipt();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Webhook processed successfully. Endpoint: {EndpointKey}, Incident: {IncidentId}",
                endpointKey,
                incident.Id);

            return new WebhookProcessingResult
            {
                Success = true,
                Message = "Incident created successfully",
                IncidentId = incident.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook for endpoint: {EndpointKey}", endpointKey);
            return new WebhookProcessingResult
            {
                Success = false,
                Error = "Internal processing error"
            };
        }
    }

    private static bool ValidateSignature(string payload, string signature, string secretKey)
    {
        try
        {
            // Support multiple signature formats
            // HMAC-SHA256 signature validation
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var expectedSignature = Convert.ToBase64String(hash);

            // Also check hex format
            var expectedHex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Remove common prefixes
            var cleanSignature = signature
                .Replace("sha256=", "")
                .Replace("v0=", "")
                .Trim();

            return cleanSignature == expectedSignature ||
                   cleanSignature.Equals(expectedHex, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static ParsedWebhookPayload ParsePayload(string payload, WebhookEndpoint endpoint)
    {
        var json = JsonDocument.Parse(payload);
        var root = json.RootElement;

        var title = ExtractValue(root, endpoint.TitleMapping) ?? "Alert from " + endpoint.Name;
        var description = ExtractValue(root, endpoint.DescriptionMapping) ?? payload;
        var severityStr = ExtractValue(root, endpoint.SeverityMapping);

        var severity = ParseSeverity(severityStr, endpoint.SourceType, endpoint.DefaultSeverity);

        return new ParsedWebhookPayload
        {
            Title = title,
            Description = description,
            Severity = severity
        };
    }

    private static string? ExtractValue(JsonElement root, string? jsonPath)
    {
        if (string.IsNullOrEmpty(jsonPath))
            return null;

        try
        {
            // Simple JSON path support (e.g., "$.alert.title" or "alert.title")
            var path = jsonPath.TrimStart('$', '.');
            var parts = path.Split('.');

            var current = root;
            foreach (var part in parts)
            {
                if (current.TryGetProperty(part, out var next))
                {
                    current = next;
                }
                else
                {
                    return null;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.GetRawText(),
                _ => current.GetRawText()
            };
        }
        catch
        {
            return null;
        }
    }

    private static IncidentSeverity ParseSeverity(string? severityStr, WebhookSourceType sourceType, IncidentSeverity defaultSeverity)
    {
        if (string.IsNullOrEmpty(severityStr))
            return defaultSeverity;

        var lower = severityStr.ToLowerInvariant();

        // Standard severity names
        if (lower.Contains("critical") || lower.Contains("p1") || lower == "1")
            return IncidentSeverity.Critical;
        if (lower.Contains("high") || lower.Contains("p2") || lower == "2")
            return IncidentSeverity.High;
        if (lower.Contains("medium") || lower.Contains("p3") || lower == "3" || lower.Contains("warning"))
            return IncidentSeverity.Medium;
        if (lower.Contains("low") || lower.Contains("p4") || lower == "4" || lower.Contains("info"))
            return IncidentSeverity.Low;

        // Source-specific mappings
        return sourceType switch
        {
            WebhookSourceType.Datadog when lower == "error" => IncidentSeverity.High,
            WebhookSourceType.Prometheus when lower == "firing" => IncidentSeverity.High,
            _ => defaultSeverity
        };
    }

    private class ParsedWebhookPayload
    {
        public required string Title { get; init; }
        public required string Description { get; init; }
        public IncidentSeverity Severity { get; init; }
    }
}
