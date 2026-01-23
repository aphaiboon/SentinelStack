using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Auth.Interfaces;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for a user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a refresh token and returns the user ID if valid.
    /// </summary>
    Task<Guid?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a refresh token for a user.
    /// </summary>
    Task StoreRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiresAtUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all refresh tokens for a user.
    /// </summary>
    Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
