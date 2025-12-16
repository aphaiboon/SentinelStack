using System;

namespace SentinelStack.Domain.Entities;
{
    /// <summary>
    /// Represents a real operational incident in the system, not a synthetic alert.
    /// Tracks status and changes for healthcare-compliant audit trails.
    /// </summary>
    public class Incident
{
    /// <summary>
    /// Unique identifier for the incident.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Title of the incident.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Detailed description of the incident.
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// Severity level (e.g., "critical", "major", "minor") of the incident.
    /// </summary>
    public string Severity { get; private set; }

    /// <summary>
    /// Current status of the incident (e.g., "open", "in_progress", "resolved").
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// UTC timestamp for when the incident was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp for when the incident was resolved. Defaults to DateTime.MinValue.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp for when the incident was last updated.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp for when the incident was soft-deleted.
    /// </summary>
    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// UTC timestamp for when the incident was archived.
    /// </summary>
    public DateTime? ArchivedAtUtc { get; private set; }

    /// <summary>
    /// Public constructor for creating a new incident instance.
    /// </summary>
    /// <param name="title">The title of the incident.</param>
    /// <param name="description">The description of the incident.</param>
    /// <param name="severity">The severity of the incident.</param>
    /// <param name="status">The initial status of the incident.</param>
    public Incident(string title, string description, string severity)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Severity = severity;
        Status = "Open";
        CreatedAtUtc = DateTime.UtcNow;
        ResolvedAtUtc = DateTime.MinValue;
    }

    /// <summary>
    /// Updates the status of the incident, setting the updated timestamp accordingly.
    /// </summary>
    /// <param name="status">The new status of the incident.</param>
    public void UpdateStatus(string status)
    {
        Status = status;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the description of the incident, setting the updated timestamp accordingly.
    /// </summary>
    /// <param name="description">The new description of the incident.</param>
    public void UpdateDescription(string description)
    {
        Description = description;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the incident as resolved and sets the resolved UTC timestamp.
    /// </summary>
    public void Resolve()
    {
        Status = "resolved";
        ResolvedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the incident as archived and sets the archived UTC timestamp.
    /// </summary>
    public void Archive()
    {
        ArchivedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the incident as deleted and sets the deleted UTC timestamp.
    /// </summary>
    public void Delete()
    {
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
}