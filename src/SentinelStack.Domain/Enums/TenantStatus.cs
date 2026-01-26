namespace SentinelStack.Domain.Enums;

/// <summary>
/// Tenant account status.
/// </summary>
public enum TenantStatus
{
    /// <summary>
    /// Tenant is active and can access the platform
    /// </summary>
    Active = 0,

    /// <summary>
    /// Tenant is suspended (payment issues, policy violations)
    /// </summary>
    Suspended = 1,

    /// <summary>
    /// Tenant is soft-deleted (data retained for GDPR/recovery)
    /// </summary>
    Deleted = 2
}
