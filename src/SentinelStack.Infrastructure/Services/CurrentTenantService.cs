using Microsoft.AspNetCore.Http;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Provides access to the current tenant from the HTTP context.
/// </summary>
public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TenantId
    {
        get
        {
            // First try to get from JWT claims
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id");
            if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
            {
                return tenantId;
            }

            // Fallback to header (for API key auth scenarios)
            var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue) && Guid.TryParse(headerValue, out var headerTenantId))
            {
                return headerTenantId;
            }

            return null;
        }
    }

    public string? TenantConnectionString
    {
        get
        {
            // For Alpha, all tenants use shared schema
            // Schema-per-tenant will be implemented in Beta for healthcare orgs
            return null;
        }
    }

    public bool IsIsolatedTenant
    {
        get
        {
            // For Alpha, no isolated tenants
            // Check for healthcare flag in tenant metadata in Beta
            return false;
        }
    }
}
