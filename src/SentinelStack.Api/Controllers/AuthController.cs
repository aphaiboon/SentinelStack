using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SentinelStack.Application.Auth.DTOs;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Infrastructure.Auth;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// Handles authentication operations including login and token refresh.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: user not found for email {Email}", request.Email);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt failed: user {UserId} is inactive", user.Id);
            return Unauthorized(new { message = "Account is disabled" });
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: invalid password for user {UserId}", user.Id);
            return Unauthorized(new { message = "Invalid email or password" });
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _dateTimeProvider.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry, cancellationToken);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            TokenType = "Bearer"
        });
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (userId == null)
        {
            _logger.LogWarning("Refresh token validation failed");
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Refresh token for inactive or deleted user {UserId}", userId);
            return Unauthorized(new { message = "User account not found or disabled" });
        }

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokensAsync(userId.Value, cancellationToken);

        // Generate new tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _dateTimeProvider.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry, cancellationToken);

        _logger.LogInformation("Tokens refreshed for user {UserId}", user.Id);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            TokenType = "Bearer"
        });
    }

    /// <summary>
    /// Logs out the current user by revoking all refresh tokens.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "Invalid user context" });
        }

        await _tokenService.RevokeRefreshTokensAsync(userId, cancellationToken);

        _logger.LogInformation("User {UserId} logged out", userId);

        return Ok(new { message = "Logged out successfully" });
    }
}
