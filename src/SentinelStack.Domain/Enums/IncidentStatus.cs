namespace SentinelStack.Domain.Enums;

/// <summary>
/// Status of an incident through its lifecycle.
/// </summary>
public enum IncidentStatus
{
    /// <summary>
    /// Incident has been created but not yet acknowledged.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Incident has been acknowledged by a responder.
    /// </summary>
    Acknowledged = 1,

    /// <summary>
    /// Incident is actively being investigated.
    /// </summary>
    Investigating = 2,

    /// <summary>
    /// Incident has been resolved.
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Incident has been closed after resolution.
    /// </summary>
    Closed = 4
}
