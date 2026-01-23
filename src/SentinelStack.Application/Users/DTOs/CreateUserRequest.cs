using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Users.DTOs;

/// <summary>
/// Request model for creating a new user.
/// </summary>
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public required string Email { get; init; }

    [Required]
    [MinLength(8)]
    public required string Password { get; init; }

    [Required]
    [MaxLength(100)]
    public required string DisplayName { get; init; }

    [Required]
    public UserRole Role { get; init; }
}
