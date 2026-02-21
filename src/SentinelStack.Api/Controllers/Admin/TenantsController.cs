using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Tenants.Commands;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Application.Tenants.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers.Admin;

/// <summary>
/// Admin endpoint for tenant management (FR67, FR68).
/// Platform administrators only.
/// </summary>
[ApiController]
[Route("admin/tenants")]
[Authorize(Roles = "PlatformAdmin,Admin")]
public class TenantsController : ControllerBase
{
    private readonly CreateTenantCommand _createCommand;
    private readonly GetTenantsQuery _getTenantsQuery;
    private readonly GetTenantBySubdomainQuery _getBySubdomainQuery;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        CreateTenantCommand createCommand,
        GetTenantsQuery getTenantsQuery,
        GetTenantBySubdomainQuery getBySubdomainQuery,
        ITenantRepository tenantRepository,
        ILogger<TenantsController> logger)
    {
        _createCommand = createCommand;
        _getTenantsQuery = getTenantsQuery;
        _getBySubdomainQuery = getBySubdomainQuery;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new tenant organization (AC #1).
    /// AC #3, #4: Isolation model auto-set based on tier.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> CreateTenant([FromBody] CreateTenantRequest request)
    {
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
    /// Gets tenant by ID (Task 5: GET /admin/tenants/{id}).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetTenant(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with ID {id} not found" });
        }

        return Ok(new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Tier = tenant.Tier,
            IsolationModel = tenant.IsolationModel,
            Status = tenant.Status,
            SchemaName = tenant.SchemaName,
            CreatedAtUtc = tenant.CreatedAtUtc,
            SuspendedAtUtc = tenant.SuspendedAtUtc,
            UpdatedAtUtc = tenant.UpdatedAtUtc
        });
    }

    /// <summary>
    /// Updates tenant name (Task 5: PUT /admin/tenants/{id}).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantDto>> UpdateTenant(Guid id, [FromBody] UpdateTenantRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);

        if (tenant == null)
        {
            return NotFound(new { error = $"Tenant with ID {id} not found" });
        }

        try
        {
            tenant.UpdateName(request.Name);
            await _tenantRepository.UpdateAsync(tenant);

            _logger.LogInformation("Tenant {TenantId} updated", id);

            return Ok(new TenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Subdomain = tenant.Subdomain,
                Tier = tenant.Tier,
                IsolationModel = tenant.IsolationModel,
                Status = tenant.Status,
                SchemaName = tenant.SchemaName,
                CreatedAtUtc = tenant.CreatedAtUtc,
                SuspendedAtUtc = tenant.SuspendedAtUtc,
                UpdatedAtUtc = tenant.UpdatedAtUtc
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Suspends a tenant account (FR68).
    /// </summary>
    [HttpPost("{id}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendTenant(Guid id)
    {
        try
        {
            await _tenantRepository.SuspendAsync(id);
            _logger.LogInformation("Tenant {TenantId} suspended", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Activates a suspended tenant account.
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateTenant(Guid id)
    {
        try
        {
            await _tenantRepository.ActivateAsync(id);
            _logger.LogInformation("Tenant {TenantId} activated", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
