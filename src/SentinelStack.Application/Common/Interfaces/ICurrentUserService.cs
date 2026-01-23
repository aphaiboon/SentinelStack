namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current user context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID from the JWT claims.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email from the JWT claims.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's roles from the JWT claims.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    bool IsInRole(string role);

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    bool HasPermission(string permission);
}
