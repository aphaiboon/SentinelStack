namespace SentinelStack.Domain.Enums;

/// <summary>
/// Severity level of an incident.
/// </summary>
public enum IncidentSeverity
{
    /// <summary>
    /// Critical severity - major system outage affecting all users.
    /// </summary>
    Critical = 1,

    /// <summary>
    /// High severity - significant impact on multiple users or critical functionality.
    /// </summary>
    High = 2,

    /// <summary>
    /// Medium severity - moderate impact on some users or non-critical functionality.
    /// </summary>
    Medium = 3,

    /// <summary>
    /// Low severity - minor impact with workaround available.
    /// </summary>
    Low = 4,

    /// <summary>
    /// Informational - no immediate impact, for tracking purposes.
    /// </summary>
    Info = 5
}
