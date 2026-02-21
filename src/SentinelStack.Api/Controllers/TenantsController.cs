using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Application.Tenants.Queries;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Public tenant resolution endpoint (AC #2).
/// Any authenticated user can resolve a subdomain to a tenant.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly GetTenantBySubdomainQuery _getBySubdomainQuery;

    public TenantsController(GetTenantBySubdomainQuery getBySubdomainQuery)
    {
        _getBySubdomainQuery = getBySubdomainQuery;
    }

    /// <summary>
    /// Resolves a subdomain to a tenant (AC #2: unique subdomain per tenant).
    /// Returns tenant info if found, 404 otherwise.
    /// </summary>
    [HttpGet("resolve/{subdomain}")]
    [ProducesResponseType(typeof(TenantInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantInfoDto>> ResolveSubdomain(string subdomain)
    {
        _getBySubdomainQuery.Subdomain = subdomain;

        var tenant = await _getBySubdomainQuery.ExecuteAsync();

        if (tenant == null)
        {
            return NotFound(new { error = $"No tenant found for subdomain '{subdomain}'" });
        }

        return Ok(tenant);
    }
}
