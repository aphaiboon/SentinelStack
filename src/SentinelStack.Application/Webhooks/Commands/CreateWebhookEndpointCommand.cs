using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Common.Settings;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Webhooks.Commands;

/// <summary>
/// Command to create a new webhook endpoint.
/// </summary>
public class CreateWebhookEndpointCommand
{
    private readonly IWebhookEndpointRepository _webhookRepository;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _baseUrl;

    public CreateWebhookEndpointCommand(
        IWebhookEndpointRepository webhookRepository,
        ICurrentTenantService tenantService,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork,
        IOptions<ApplicationSettings> settings)
    {
        _webhookRepository = webhookRepository;
        _tenantService = tenantService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _baseUrl = settings.Value.BaseUrl;
    }

    public async Task<WebhookEndpointWithSecretDto> ExecuteAsync(CreateWebhookEndpointRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        // Check if endpoint key already exists
        if (await _webhookRepository.EndpointKeyExistsAsync(request.EndpointKey, cancellationToken))
        {
            throw new InvalidOperationException($"Webhook endpoint with key '{request.EndpointKey}' already exists");
        }

        // Generate a secure secret key
        var secretKey = GenerateSecretKey();

        var endpoint = new WebhookEndpoint(
            tenantId,
            request.Name,
            request.EndpointKey,
            secretKey,
            request.SourceType,
            request.Description,
            request.DefaultServiceId,
            request.DefaultSeverity);

        // Set mappings if provided
        endpoint.SetMappings(request.TitleMapping, request.DescriptionMapping, request.SeverityMapping);

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            endpoint.SetCreatedBy(userId.Value);
        }

        await _webhookRepository.AddAsync(endpoint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDtoWithSecret(endpoint);
    }

    private static string GenerateSecretKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private WebhookEndpointWithSecretDto MapToDtoWithSecret(WebhookEndpoint endpoint) => new()
    {
        Id = endpoint.Id,
        TenantId = endpoint.TenantId,
        Name = endpoint.Name,
        Description = endpoint.Description,
        EndpointKey = endpoint.EndpointKey,
        WebhookUrl = $"{_baseUrl}/api/v1/webhooks/ingest/{endpoint.EndpointKey}",
        SecretKey = endpoint.SecretKey,
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
