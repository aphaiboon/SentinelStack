using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Users.Commands;

/// <summary>
/// Command to update an existing user.
/// </summary>
public class UpdateUserCommand
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommand(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto?> ExecuteAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        if (request.DisplayName != null)
        {
            user.UpdateProfile(request.DisplayName, user.FirstName, user.LastName);
        }

        if (request.Role.HasValue)
        {
            user.UpdateRole(request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
