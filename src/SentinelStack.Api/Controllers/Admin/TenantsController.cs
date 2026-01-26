using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Tenants.Commands;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Application.Tenants.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers.Admin;

/// <summary>
/// Request DTO for creating a tenant.
/// </summary>
public record CreateTenantRequest
{
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public TenantTier Tier { get; init; }
}

/// <summary>
/// Admin endpoint for tenant management (FR67, FR68).
/// Platform administrators only.
/// </summary>
[ApiController]
[Route("admin/tenants")]
[Authorize] // Future: Add [Authorize(Policy = "PlatformAdmin")] when RBAC is implemented
public class TenantsController : ControllerBase
{
    private readonly CreateTenantCommand _createCommand;
    private readonly GetTenantsQuery _getTenantsQuery;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        CreateTenantCommand createCommand,
        GetTenantsQuery getTenantsQuery,
        ILogger<TenantsController> logger)
    {
        _createCommand = createCommand;
        _getTenantsQuery = getTenantsQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new tenant organization (AC #1).
    /// </summary>
    /// <remarks>
    /// AC #3, #4: Isolation model auto-set based on tier.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantRequest request)
    {
        // Map request to command
        _createCommand.Name = request.Name;
        _createCommand.Subdomain = request.Subdomain;
        _createCommand.Tier = request.Tier;

        try
        {
            var tenant = await _createCommand.ExecuteAsync();

            _logger.LogInformation(
                "Tenant {TenantId} created with subdomain {Subdomain} and tier {Tier}",
                tenant.Id, tenant.Subdomain, tenant.Tier);

            return CreatedAtAction(nameof(GetTenant), new { id = tenant.Id }, tenant);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid tenant data: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all tenants with pagination (AC #5).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetTenants(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        _getTenantsQuery.Page = page;
        _getTenantsQuery.PageSize = pageSize;

        var (items, totalCount) = await _getTenantsQuery.ExecuteAsync();

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize
        });
    }

    /// <summary>
    /// Gets tenant by ID (placeholder for CreatedAtAction route).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TenantDto> GetTenant(Guid id)
    {
        // Placeholder for now - will be implemented in future stories
        return NotFound();
    }
}
