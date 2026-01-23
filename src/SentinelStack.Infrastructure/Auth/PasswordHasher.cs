using System.Security.Cryptography;
using SentinelStack.Application.Auth.Interfaces;

namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// PBKDF2-based password hashing following OWASP recommendations.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100_000; // OWASP recommendation for PBKDF2-SHA256
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    private const char Delimiter = ':';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // Format: iterations:salt:hash (all base64 encoded)
        return string.Join(
            Delimiter,
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split(Delimiter);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] storedHash;
        try
        {
            salt = Convert.FromBase64String(parts[1]);
            storedHash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var computedHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            Algorithm,
            storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
