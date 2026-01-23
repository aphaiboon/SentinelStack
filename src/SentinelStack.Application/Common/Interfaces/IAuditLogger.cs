using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Abstraction for HIPAA-compliant audit logging.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs an audit event for an entity operation.
    /// </summary>
    /// <param name="action">The type of action performed.</param>
    /// <param name="entityType">The type of entity affected.</param>
    /// <param name="entityId">The ID of the entity affected.</param>
    /// <param name="oldValues">JSON representation of the entity before the change (for updates/deletes).</param>
    /// <param name="newValues">JSON representation of the entity after the change (for creates/updates).</param>
    /// <param name="notes">Additional context or notes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogAsync(
        AuditAction action,
        string entityType,
        Guid? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an audit event for entity creation.
    /// </summary>
    /// <typeparam name="T">The type of entity created.</typeparam>
    /// <param name="entityId">The ID of the created entity.</param>
    /// <param name="newValues">JSON representation of the created entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogCreateAsync<T>(Guid entityId, string newValues, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs an audit event for entity update.
    /// </summary>
    /// <typeparam name="T">The type of entity updated.</typeparam>
    /// <param name="entityId">The ID of the updated entity.</param>
    /// <param name="oldValues">JSON representation of the entity before update.</param>
    /// <param name="newValues">JSON representation of the entity after update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogUpdateAsync<T>(Guid entityId, string oldValues, string newValues, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs an audit event for entity deletion.
    /// </summary>
    /// <typeparam name="T">The type of entity deleted.</typeparam>
    /// <param name="entityId">The ID of the deleted entity.</param>
    /// <param name="oldValues">JSON representation of the deleted entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogDeleteAsync<T>(Guid entityId, string oldValues, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs a user login event.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="userEmail">The email of the user.</param>
    /// <param name="success">Whether the login was successful.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogLoginAsync(Guid userId, string userEmail, bool success, CancellationToken cancellationToken = default);
}
