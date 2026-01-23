namespace SentinelStack.Domain.Enums;

/// <summary>
/// Roles available for users in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Read-only access to incidents and dashboards.
    /// </summary>
    Viewer = 0,

    /// <summary>
    /// Can respond to and update incidents.
    /// </summary>
    Responder = 1,

    /// <summary>
    /// Can manage incidents, services, and schedules.
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Full administrative access to tenant configuration.
    /// </summary>
    Admin = 3,

    /// <summary>
    /// Platform owner with access to all tenants (internal use only).
    /// </summary>
    PlatformAdmin = 99
}
