using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service interface for creating audit log entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an entity creation.
    /// </summary>
    Task LogCreateAsync<TEntity>(Guid entityId, TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Logs an entity update with before/after values.
    /// </summary>
    Task LogUpdateAsync<TEntity>(Guid entityId, TEntity? oldEntity, TEntity newEntity, CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Logs an entity deletion.
    /// </summary>
    Task LogDeleteAsync<TEntity>(Guid entityId, TEntity entity, CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Logs an entity access (view/read).
    /// </summary>
    Task LogAccessAsync<TEntity>(Guid entityId, CancellationToken cancellationToken = default)
        where TEntity : class;

    /// <summary>
    /// Logs a custom audit action.
    /// </summary>
    Task LogActionAsync(
        AuditAction action,
        string entityType,
        Guid? entityId,
        string? notes = null,
        string? oldValues = null,
        string? newValues = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a user login attempt.
    /// </summary>
    Task LogLoginAsync(Guid userId, string email, bool success, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a user logout.
    /// </summary>
    Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default);
}
