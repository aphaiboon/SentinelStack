namespace SentinelStack.Domain.Enums;

/// <summary>
/// Types of audit actions tracked for HIPAA compliance.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Entity was created.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Entity was updated.
    /// </summary>
    Updated = 1,

    /// <summary>
    /// Entity was deleted (soft delete).
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// Entity was viewed/accessed.
    /// </summary>
    Accessed = 3,

    /// <summary>
    /// User logged in.
    /// </summary>
    Login = 10,

    /// <summary>
    /// User logged out.
    /// </summary>
    Logout = 11,

    /// <summary>
    /// Failed login attempt.
    /// </summary>
    LoginFailed = 12,

    /// <summary>
    /// User password was changed.
    /// </summary>
    PasswordChanged = 13,

    /// <summary>
    /// User MFA was enabled/disabled.
    /// </summary>
    MfaChanged = 14,

    /// <summary>
    /// Data was exported.
    /// </summary>
    Exported = 20,

    /// <summary>
    /// Configuration was changed.
    /// </summary>
    ConfigChanged = 30
}
