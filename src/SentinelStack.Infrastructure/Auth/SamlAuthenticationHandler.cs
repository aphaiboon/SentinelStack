using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SentinelStack.Application.Auth.DTOs;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using Sustainsys.Saml2.AspNetCore2;

namespace SentinelStack.Infrastructure.Auth;

/// <summary>
/// Handles SAML authentication callbacks and converts SAML assertions to JWT tokens.
/// Implements AC #2, #3, #4 from Story 1.7.
/// </summary>
public class SamlAuthenticationHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISamlIdpConfigurationService _idpConfigService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<SamlAuthenticationHandler> _logger;

    public SamlAuthenticationHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider,
        ISamlIdpConfigurationService idpConfigService,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        ILogger<SamlAuthenticationHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _idpConfigService = idpConfigService;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Process SAML assertion and issue JWT tokens (AC #2, #3).
    /// </summary>
    public async Task<SamlAuthenticationResult> HandleSamlCallbackAsync(
        HttpContext httpContext,
        Guid? tenantId = null)
    {
        // Authenticate the SAML response
        var authenticateResult = await httpContext.AuthenticateAsync(Saml2Defaults.Scheme);

        if (!authenticateResult.Succeeded)
        {
            _logger.LogWarning("SAML authentication failed: {Error}", authenticateResult.Failure?.Message);
            return SamlAuthenticationResult.Failed("SAML authentication failed");
        }

        var principal = authenticateResult.Principal;
        if (principal == null)
        {
            _logger.LogError("SAML authentication succeeded but principal is null");
            return SamlAuthenticationResult.Failed("Authentication failed");
        }

        // Determine tenant ID (from RelayState for SP-initiated or IdP lookup for IdP-initiated)
        if (!tenantId.HasValue)
        {
            // Try to get from RelayState (SP-initiated flow)
            if (authenticateResult.Properties?.Items.TryGetValue("tenantId", out var tenantIdString) == true)
            {
                tenantId = Guid.Parse(tenantIdString);
            }
            else
            {
                // IdP-initiated flow: lookup tenant by IdP Entity ID
                var issuer = principal.FindFirst(ClaimTypes.NameIdentifier)?.Issuer;
                if (!string.IsNullOrEmpty(issuer))
                {
                    tenantId = await _idpConfigService.GetTenantIdByIdpEntityIdAsync(issuer);
                }
            }
        }

        if (!tenantId.HasValue)
        {
            _logger.LogError("Could not determine tenant ID from SAML response");
            return SamlAuthenticationResult.Failed("Could not determine tenant");
        }

        // Extract SAML attributes (AC #3)
        var email = ExtractEmail(principal);
        var firstName = ExtractFirstName(principal);
        var lastName = ExtractLastName(principal);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogError("SAML assertion missing email claim");
            return SamlAuthenticationResult.Failed("Email claim is required in SAML assertion");
        }

        // AC #2, #3: Create or update user
        var user = await GetOrCreateUserAsync(tenantId.Value, email, firstName, lastName);

        if (user == null)
        {
            _logger.LogError("Failed to create or retrieve user {Email}", email);
            return SamlAuthenticationResult.Failed("Failed to create user");
        }

        // AC #4: User deactivation is handled by IdP not sending assertion
        // If user is deactivated in IdP, we never receive the assertion

        // AC #2: Issue SentinelStack JWT
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _dateTimeProvider.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        var tokens = new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            TokenType = "Bearer"
        };

        _logger.LogInformation(
            "SAML authentication successful for user {Email}, tenant {TenantId}",
            email,
            tenantId.Value);

        return SamlAuthenticationResult.Success(tokens, user);
    }

    private string? ExtractEmail(ClaimsPrincipal principal)
    {
        // Try different email claim types (different IdPs use different claim types)
        return principal.FindFirst(ClaimTypes.Email)?.Value
               ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
               ?? principal.FindFirst("email")?.Value
               ?? principal.FindFirst("mail")?.Value;
    }

    private string? ExtractFirstName(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.GivenName)?.Value
               ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value
               ?? principal.FindFirst("givenName")?.Value
               ?? principal.FindFirst("firstName")?.Value;
    }

    private string? ExtractLastName(ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Surname)?.Value
               ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value
               ?? principal.FindFirst("sn")?.Value
               ?? principal.FindFirst("lastName")?.Value;
    }

    private async Task<User?> GetOrCreateUserAsync(
        Guid tenantId,
        string email,
        string? firstName,
        string? lastName)
    {
        // Try to find existing user by email and tenant
        var user = await _userRepository.FirstOrDefaultAsync(
            u => u.Email == email && u.TenantId == tenantId);

        if (user == null)
        {
            // AC #3: Auto-provision user from SAML attributes
            var displayName = !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName)
                ? $"{firstName} {lastName}"
                : email.Split('@')[0]; // Fallback to email username

            user = new User(
                tenantId,
                email,
                displayName,
                _passwordHasher.HashPassword(Guid.NewGuid().ToString()), // Random password (user won't use it)
                UserRole.Responder); // Default role: Responder can respond to incidents

            // Set first name and last name using reflection (private setters)
            if (!string.IsNullOrEmpty(firstName))
            {
                typeof(User).GetProperty("FirstName")!.SetValue(user, firstName);
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                typeof(User).GetProperty("LastName")!.SetValue(user, lastName);
            }

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Auto-provisioned user {Email} for tenant {TenantId} from SAML",
                email,
                tenantId);
        }
        else
        {
            // Update user attributes from SAML on subsequent logins
            var updated = false;

            if (!string.IsNullOrEmpty(firstName))
            {
                typeof(User).GetProperty("FirstName")!.SetValue(user, firstName);
                updated = true;
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                typeof(User).GetProperty("LastName")!.SetValue(user, lastName);
                updated = true;
            }

            if (updated)
            {
                _userRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogDebug("Updated user {Email} attributes from SAML", email);
            }
        }

        return user;
    }
}

/// <summary>
/// Result of SAML authentication processing.
/// </summary>
public class SamlAuthenticationResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public TokenResponse? Tokens { get; init; }
    public User? User { get; init; }

    public static SamlAuthenticationResult Success(TokenResponse tokens, User user)
    {
        return new SamlAuthenticationResult
        {
            IsSuccess = true,
            Tokens = tokens,
            User = user
        };
    }

    public static SamlAuthenticationResult Failed(string errorMessage)
    {
        return new SamlAuthenticationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
