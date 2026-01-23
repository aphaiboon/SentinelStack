using SentinelStack.Application.AuditLogs.DTOs;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.AuditLogs.Queries;

/// <summary>
/// Query to get a single audit log by ID.
/// </summary>
public class GetAuditLogQuery
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogQuery(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<AuditLogDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var auditLog = await _auditLogRepository.GetByIdAsync(id, cancellationToken);
        return auditLog == null ? null : MapToDto(auditLog);
    }

    private static AuditLogDto MapToDto(AuditLog log) => new()
    {
        Id = log.Id,
        TenantId = log.TenantId,
        Action = log.Action,
        EntityType = log.EntityType,
        EntityId = log.EntityId,
        UserId = log.UserId,
        UserEmail = log.UserEmail,
        OccurredAtUtc = log.OccurredAtUtc,
        IpAddress = log.IpAddress,
        UserAgent = log.UserAgent,
        OldValues = log.OldValues,
        NewValues = log.NewValues,
        Notes = log.Notes,
        CorrelationId = log.CorrelationId
    };
}
