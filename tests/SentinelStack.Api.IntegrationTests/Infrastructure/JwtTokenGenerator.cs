using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace SentinelStack.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Helper class for generating JWT tokens in integration tests.
/// </summary>
public static class JwtTokenGenerator
{
    private const string _testSecret = "_testSecretKeyThatIsAtLeast32CharactersLongForHMACSHA256";
    private const string _testIssuer = "SentinelStack.Tests";
    private const string _testAudience = "SentinelStack.Tests";

    /// <summary>
    /// Generates a JWT token with the specified claims.
    /// </summary>
    /// <param name="userId">User ID to include in the token</param>
    /// <param name="tenantIds">List of tenant IDs the user has access to</param>
    /// <param name="email">User email address</param>
    /// <param name="role">User role</param>
    /// <param name="expiresInMinutes">Token expiration time in minutes (default 60)</param>
    /// <returns>JWT token string</returns>
    public static string GenerateToken(
        Guid userId,
        List<Guid> tenantIds,
        string? email = null,
        string? role = null,
        int expiresInMinutes = 60)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_testSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenantIds", JsonSerializer.Serialize(tenantIds.Select(t => t.ToString()).ToList()))
        };

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        }

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _testIssuer,
            audience: _testAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a JWT token with a single tenant ID.
    /// </summary>
    public static string GenerateToken(
        Guid userId,
        Guid tenantId,
        string? email = null,
        string? role = null,
        int expiresInMinutes = 60)
    {
        return GenerateToken(userId, new List<Guid> { tenantId }, email, role, expiresInMinutes);
    }
}
