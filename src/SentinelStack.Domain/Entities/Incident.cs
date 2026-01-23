using SentinelStack.Domain.Common;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a real operational incident in the system.
/// Tracks status and changes for healthcare-compliant audit trails.
/// </summary>
public class Incident : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this incident belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Title of the incident.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of the incident.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Severity level of the incident.
    /// </summary>
    public IncidentSeverity Severity { get; private set; }

    /// <summary>
    /// Current status of the incident.
    /// </summary>
    public IncidentStatus Status { get; private set; }

    /// <summary>
    /// The service affected by this incident.
    /// </summary>
    public Guid? ServiceId { get; private set; }

    /// <summary>
    /// The user assigned to handle this incident.
    /// </summary>
    public Guid? AssignedToId { get; private set; }

    /// <summary>
    /// The user who acknowledged this incident.
    /// </summary>
    public Guid? AcknowledgedBy { get; private set; }

    /// <summary>
    /// UTC timestamp when the incident was acknowledged.
    /// </summary>
    public DateTime? AcknowledgedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the incident was resolved.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>
    /// The user who resolved this incident.
    /// </summary>
    public Guid? ResolvedBy { get; private set; }

    /// <summary>
    /// UTC timestamp when the incident was archived.
    /// </summary>
    public DateTime? ArchivedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp when the incident was soft-deleted.
    /// </summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Incident() { }

    /// <summary>
    /// Creates a new incident.
    /// </summary>
    public Incident(
        Guid tenantId,
        string title,
        IncidentSeverity severity,
        string? description = null,
        Guid? serviceId = null,
        Guid? assignedToId = null)
    {
        TenantId = tenantId;
        Title = title;
        Description = description ?? string.Empty;
        Severity = severity;
        ServiceId = serviceId;
        AssignedToId = assignedToId;
        Status = IncidentStatus.Open;
    }

    /// <summary>
    /// Acknowledges the incident.
    /// </summary>
    public void Acknowledge(Guid userId)
    {
        if (Status != IncidentStatus.Open)
        {
            return;
        }

        Status = IncidentStatus.Acknowledged;
        AcknowledgedBy = userId;
        AcknowledgedAtUtc = DateTime.UtcNow;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Starts investigation of the incident.
    /// </summary>
    public void StartInvestigation(Guid userId)
    {
        if (Status != IncidentStatus.Acknowledged)
        {
            return;
        }

        Status = IncidentStatus.Investigating;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Resolves the incident.
    /// </summary>
    public void Resolve(Guid userId)
    {
        if (Status == IncidentStatus.Resolved || Status == IncidentStatus.Closed)
        {
            return;
        }

        Status = IncidentStatus.Resolved;
        ResolvedBy = userId;
        ResolvedAtUtc = DateTime.UtcNow;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Closes the incident after resolution.
    /// </summary>
    public void Close(Guid userId)
    {
        if (Status != IncidentStatus.Resolved)
        {
            return;
        }

        Status = IncidentStatus.Closed;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Updates the incident title.
    /// </summary>
    public void UpdateTitle(string title, Guid userId)
    {
        Title = title;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Updates the incident description.
    /// </summary>
    public void UpdateDescription(string description, Guid userId)
    {
        Description = description;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Assigns the incident to a user.
    /// </summary>
    public void Assign(Guid? userId, Guid updatedByUserId)
    {
        AssignedToId = userId;
        SetUpdatedBy(updatedByUserId);
    }

    /// <summary>
    /// Updates the incident severity.
    /// </summary>
    public void UpdateSeverity(IncidentSeverity severity, Guid userId)
    {
        Severity = severity;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Archives the incident.
    /// </summary>
    public void Archive(Guid userId)
    {
        ArchivedAtUtc = DateTime.UtcNow;
        SetUpdatedBy(userId);
    }

    /// <summary>
    /// Soft-deletes the incident.
    /// </summary>
    public void Delete(Guid userId)
    {
        DeletedAtUtc = DateTime.UtcNow;
        SetUpdatedBy(userId);
    }
}
