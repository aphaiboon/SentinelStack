using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Provides access to the current user from the HTTP context.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public IReadOnlyList<string> Roles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return roles ?? new List<string>();
        }
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool HasPermission(string permission)
    {
        // For Alpha, permissions are derived from roles
        // This can be extended to check specific permission claims
        return _httpContextAccessor.HttpContext?.User?
            .HasClaim("permission", permission) ?? false;
    }
}
