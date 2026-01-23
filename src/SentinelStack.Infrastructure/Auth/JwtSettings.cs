namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// JWT configuration settings.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key for signing tokens (minimum 32 characters).
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Token issuer.
    /// </summary>
    public string Issuer { get; set; } = "sentinelstack";

    /// <summary>
    /// Token audience.
    /// </summary>
    public string Audience { get; set; } = "sentinelstack-api";

    /// <summary>
    /// Access token lifetime in minutes.
    /// </summary>
    public int AccessTokenMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days.
    /// </summary>
    public int RefreshTokenDays { get; set; } = 7;
}
