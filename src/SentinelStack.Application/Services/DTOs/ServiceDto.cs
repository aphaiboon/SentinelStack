namespace SentinelStack.Application.Services.DTOs;

/// <summary>
/// Data transfer object for Service entity.
/// </summary>
public class ServiceDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Name { get; init; }
    public required string ServiceKey { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public string? OwnerTeam { get; init; }
    public string? ContactEmail { get; init; }
    public string? SlackChannel { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public Guid? UpdatedBy { get; init; }
}
