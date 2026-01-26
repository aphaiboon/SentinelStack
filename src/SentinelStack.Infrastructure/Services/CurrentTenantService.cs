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
            // Read from validated context (populated by TenantContextMiddleware)
            // Middleware validates X-Tenant-Id header against JWT tenantIds claim
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("TenantId", out var tenantId) == true)
            {
                return (Guid?)tenantId;
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
