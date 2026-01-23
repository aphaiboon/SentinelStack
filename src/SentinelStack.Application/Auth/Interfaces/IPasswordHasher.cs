namespace SentinelStack.Application.Auth.Interfaces;

/// <summary>
/// Service for hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool VerifyPassword(string password, string passwordHash);
}
