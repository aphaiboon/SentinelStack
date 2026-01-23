namespace SentinelStack.Application.Auth.DTOs;

/// <summary>
/// Response model containing authentication tokens.
/// </summary>
public class TokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string TokenType { get; init; } = "Bearer";
}
