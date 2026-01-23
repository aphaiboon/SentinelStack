using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Webhooks.Commands;
using SentinelStack.Application.Webhooks.DTOs;
using SentinelStack.Application.Webhooks.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Manages webhook endpoints and processes incoming webhooks.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly CreateWebhookEndpointCommand _createCommand;
    private readonly UpdateWebhookEndpointCommand _updateCommand;
    private readonly ProcessWebhookCommand _processCommand;
    private readonly GetWebhookEndpointQuery _getQuery;
    private readonly GetWebhookEndpointsQuery _listQuery;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        CreateWebhookEndpointCommand createCommand,
        UpdateWebhookEndpointCommand updateCommand,
        ProcessWebhookCommand processCommand,
        GetWebhookEndpointQuery getQuery,
        GetWebhookEndpointsQuery listQuery,
        ILogger<WebhooksController> logger)
    {
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _processCommand = processCommand;
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new webhook endpoint.
    /// Returns the endpoint with its secret key (only shown once).
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(WebhookEndpointWithSecretDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WebhookEndpointWithSecretDto>> CreateEndpoint(
        [FromBody] CreateWebhookEndpointRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoint = await _createCommand.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Webhook endpoint {EndpointId} created: {EndpointKey}", endpoint.Id, endpoint.EndpointKey);

            return CreatedAtAction(nameof(GetEndpoint), new { id = endpoint.Id }, endpoint);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a webhook endpoint by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(WebhookEndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookEndpointDto>> GetEndpoint(
        Guid id,
        CancellationToken cancellationToken)
    {
        var endpoint = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (endpoint == null)
        {
            return NotFound(new { message = "Webhook endpoint not found" });
        }

        return Ok(endpoint);
    }

    /// <summary>
    /// Lists webhook endpoints with optional filtering.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(GetWebhookEndpointsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetWebhookEndpointsResponse>> ListEndpoints(
        [FromQuery] bool? isActive,
        [FromQuery] WebhookSourceType? sourceType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetWebhookEndpointsRequest
        {
            IsActive = isActive,
            SourceType = sourceType,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing webhook endpoint.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(WebhookEndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookEndpointDto>> UpdateEndpoint(
        Guid id,
        [FromBody] UpdateWebhookEndpointRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        if (endpoint == null)
        {
            return NotFound(new { message = "Webhook endpoint not found" });
        }

        _logger.LogInformation("Webhook endpoint {EndpointId} updated", id);

        return Ok(endpoint);
    }

    /// <summary>
    /// Receives and processes an incoming webhook.
    /// This endpoint is public and validates using the endpoint's secret key.
    /// </summary>
    [HttpPost("ingest/{endpointKey}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WebhookProcessingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<WebhookProcessingResult>> IngestWebhook(
        string endpointKey,
        CancellationToken cancellationToken)
    {
        // Read the raw body
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(payload))
        {
            return BadRequest(new WebhookProcessingResult
            {
                Success = false,
                Error = "Empty payload"
            });
        }

        // Get signature from headers (supports multiple formats)
        var signature = Request.Headers["X-Signature"].FirstOrDefault()
            ?? Request.Headers["X-Hub-Signature-256"].FirstOrDefault()
            ?? Request.Headers["X-Webhook-Signature"].FirstOrDefault();

        _logger.LogDebug("Received webhook for endpoint {EndpointKey}, payload size: {Size} bytes",
            endpointKey, payload.Length);

        var result = await _processCommand.ExecuteAsync(endpointKey, payload, signature, cancellationToken);

        if (!result.Success && result.Error == "Invalid signature")
        {
            return Unauthorized(result);
        }

        if (!result.Success && result.Error == "Unknown endpoint")
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Activates a webhook endpoint.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize]
    [ProducesResponseType(typeof(WebhookEndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookEndpointDto>> ActivateEndpoint(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Webhook endpoint not found" });
        }

        var request = new UpdateWebhookEndpointRequest
        {
            Name = existing.Name,
            Description = existing.Description,
            SourceType = existing.SourceType,
            DefaultServiceId = existing.DefaultServiceId,
            DefaultSeverity = existing.DefaultSeverity,
            IsActive = true
        };

        var endpoint = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        _logger.LogInformation("Webhook endpoint {EndpointId} activated", id);

        return Ok(endpoint);
    }

    /// <summary>
    /// Deactivates a webhook endpoint.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(WebhookEndpointDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WebhookEndpointDto>> DeactivateEndpoint(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Webhook endpoint not found" });
        }

        var request = new UpdateWebhookEndpointRequest
        {
            Name = existing.Name,
            Description = existing.Description,
            SourceType = existing.SourceType,
            DefaultServiceId = existing.DefaultServiceId,
            DefaultSeverity = existing.DefaultSeverity,
            IsActive = false
        };

        var endpoint = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        _logger.LogInformation("Webhook endpoint {EndpointId} deactivated", id);

        return Ok(endpoint);
    }
}
