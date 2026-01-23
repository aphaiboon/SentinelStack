using System.ComponentModel.DataAnnotations;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Users.DTOs;

/// <summary>
/// Request model for updating an existing user.
/// </summary>
public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? DisplayName { get; init; }

    public UserRole? Role { get; init; }

    public bool? IsActive { get; init; }
}
