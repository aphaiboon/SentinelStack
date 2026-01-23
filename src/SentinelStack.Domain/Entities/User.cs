using SentinelStack.Domain.Common;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this user belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// User's email address (unique per tenant).
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// User's display name.
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; private set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; private set; }

    /// <summary>
    /// Hashed password.
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// User's role in the system.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Whether MFA is enabled for this user.
    /// </summary>
    public bool MfaEnabled { get; private set; }

    /// <summary>
    /// UTC timestamp of the user's last login.
    /// </summary>
    public DateTime? LastLoginAtUtc { get; private set; }

    /// <summary>
    /// Number of failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// UTC timestamp when the account was locked.
    /// </summary>
    public DateTime? LockedUntilUtc { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private User() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public User(
        Guid tenantId,
        string email,
        string displayName,
        string passwordHash,
        UserRole role = UserRole.Viewer)
    {
        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        DisplayName = displayName;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    /// <summary>
    /// Updates the user's profile information.
    /// </summary>
    public void UpdateProfile(string displayName, string? firstName, string? lastName)
    {
        DisplayName = displayName;
        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Updates the user's password.
    /// </summary>
    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
    }

    /// <summary>
    /// Updates the user's role.
    /// </summary>
    public void UpdateRole(UserRole role)
    {
        Role = role;
    }

    /// <summary>
    /// Records a successful login.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAtUtc = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
    }

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 15)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntilUtc = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    /// <summary>
    /// Checks if the account is currently locked.
    /// </summary>
    public bool IsLocked => LockedUntilUtc.HasValue && LockedUntilUtc.Value > DateTime.UtcNow;

    /// <summary>
    /// Enables MFA for this user.
    /// </summary>
    public void EnableMfa()
    {
        MfaEnabled = true;
    }

    /// <summary>
    /// Disables MFA for this user.
    /// </summary>
    public void DisableMfa()
    {
        MfaEnabled = false;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates the user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
        LockedUntilUtc = null;
    }
}
