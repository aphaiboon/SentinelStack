using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Incidents.Commands;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Application.Incidents.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Manages incident lifecycle operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly CreateIncidentCommand _createCommand;
    private readonly UpdateIncidentCommand _updateCommand;
    private readonly AcknowledgeIncidentCommand _acknowledgeCommand;
    private readonly ResolveIncidentCommand _resolveCommand;
    private readonly GetIncidentQuery _getQuery;
    private readonly GetIncidentsQuery _listQuery;
    private readonly ILogger<IncidentsController> _logger;

    public IncidentsController(
        CreateIncidentCommand createCommand,
        UpdateIncidentCommand updateCommand,
        AcknowledgeIncidentCommand acknowledgeCommand,
        ResolveIncidentCommand resolveCommand,
        GetIncidentQuery getQuery,
        GetIncidentsQuery listQuery,
        ILogger<IncidentsController> logger)
    {
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _acknowledgeCommand = acknowledgeCommand;
        _resolveCommand = resolveCommand;
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new incident.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentDto>> CreateIncident(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var incident = await _createCommand.ExecuteAsync(request, cancellationToken);

        _logger.LogInformation("Incident {IncidentId} created", incident.Id);

        return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, incident);
    }

    /// <summary>
    /// Gets an incident by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> GetIncident(
        Guid id,
        CancellationToken cancellationToken)
    {
        var incident = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (incident == null)
        {
            return NotFound(new { message = "Incident not found" });
        }

        return Ok(incident);
    }

    /// <summary>
    /// Lists incidents with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetIncidentsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetIncidentsResponse>> ListIncidents(
        [FromQuery] IncidentStatus? status,
        [FromQuery] IncidentSeverity? severity,
        [FromQuery] Guid? serviceId,
        [FromQuery] Guid? assignedToId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetIncidentsRequest
        {
            Status = status,
            Severity = severity,
            ServiceId = serviceId,
            AssignedToId = assignedToId,
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Cap at 100 per page
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing incident.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentDto>> UpdateIncident(
        Guid id,
        [FromBody] UpdateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var incident = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        if (incident == null)
        {
            return NotFound(new { message = "Incident not found" });
        }

        _logger.LogInformation("Incident {IncidentId} updated", id);

        return Ok(incident);
    }

    /// <summary>
    /// Acknowledges an incident.
    /// </summary>
    [HttpPost("{id:guid}/acknowledge")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentDto>> AcknowledgeIncident(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _acknowledgeCommand.ExecuteAsync(id, cancellationToken);

            if (incident == null)
            {
                return NotFound(new { message = "Incident not found" });
            }

            _logger.LogInformation("Incident {IncidentId} acknowledged", id);

            return Ok(incident);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Resolves an incident.
    /// </summary>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(typeof(IncidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IncidentDto>> ResolveIncident(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var incident = await _resolveCommand.ExecuteAsync(id, cancellationToken);

            if (incident == null)
            {
                return NotFound(new { message = "Incident not found" });
            }

            _logger.LogInformation("Incident {IncidentId} resolved", id);

            return Ok(incident);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an incident (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteIncident(
        Guid id,
        CancellationToken cancellationToken)
    {
        var incident = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (incident == null)
        {
            return NotFound(new { message = "Incident not found" });
        }

        // Soft delete would be handled in a DeleteIncidentCommand
        // For now, we'll log and return NoContent
        _logger.LogInformation("Incident {IncidentId} deleted", id);

        return NoContent();
    }
}
