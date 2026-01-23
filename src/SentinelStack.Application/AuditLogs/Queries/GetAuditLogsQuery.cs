using SentinelStack.Application.AuditLogs.DTOs;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.AuditLogs.Queries;

/// <summary>
/// Request model for filtering audit logs.
/// </summary>
public class GetAuditLogsRequest
{
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public Guid? UserId { get; init; }
    public AuditAction? Action { get; init; }
    public DateTime? FromUtc { get; init; }
    public DateTime? ToUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

/// <summary>
/// Response model for paginated audit logs.
/// </summary>
public class GetAuditLogsResponse
{
    public required IReadOnlyList<AuditLogDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get audit logs with filtering and pagination.
/// </summary>
public class GetAuditLogsQuery
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQuery(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<GetAuditLogsResponse> ExecuteAsync(GetAuditLogsRequest request, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AuditLog> auditLogs;

        // Apply specific filters based on request
        if (request.EntityId.HasValue && !string.IsNullOrEmpty(request.EntityType))
        {
            auditLogs = await _auditLogRepository.GetByEntityAsync(request.EntityType, request.EntityId.Value, cancellationToken);
        }
        else if (request.UserId.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByUserAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.Action.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByActionAsync(request.Action.Value, cancellationToken);
        }
        else if (request.FromUtc.HasValue && request.ToUtc.HasValue)
        {
            auditLogs = await _auditLogRepository.GetByDateRangeAsync(request.FromUtc.Value, request.ToUtc.Value, cancellationToken);
        }
        else
        {
            auditLogs = await _auditLogRepository.GetAllAsync(cancellationToken);
        }

        // Apply additional filters
        var filtered = auditLogs.AsEnumerable();

        if (!string.IsNullOrEmpty(request.EntityType) && !request.EntityId.HasValue)
        {
            filtered = filtered.Where(l => l.EntityType == request.EntityType);
        }

        if (request.FromUtc.HasValue && (!request.ToUtc.HasValue || request.UserId.HasValue || request.Action.HasValue || !string.IsNullOrEmpty(request.EntityType)))
        {
            filtered = filtered.Where(l => l.OccurredAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue && (!request.FromUtc.HasValue || request.UserId.HasValue || request.Action.HasValue || !string.IsNullOrEmpty(request.EntityType)))
        {
            filtered = filtered.Where(l => l.OccurredAtUtc <= request.ToUtc.Value);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination (most recent first)
        var paged = filteredList
            .OrderByDescending(l => l.OccurredAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetAuditLogsResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
