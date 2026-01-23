using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Users.DTOs;

/// <summary>
/// Data transfer object for User entity.
/// </summary>
public class UserDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public UserRole Role { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLoginAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public Guid? UpdatedBy { get; init; }
}
