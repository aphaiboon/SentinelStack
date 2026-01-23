using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Escalations.Commands;
using SentinelStack.Application.Escalations.DTOs;
using SentinelStack.Application.Escalations.Queries;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Manages escalation policies for incident notification workflows.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class EscalationPoliciesController : ControllerBase
{
    private readonly CreateEscalationPolicyCommand _createCommand;
    private readonly UpdateEscalationPolicyCommand _updateCommand;
    private readonly GetEscalationPolicyQuery _getQuery;
    private readonly GetEscalationPoliciesQuery _listQuery;
    private readonly ILogger<EscalationPoliciesController> _logger;

    public EscalationPoliciesController(
        CreateEscalationPolicyCommand createCommand,
        UpdateEscalationPolicyCommand updateCommand,
        GetEscalationPolicyQuery getQuery,
        GetEscalationPoliciesQuery listQuery,
        ILogger<EscalationPoliciesController> logger)
    {
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new escalation policy.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EscalationPolicyDto>> CreatePolicy(
        [FromBody] CreateEscalationPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _createCommand.ExecuteAsync(request, cancellationToken);

        _logger.LogInformation("Escalation policy {PolicyId} created: {PolicyName}", policy.Id, policy.Name);

        return CreatedAtAction(nameof(GetPolicy), new { id = policy.Id }, policy);
    }

    /// <summary>
    /// Gets an escalation policy by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> GetPolicy(
        Guid id,
        CancellationToken cancellationToken)
    {
        var policy = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (policy == null)
        {
            return NotFound(new { message = "Escalation policy not found" });
        }

        return Ok(policy);
    }

    /// <summary>
    /// Gets the default escalation policy.
    /// </summary>
    [HttpGet("default")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> GetDefaultPolicy(
        CancellationToken cancellationToken)
    {
        var policy = await _getQuery.GetDefaultAsync(cancellationToken);

        if (policy == null)
        {
            return NotFound(new { message = "No default escalation policy configured" });
        }

        return Ok(policy);
    }

    /// <summary>
    /// Gets the escalation policy for a specific service.
    /// </summary>
    [HttpGet("service/{serviceId:guid}")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> GetPolicyByService(
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var policy = await _getQuery.GetByServiceAsync(serviceId, cancellationToken);

        if (policy == null)
        {
            // Fall back to default
            policy = await _getQuery.GetDefaultAsync(cancellationToken);
        }

        if (policy == null)
        {
            return NotFound(new { message = "No escalation policy found for this service" });
        }

        return Ok(policy);
    }

    /// <summary>
    /// Lists escalation policies with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetEscalationPoliciesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetEscalationPoliciesResponse>> ListPolicies(
        [FromQuery] bool? isActive,
        [FromQuery] Guid? serviceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetEscalationPoliciesRequest
        {
            IsActive = isActive,
            ServiceId = serviceId,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing escalation policy.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> UpdatePolicy(
        Guid id,
        [FromBody] UpdateEscalationPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        if (policy == null)
        {
            return NotFound(new { message = "Escalation policy not found" });
        }

        _logger.LogInformation("Escalation policy {PolicyId} updated", id);

        return Ok(policy);
    }

    /// <summary>
    /// Activates an escalation policy.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> ActivatePolicy(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Escalation policy not found" });
        }

        var request = new UpdateEscalationPolicyRequest
        {
            Name = existing.Name,
            Description = existing.Description,
            IsActive = true
        };

        var policy = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        _logger.LogInformation("Escalation policy {PolicyId} activated", id);

        return Ok(policy);
    }

    /// <summary>
    /// Deactivates an escalation policy.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> DeactivatePolicy(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Escalation policy not found" });
        }

        var request = new UpdateEscalationPolicyRequest
        {
            Name = existing.Name,
            Description = existing.Description,
            IsActive = false
        };

        var policy = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        _logger.LogInformation("Escalation policy {PolicyId} deactivated", id);

        return Ok(policy);
    }

    /// <summary>
    /// Sets an escalation policy as the default.
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    [ProducesResponseType(typeof(EscalationPolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationPolicyDto>> SetAsDefault(
        Guid id,
        CancellationToken cancellationToken)
    {
        var existing = await _getQuery.ExecuteAsync(id, cancellationToken);
        if (existing == null)
        {
            return NotFound(new { message = "Escalation policy not found" });
        }

        var request = new UpdateEscalationPolicyRequest
        {
            Name = existing.Name,
            Description = existing.Description,
            IsDefault = true
        };

        var policy = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        _logger.LogInformation("Escalation policy {PolicyId} set as default", id);

        return Ok(policy);
    }
}
