using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.AuditLogs.DTOs;

/// <summary>
/// Data transfer object for AuditLog entity.
/// </summary>
public class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public AuditAction Action { get; init; }
    public string ActionName => Action.ToString();
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public Guid? UserId { get; init; }
    public string? UserEmail { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? Notes { get; init; }
    public string? CorrelationId { get; init; }
}
