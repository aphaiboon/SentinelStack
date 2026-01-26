using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.Controllers.Admin;

/// <summary>
/// Admin endpoint for SAML SSO configuration (Enterprise tier only).
/// FR79: SSO SAML 2.0 Authentication
/// </summary>
[ApiController]
[Route("admin/tenants/{tenantId}/saml-config")]
[Authorize] // Future: Add [Authorize(Policy = "PlatformAdmin")] when RBAC is implemented
public class SamlConfigurationController : ControllerBase
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ISamlConfigurationRepository _samlConfigRepository;
    private readonly ISamlMetadataParser _metadataParser;
    private readonly ILogger<SamlConfigurationController> _logger;

    public SamlConfigurationController(
        ITenantRepository tenantRepository,
        ISamlConfigurationRepository samlConfigRepository,
        ISamlMetadataParser metadataParser,
        ILogger<SamlConfigurationController> logger)
    {
        _tenantRepository = tenantRepository;
        _samlConfigRepository = samlConfigRepository;
        _metadataParser = metadataParser;
        _logger = logger;
    }

    /// <summary>
    /// Configure SAML SSO for a tenant (AC #5)
    /// </summary>
    /// <param name="tenantId">Tenant ID</param>
    /// <param name="request">SAML configuration request with IdP metadata XML</param>
    /// <returns>SAML configuration details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SamlConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SamlConfigurationDto>> ConfigureSaml(
        Guid tenantId,
        [FromBody] ConfigureSamlRequest request)
    {
        // Validate tenant exists
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found" });
        }

        // AC #5: Enterprise tier only (FR79)
        if (tenant.Tier != TenantTier.Enterprise && tenant.Tier != TenantTier.Healthcare)
        {
            _logger.LogWarning(
                "Tenant {TenantId} with tier {Tier} attempted to configure SAML (Enterprise/Healthcare only)",
                tenantId, tenant.Tier);

            return StatusCode(403, new
            {
                error = "SAML authentication is only available for Enterprise and Healthcare tiers"
            });
        }

        // AC #5: Validate and parse IdP metadata XML
        var metadata = _metadataParser.ParseMetadata(request.IdpMetadataXml);
        if (!metadata.IsValid)
        {
            _logger.LogWarning(
                "Invalid SAML metadata for tenant {TenantId}: {Error}",
                tenantId, metadata.ErrorMessage);

            return BadRequest(new { error = metadata.ErrorMessage });
        }

        // Create or update SAML configuration
        var existing = await _samlConfigRepository.GetByTenantIdAsync(tenantId);

        SamlConfiguration samlConfig;
        if (existing != null)
        {
            // Update existing configuration
            existing.UpdateMetadata(
                metadata.IdpEntityId,
                metadata.SsoServiceUrl,
                metadata.SigningCertificate,
                request.IdpMetadataXml);
            samlConfig = await _samlConfigRepository.CreateOrUpdateAsync(existing);

            _logger.LogInformation(
                "Updated SAML configuration for tenant {TenantId} with IdP {IdpEntityId}",
                tenantId, metadata.IdpEntityId);
        }
        else
        {
            // Create new configuration
            samlConfig = new SamlConfiguration(
                tenantId,
                metadata.IdpEntityId,
                metadata.SsoServiceUrl,
                metadata.SigningCertificate);

            samlConfig.UpdateMetadata(
                metadata.IdpEntityId,
                metadata.SsoServiceUrl,
                metadata.SigningCertificate,
                request.IdpMetadataXml);

            samlConfig = await _samlConfigRepository.CreateOrUpdateAsync(samlConfig);

            _logger.LogInformation(
                "Created SAML configuration for tenant {TenantId} with IdP {IdpEntityId}",
                tenantId, metadata.IdpEntityId);
        }

        return Ok(new SamlConfigurationDto
        {
            Id = samlConfig.Id,
            TenantId = samlConfig.TenantId,
            IdpEntityId = samlConfig.IdpEntityId,
            SsoServiceUrl = samlConfig.SsoServiceUrl,
            Enabled = samlConfig.Enabled,
            CreatedAtUtc = samlConfig.CreatedAtUtc,
            UpdatedAtUtc = samlConfig.UpdatedAtUtc
        });
    }

    /// <summary>
    /// Get SAML configuration for a tenant
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SamlConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SamlConfigurationDto>> GetSamlConfig(Guid tenantId)
    {
        var samlConfig = await _samlConfigRepository.GetByTenantIdAsync(tenantId);
        if (samlConfig == null)
        {
            return NotFound(new { error = "SAML configuration not found for this tenant" });
        }

        return Ok(new SamlConfigurationDto
        {
            Id = samlConfig.Id,
            TenantId = samlConfig.TenantId,
            IdpEntityId = samlConfig.IdpEntityId,
            SsoServiceUrl = samlConfig.SsoServiceUrl,
            Enabled = samlConfig.Enabled,
            CreatedAtUtc = samlConfig.CreatedAtUtc,
            UpdatedAtUtc = samlConfig.UpdatedAtUtc
        });
    }

    /// <summary>
    /// Delete SAML configuration for a tenant
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSamlConfig(Guid tenantId)
    {
        var samlConfig = await _samlConfigRepository.GetByTenantIdAsync(tenantId);
        if (samlConfig == null)
        {
            return NotFound(new { error = "SAML configuration not found for this tenant" });
        }

        await _samlConfigRepository.DeleteAsync(tenantId);

        _logger.LogInformation("Deleted SAML configuration for tenant {TenantId}", tenantId);

        return NoContent();
    }
}
