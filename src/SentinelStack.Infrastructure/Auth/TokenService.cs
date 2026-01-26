using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _dateTimeProvider;

    // In-memory refresh token store for Alpha. Replace with database in production.
    private static readonly Dictionary<string, (Guid UserId, DateTime ExpiresAt)> _refreshTokens = new();

    public TokenService(IOptions<JwtSettings> settings, IDateTimeProvider dateTimeProvider)
    {
        _settings = settings.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public string GenerateAccessToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // For multi-tenant support, tenantIds is a JSON array of allowed tenant GUIDs
        // Currently users belong to one tenant, so array has one element
        var tenantIds = JsonSerializer.Serialize(new[] { user.TenantId.ToString() });

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", user.TenantId.ToString()), // Legacy claim for backward compatibility
            new("tenantIds", tenantIds), // Multi-tenant claim (JSON array)
            new(ClaimTypes.Role, user.Role.ToString()),
            new("display_name", user.DisplayName)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (_refreshTokens.TryGetValue(refreshToken, out var tokenData))
        {
            if (tokenData.ExpiresAt > _dateTimeProvider.UtcNow)
            {
                return Task.FromResult<Guid?>(tokenData.UserId);
            }

            // Token expired, remove it
            _refreshTokens.Remove(refreshToken);
        }

        return Task.FromResult<Guid?>(null);
    }

    public Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        _refreshTokens[refreshToken] = (userId, expiresAtUtc);
        return Task.CompletedTask;
    }

    public Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokensToRemove = _refreshTokens
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            _refreshTokens.Remove(token);
        }

        return Task.CompletedTask;
    }
}
