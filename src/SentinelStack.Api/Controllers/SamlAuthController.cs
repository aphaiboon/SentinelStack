using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Infrastructure.Auth;
using Sustainsys.Saml2.AspNetCore2;

namespace SentinelStack.Api.Controllers;

/// <summary>
/// SAML SSO authentication endpoints (Story 1.7)
/// Handles SP-initiated and IdP-initiated SAML authentication flows.
/// </summary>
[ApiController]
[Route("auth/saml")]
public class SamlAuthController : ControllerBase
{
    private readonly ISamlIdpConfigurationService _idpConfigService;
    private readonly ISamlConfigurationRepository _samlConfigRepository;
    private readonly SamlAuthenticationHandler _samlAuthHandler;
    private readonly DynamicSamlOptionsProvider _samlOptionsProvider;
    private readonly ILogger<SamlAuthController> _logger;

    public SamlAuthController(
        ISamlIdpConfigurationService idpConfigService,
        ISamlConfigurationRepository samlConfigRepository,
        SamlAuthenticationHandler samlAuthHandler,
        DynamicSamlOptionsProvider samlOptionsProvider,
        ILogger<SamlAuthController> logger)
    {
        _idpConfigService = idpConfigService;
        _samlConfigRepository = samlConfigRepository;
        _samlAuthHandler = samlAuthHandler;
        _samlOptionsProvider = samlOptionsProvider;
        _logger = logger;
    }

    /// <summary>
    /// Check if SAML is configured for a tenant (AC #1)
    /// </summary>
    [HttpGet("configured/{tenantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> IsSamlConfigured(Guid tenantId)
    {
        var isConfigured = await _idpConfigService.IsSamlConfiguredForTenantAsync(tenantId);
        return Ok(new { configured = isConfigured });
    }

    /// <summary>
    /// Initiate SP-initiated SAML login (AC #1)
    /// Redirects user to IdP for authentication.
    /// </summary>
    [HttpGet("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] Guid tenantId)
    {
        // Verify tenant has SAML configured
        var samlConfig = await _samlConfigRepository.GetByTenantIdAsync(tenantId);
        if (samlConfig == null || !samlConfig.Enabled)
        {
            return BadRequest(new { error = "SAML authentication is not configured for this tenant" });
        }

        // Load IdP configuration for this tenant
        var idp = await _samlOptionsProvider.GetIdpForTenantAsync(tenantId);
        if (idp == null)
        {
            return BadRequest(new { error = "Failed to load SAML configuration" });
        }

        // Store tenant ID in authentication properties (RelayState)
        // so we can retrieve it after authentication
        var authProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback)),
            Items =
            {
                { "tenantId", tenantId.ToString() }
            }
        };

        _logger.LogInformation(
            "Initiating SP-initiated SAML login for tenant {TenantId} with IdP {IdpEntityId}",
            tenantId,
            samlConfig.IdpEntityId);

        // Challenge with SAML authentication scheme
        // Sustainsys.Saml2 middleware will:
        // 1. Generate SAML AuthnRequest
        // 2. Sign the request (if configured)
        // 3. Redirect user to IdP SSO URL
        return Challenge(authProperties, Saml2Defaults.Scheme);
    }

    /// <summary>
    /// SAML Assertion Consumer Service (ACS) callback (AC #2, #3, #4)
    /// Handles both SP-initiated and IdP-initiated SAML authentication flows.
    /// Processes SAML assertion, auto-provisions users, and issues JWT tokens.
    /// </summary>
    [HttpPost("acs")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] Guid? tenantId = null)
    {
        // Process SAML assertion and convert to JWT
        var result = await _samlAuthHandler.HandleSamlCallbackAsync(HttpContext, tenantId);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("SAML authentication failed: {Error}", result.ErrorMessage);
            return Unauthorized(new { error = result.ErrorMessage });
        }

        // Return JWT tokens (AC #2)
        return Ok(new
        {
            accessToken = result.Tokens!.AccessToken,
            refreshToken = result.Tokens.RefreshToken,
            expiresIn = 3600,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                displayName = result.User.DisplayName,
                role = result.User.Role.ToString(),
                tenantId = result.User.TenantId
            }
        });
    }

    /// <summary>
    /// SAML metadata endpoint - provides SP metadata for IdP configuration.
    /// IdPs need this metadata to configure SentinelStack as a Service Provider.
    /// </summary>
    [HttpGet("metadata")]
    [AllowAnonymous]
    public IActionResult Metadata()
    {
        // TODO: Generate SP metadata XML
        // This would include:
        // - SP Entity ID
        // - ACS endpoint URL
        // - SP signing certificate (if request signing is enabled)
        // For now, return a simple message
        return Ok(new
        {
            entityId = "https://api.sentinelstack.io/saml",
            acsUrl = $"{Request.Scheme}://{Request.Host}/auth/saml/acs",
            message = "SP metadata generation not yet implemented. Configure your IdP with the endpoints above."
        });
    }
}

