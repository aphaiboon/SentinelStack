using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Common.Settings;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Webhooks.Queries;

/// <summary>
/// Query to get a single webhook endpoint.
/// </summary>
public class GetWebhookEndpointQuery
{
    private readonly IWebhookEndpointRepository _webhookRepository;
    private readonly string _baseUrl;

    public GetWebhookEndpointQuery(
        IWebhookEndpointRepository webhookRepository,
        IOptions<ApplicationSettings> settings)
    {
        _webhookRepository = webhookRepository;
        _baseUrl = settings.Value.BaseUrl;
    }

    public async Task<WebhookEndpointDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var endpoint = await _webhookRepository.GetByIdAsync(id, cancellationToken);
        return endpoint == null ? null : MapToDto(endpoint);
    }

    private WebhookEndpointDto MapToDto(WebhookEndpoint endpoint) => new()
    {
        Id = endpoint.Id,
        TenantId = endpoint.TenantId,
        Name = endpoint.Name,
        Description = endpoint.Description,
        EndpointKey = endpoint.EndpointKey,
        WebhookUrl = $"{_baseUrl}/api/v1/webhooks/ingest/{endpoint.EndpointKey}",
        IsActive = endpoint.IsActive,
        SourceType = endpoint.SourceType,
        DefaultServiceId = endpoint.DefaultServiceId,
        DefaultSeverity = endpoint.DefaultSeverity,
        TitleMapping = endpoint.TitleMapping,
        DescriptionMapping = endpoint.DescriptionMapping,
        SeverityMapping = endpoint.SeverityMapping,
        LastReceivedAtUtc = endpoint.LastReceivedAtUtc,
        TotalReceived = endpoint.TotalReceived,
        CreatedAtUtc = endpoint.CreatedAtUtc,
        CreatedBy = endpoint.CreatedBy,
        UpdatedAtUtc = endpoint.UpdatedAtUtc,
        UpdatedBy = endpoint.UpdatedBy
    };
}
