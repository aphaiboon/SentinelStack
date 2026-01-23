using System.ComponentModel.DataAnnotations;

namespace SentinelStack.Application.Auth.DTOs;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }
}
