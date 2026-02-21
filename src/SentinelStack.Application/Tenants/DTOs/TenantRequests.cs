using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Tenants.DTOs;

/// <summary>
/// Request DTO for creating a tenant (AC #1).
/// </summary>
public record CreateTenantRequest
{
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public TenantTier Tier { get; init; }
}

/// <summary>
/// Request DTO for updating a tenant.
/// </summary>
public record UpdateTenantRequest
{
    public string Name { get; init; } = string.Empty;
}
