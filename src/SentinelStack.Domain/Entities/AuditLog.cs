using SentinelStack.Domain.Common;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Immutable audit log entry for HIPAA compliance.
/// Audit logs are append-only and cannot be modified or deleted.
/// </summary>
public class AuditLog : BaseEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this audit log belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// The type of action that was performed.
    /// </summary>
    public AuditAction Action { get; private set; }

    /// <summary>
    /// The type of entity that was affected (e.g., "Incident", "User").
    /// </summary>
    public string EntityType { get; private set; } = string.Empty;

    /// <summary>
    /// The ID of the entity that was affected.
    /// </summary>
    public Guid? EntityId { get; private set; }

    /// <summary>
    /// The ID of the user who performed the action.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The email of the user who performed the action (denormalized for audit retention).
    /// </summary>
    public string? UserEmail { get; private set; }

    /// <summary>
    /// UTC timestamp when the action occurred.
    /// </summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>
    /// IP address from which the action was performed.
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// User agent string from the request.
    /// </summary>
    public string? UserAgent { get; private set; }

    /// <summary>
    /// JSON representation of the entity before the change (for updates/deletes).
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// JSON representation of the entity after the change (for creates/updates).
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// Additional context or notes about the action.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Correlation ID for tracing related operations.
    /// </summary>
    public string? CorrelationId { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private AuditLog() { }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public AuditLog(
        Guid tenantId,
        AuditAction action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string? userEmail = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? oldValues = null,
        string? newValues = null,
        string? notes = null,
        string? correlationId = null)
    {
        TenantId = tenantId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
        UserEmail = userEmail;
        OccurredAtUtc = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OldValues = oldValues;
        NewValues = newValues;
        Notes = notes;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Creates an audit log for entity creation.
    /// </summary>
    public static AuditLog ForCreate<T>(
        Guid tenantId,
        Guid entityId,
        Guid userId,
        string? userEmail,
        string? newValues,
        string? ipAddress = null,
        string? correlationId = null) where T : class
    {
        return new AuditLog(
            tenantId,
            AuditAction.Created,
            typeof(T).Name,
            entityId,
            userId,
            userEmail,
            ipAddress,
            null,
            null,
            newValues,
            null,
            correlationId);
    }

    /// <summary>
    /// Creates an audit log for entity update.
    /// </summary>
    public static AuditLog ForUpdate<T>(
        Guid tenantId,
        Guid entityId,
        Guid userId,
        string? userEmail,
        string? oldValues,
        string? newValues,
        string? ipAddress = null,
        string? correlationId = null) where T : class
    {
        return new AuditLog(
            tenantId,
            AuditAction.Updated,
            typeof(T).Name,
            entityId,
            userId,
            userEmail,
            ipAddress,
            null,
            oldValues,
            newValues,
            null,
            correlationId);
    }

    /// <summary>
    /// Creates an audit log for user login.
    /// </summary>
    public static AuditLog ForLogin(
        Guid tenantId,
        Guid userId,
        string userEmail,
        string? ipAddress,
        string? userAgent,
        bool success)
    {
        return new AuditLog(
            tenantId,
            success ? AuditAction.Login : AuditAction.LoginFailed,
            nameof(User),
            userId,
            userId,
            userEmail,
            ipAddress,
            userAgent);
    }
}
