using System.Security.Cryptography;
using SentinelStack.Application.Auth.Interfaces;

namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// PBKDF2-based password hashing following OWASP recommendations.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int _saltSize = 16; // 128 bits
    private const int _keySize = 32; // 256 bits
    private const int _iterations = 100_000; // OWASP recommendation for PBKDF2-SHA256
    private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

    private const char _delimiter = ':';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            _algorithm,
            _keySize);

        // Format: iterations:salt:hash (all base64 encoded)
        return string.Join(
            _delimiter,
            _iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var parts = passwordHash.Split(_delimiter);
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
            _algorithm,
            storedHash.Length);

        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }
}
