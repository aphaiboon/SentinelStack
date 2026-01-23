using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.AuditLogs.DTOs;
using SentinelStack.Application.AuditLogs.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Provides read-only access to audit logs for HIPAA compliance.
/// Audit logs are immutable and cannot be created, updated, or deleted via API.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly GetAuditLogQuery _getQuery;
    private readonly GetAuditLogsQuery _listQuery;
    private readonly ILogger<AuditLogsController> _logger;

    public AuditLogsController(
        GetAuditLogQuery getQuery,
        GetAuditLogsQuery listQuery,
        ILogger<AuditLogsController> logger)
    {
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Gets an audit log entry by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogDto>> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var auditLog = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (auditLog == null)
        {
            return NotFound(new { message = "Audit log not found" });
        }

        return Ok(auditLog);
    }

    /// <summary>
    /// Lists audit logs with optional filtering.
    /// Results are ordered by most recent first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuditLogsResponse>> ListAuditLogs(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        [FromQuery] Guid? userId,
        [FromQuery] AuditAction? action,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new GetAuditLogsRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            Action = action,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Cap at 100 per page
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets audit logs for a specific entity.
    /// Useful for viewing the complete history of changes to an entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(GetAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetEntityAuditTrail(
        string entityType,
        Guid entityId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new GetAuditLogsRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets audit logs for a specific user's actions.
    /// Useful for reviewing all actions taken by a particular user.
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(GetAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetUserAuditTrail(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var request = new GetAuditLogsRequest
        {
            UserId = userId,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Gets login activity audit logs.
    /// </summary>
    [HttpGet("logins")]
    [ProducesResponseType(typeof(GetAuditLogsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetAuditLogsResponse>> GetLoginActivity(
        [FromQuery] bool includeFailures = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        // Get successful logins
        var loginRequest = new GetAuditLogsRequest
        {
            Action = AuditAction.Login,
            Page = page,
            PageSize = includeFailures ? pageSize / 2 : pageSize
        };

        var response = await _listQuery.ExecuteAsync(loginRequest, cancellationToken);

        if (includeFailures)
        {
            var failedLoginRequest = new GetAuditLogsRequest
            {
                Action = AuditAction.LoginFailed,
                Page = page,
                PageSize = pageSize / 2
            };

            var failedResponse = await _listQuery.ExecuteAsync(failedLoginRequest, cancellationToken);

            // Merge and re-sort results
            var combined = response.Items
                .Concat(failedResponse.Items)
                .OrderByDescending(l => l.OccurredAtUtc)
                .Take(pageSize)
                .ToList();

            return Ok(new GetAuditLogsResponse
            {
                Items = combined,
                TotalCount = response.TotalCount + failedResponse.TotalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        return Ok(response);
    }
}
