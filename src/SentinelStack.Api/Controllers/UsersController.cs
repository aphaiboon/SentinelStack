using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Users.Commands;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Application.Users.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Manages user operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly CreateUserCommand _createCommand;
    private readonly UpdateUserCommand _updateCommand;
    private readonly GetUserQuery _getQuery;
    private readonly GetUsersQuery _listQuery;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        CreateUserCommand createCommand,
        UpdateUserCommand updateCommand,
        GetUserQuery getQuery,
        GetUsersQuery listQuery,
        ILogger<UsersController> logger)
    {
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _getQuery = getQuery;
        _listQuery = listQuery;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _createCommand.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("User {UserId} created with email {Email}", user.Id, user.Email);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _getQuery.ExecuteAsync(id, cancellationToken);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }

    /// <summary>
    /// Lists users with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetUsersResponse>> ListUsers(
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetUsersRequest
        {
            Role = role,
            IsActive = isActive,
            Page = page,
            PageSize = Math.Min(pageSize, 100) // Cap at 100 per page
        };

        var response = await _listQuery.ExecuteAsync(request, cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _updateCommand.ExecuteAsync(id, request, cancellationToken);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        _logger.LogInformation("User {UserId} updated", id);

        return Ok(user);
    }

    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        // Try standard JWT claim first, then fall back to mapped claim type
        var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "Invalid user context" });
        }

        var user = await _getQuery.ExecuteAsync(userId, cancellationToken);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(user);
    }
}
