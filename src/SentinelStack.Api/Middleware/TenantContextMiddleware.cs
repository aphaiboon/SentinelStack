using System.Security.Claims;
using System.Text.Json;

namespace SentinelStack.Api.Middleware;

/// <summary>
/// Validates tenant context from X-Tenant-Id header against JWT claims.
/// Implements ADR-007: Tenant Context via X-Tenant-Id Header.
/// </summary>
public class TenantContextMiddleware : IMiddleware
{
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(ILogger<TenantContextMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip tenant validation for public endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var skipPaths = new[]
        {
            "/health",
            "/api/v1/auth/login",
            "/api/v1/auth/register",
            "/api/v1/auth/refresh",
            "/swagger",
            "/hangfire"
        };

        if (skipPaths.Any(skipPath => path.StartsWith(skipPath)))
        {
            await next(context);
            return;
        }

        // AC #3: Check if X-Tenant-Id header is present
        if (!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "X-Tenant-Id header is required"
            });
            return;
        }

        // Parse tenant ID from header
        if (!Guid.TryParse(tenantIdHeader, out var requestedTenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "X-Tenant-Id header must be a valid GUID"
            });
            return;
        }

        // AC #1, #4: Validate against JWT tenantIds claim
        var tenantIdsClaim = context.User.FindFirst("tenantIds")?.Value;
        if (string.IsNullOrEmpty(tenantIdsClaim))
        {
            _logger.LogWarning("User {UserId} has no tenantIds claim in JWT",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = "User does not have access to any tenants"
            });
            return;
        }

        // Parse tenantIds claim (JSON array: ["guid1", "guid2"])
        List<Guid> allowedTenantIds;
        try
        {
            allowedTenantIds = JsonSerializer
                .Deserialize<List<string>>(tenantIdsClaim)
                ?.Select(id => Guid.Parse(id))
                .ToList() ?? new List<Guid>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse tenantIds claim for user {UserId}",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Failed to process tenant claims"
            });
            return;
        }

        // Check if user has access to any tenants
        if (allowedTenantIds.Count == 0)
        {
            _logger.LogWarning("User {UserId} has empty tenantIds claim (no tenant access)",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = "User does not have access to any tenants"
            });
            return;
        }

        // AC #4: Verify requested tenant is in allowed list
        if (!allowedTenantIds.Contains(requestedTenantId))
        {
            _logger.LogWarning(
                "User {UserId} attempted access to unauthorized tenant {TenantId}. Allowed tenants: {AllowedTenants}",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                requestedTenantId,
                string.Join(", ", allowedTenantIds));

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = "User does not have access to this tenant"
            });
            return;
        }

        // AC #2: Store validated tenant ID for ICurrentTenantService
        context.Items["TenantId"] = requestedTenantId;

        _logger.LogDebug("Tenant context validated: {TenantId} for user {UserId}",
            requestedTenantId,
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

        await next(context);
    }
}
