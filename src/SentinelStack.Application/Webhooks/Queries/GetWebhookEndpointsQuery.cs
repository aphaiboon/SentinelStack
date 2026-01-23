using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Common.Settings;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Webhooks.Queries;

/// <summary>
/// Request model for filtering webhook endpoints.
/// </summary>
public class GetWebhookEndpointsRequest
{
    public bool? IsActive { get; init; }
    public WebhookSourceType? SourceType { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response model for paginated webhook endpoints.
/// </summary>
public class GetWebhookEndpointsResponse
{
    public required IReadOnlyList<WebhookEndpointDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get webhook endpoints with filtering and pagination.
/// </summary>
public class GetWebhookEndpointsQuery
{
    private readonly IWebhookEndpointRepository _webhookRepository;
    private readonly string _baseUrl;

    public GetWebhookEndpointsQuery(
        IWebhookEndpointRepository webhookRepository,
        IOptions<ApplicationSettings> settings)
    {
        _webhookRepository = webhookRepository;
        _baseUrl = settings.Value.BaseUrl;
    }

    public async Task<GetWebhookEndpointsResponse> ExecuteAsync(GetWebhookEndpointsRequest request, CancellationToken cancellationToken = default)
    {
        var endpoints = request.IsActive == true
            ? await _webhookRepository.GetActiveEndpointsAsync(cancellationToken)
            : await _webhookRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = endpoints.AsEnumerable();

        if (request.IsActive.HasValue && request.IsActive == false)
        {
            filtered = filtered.Where(e => !e.IsActive);
        }

        if (request.SourceType.HasValue)
        {
            filtered = filtered.Where(e => e.SourceType == request.SourceType.Value);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var paged = filteredList
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetWebhookEndpointsResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
