using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Users.Queries;

/// <summary>
/// Request model for filtering users.
/// </summary>
public class GetUsersRequest
{
    public UserRole? Role { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response model for paginated users.
/// </summary>
public class GetUsersResponse
{
    public required IReadOnlyList<UserDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get users with filtering and pagination.
/// </summary>
public class GetUsersQuery
{
    private readonly IUserRepository _userRepository;

    public GetUsersQuery(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUsersResponse> ExecuteAsync(GetUsersRequest request, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = users.AsEnumerable();

        if (request.Role.HasValue)
        {
            filtered = filtered.Where(u => u.Role == request.Role.Value);
        }

        if (request.IsActive.HasValue)
        {
            filtered = filtered.Where(u => u.IsActive == request.IsActive.Value);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var paged = filteredList
            .OrderBy(u => u.DisplayName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetUsersResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
