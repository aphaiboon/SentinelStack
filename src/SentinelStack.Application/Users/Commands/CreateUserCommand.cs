using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Users.Commands;

/// <summary>
/// Command to create a new user.
/// </summary>
public class CreateUserCommand
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentTenantService _tenantService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommand(
        IUserRepository userRepository,
        ICurrentTenantService tenantService,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tenantService = tenantService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User(
            tenantId: tenantId,
            email: request.Email,
            displayName: request.DisplayName,
            passwordHash: passwordHash,
            role: request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
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
