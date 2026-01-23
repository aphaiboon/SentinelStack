using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Common.Settings;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Webhooks.Commands;

/// <summary>
/// Command to update an existing webhook endpoint.
/// </summary>
public class UpdateWebhookEndpointCommand
{
    private readonly IWebhookEndpointRepository _webhookRepository;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _baseUrl;

    public UpdateWebhookEndpointCommand(
        IWebhookEndpointRepository webhookRepository,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork,
        IOptions<ApplicationSettings> settings)
    {
        _webhookRepository = webhookRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _baseUrl = settings.Value.BaseUrl;
    }

    public async Task<WebhookEndpointDto?> ExecuteAsync(Guid id, UpdateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        var endpoint = await _webhookRepository.GetByIdAsync(id, cancellationToken);
        if (endpoint == null)
        {
            return null;
        }

        endpoint.Update(
            request.Name,
            request.Description,
            request.SourceType,
            request.DefaultServiceId,
            request.DefaultSeverity);

        endpoint.SetMappings(request.TitleMapping, request.DescriptionMapping, request.SeverityMapping);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                endpoint.Activate();
            }
            else
            {
                endpoint.Deactivate();
            }
        }

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            endpoint.SetUpdatedBy(userId.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(endpoint);
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
