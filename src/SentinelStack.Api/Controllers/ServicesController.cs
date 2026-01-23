using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Services.Commands;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Application.Services.Queries;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Manages service registry operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly CreateServiceCommand _createCommand;
    private readonly UpdateServiceCommand _updateCommand;
    private readonly GetServiceQuery _getQuery;
    private readonly GetServicesQuery _listQuery;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(
        CreateServiceCommand createCommand,
        UpdateServiceCommand updateCommand,
        GetServiceQuery getQuery,
        GetServicesQuery listQuery,
        ILogger<ServicesController> logger)
    {
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new service.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ServiceDto>> CreateService(
        [FromBody] CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _createCommand.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Service {ServiceId} created with key {ServiceKey}", service.Id, service.ServiceKey);

            return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a service by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> GetService(
        Guid id,
        CancellationToken cancellationToken)
    {
        var service = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }

        return Ok(service);
    }

    /// <summary>
    /// Gets a service by its unique key.
    /// </summary>
    [HttpGet("by-key/{serviceKey}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> GetServiceByKey(
        string serviceKey,
        CancellationToken cancellationToken)
    {
        var service = await _getQuery.ExecuteByKeyAsync(serviceKey, cancellationToken);

        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }

        return Ok(service);
    }

    /// <summary>
    /// Lists services with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetServicesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetServicesResponse>> ListServices(
        [FromQuery] bool? isActive,
        [FromQuery] string? ownerTeam,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetServicesRequest
        {
            IsActive = isActive,
            OwnerTeam = ownerTeam,
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Cap at 100 per page
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing service.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> UpdateService(
        Guid id,
        [FromBody] UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }

        _logger.LogInformation("Service {ServiceId} updated", id);

        return Ok(service);
    }

    /// <summary>
    /// Deactivates a service.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> DeactivateService(
        Guid id,
        CancellationToken cancellationToken)
    {
        var service = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }

        var updateRequest = new UpdateServiceRequest
        {
            Name = service.Name,
            Description = service.Description,
            OwnerTeam = service.OwnerTeam,
            ContactEmail = service.ContactEmail,
            SlackChannel = service.SlackChannel,
            IsActive = false
        };

        var updated = await _updateCommand.ExecuteAsync(id, updateRequest, cancellationToken);

        _logger.LogInformation("Service {ServiceId} deactivated", id);

        return Ok(updated);
    }

    /// <summary>
    /// Activates a service.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceDto>> ActivateService(
        Guid id,
        CancellationToken cancellationToken)
    {
        var service = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }

        var updateRequest = new UpdateServiceRequest
        {
            Name = service.Name,
            Description = service.Description,
            OwnerTeam = service.OwnerTeam,
            ContactEmail = service.ContactEmail,
            SlackChannel = service.SlackChannel,
            IsActive = true
        };

        var updated = await _updateCommand.ExecuteAsync(id, updateRequest, cancellationToken);

        _logger.LogInformation("Service {ServiceId} activated", id);

        return Ok(updated);
    }
}
