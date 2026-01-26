using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Tenants.DTOs;

/// <summary>
/// Data transfer object for Tenant entity.
/// </summary>
public record TenantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public TenantTier Tier { get; init; }
    public IsolationModel IsolationModel { get; init; }
    public TenantStatus Status { get; init; }
    public string? SchemaName { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? SuspendedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

/// <summary>
/// Public tenant information for subdomain resolution (AC #2).
/// Does not include sensitive data like connection strings.
/// </summary>
public record TenantInfoDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
}
