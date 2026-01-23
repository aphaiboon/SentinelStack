using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Users.Queries;

/// <summary>
/// Query to get a single user by ID.
/// </summary>
public class GetUserQuery
{
    private readonly IUserRepository _userRepository;

    public GetUserQuery(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        TenantId = user.TenantId,
        Email = user.Email,
        DisplayName = user.DisplayName,
        Role = user.Role,
        IsActive = user.IsActive,
        LastLoginAtUtc = user.LastLoginAtUtc,
        CreatedAtUtc = user.CreatedAtUtc,
        CreatedBy = user.CreatedBy,
        UpdatedAtUtc = user.UpdatedAtUtc,
        UpdatedBy = user.UpdatedBy
    };
}
