using System.ComponentModel.DataAnnotations;

namespace SentinelStack.Application.Auth.DTOs;

/// <summary>
/// Request model for refreshing tokens.
/// </summary>
public class RefreshTokenRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
