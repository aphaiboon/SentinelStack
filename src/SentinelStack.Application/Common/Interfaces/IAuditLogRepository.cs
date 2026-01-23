using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Repository interface for AuditLog entities.
/// Note: Audit logs are immutable - no update or delete operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds a new audit log entry.
    /// </summary>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an audit log by ID.
    /// </summary>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all audit logs for the current tenant.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by entity type and ID.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by user ID.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by action type.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(AuditAction action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs within a date range.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
