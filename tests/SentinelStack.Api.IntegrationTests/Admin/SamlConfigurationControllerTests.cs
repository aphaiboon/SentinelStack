using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Domain.Enums;
using SentinelStack.Domain.UnitTests.Builders;

namespace SentinelStack.Api.IntegrationTests.Admin;

/// <summary>
/// Integration tests for SAML configuration (Story 1.7, Task 8)
/// Tests AC #5: Upload IdP metadata XML, validate, and extract configuration
/// </summary>
public class SamlConfigurationControllerTests : BaseIntegrationTest
{
    // Minimal valid SAML metadata for testing
    private const string ValidOktaMetadata = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<EntityDescriptor entityID=""http://www.okta.com/exk123"" xmlns=""urn:oasis:names:tc:SAML:2.0:metadata"">
  <IDPSSODescriptor protocolSupportEnumeration=""urn:oasis:names:tc:SAML:2.0:protocol"">
    <KeyDescriptor use=""signing"">
      <KeyInfo xmlns=""http://www.w3.org/2000/09/xmldsig#"">
        <X509Data>
          <X509Certificate>MIIDpDCCAoygAwIBAgIGAXoRMnFQMA0GCSqGSIb3DQEBCwUAMIGSMQswCQYDVQQGEwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYQ==</X509Certificate>
        </X509Data>
      </KeyInfo>
    </KeyDescriptor>
    <SingleSignOnService Binding=""urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST""
                         Location=""https://dev-12345.okta.com/app/dev-12345_sentinelstack_1/exk123/sso/saml""/>
  </IDPSSODescriptor>
</EntityDescriptor>";
    public SamlConfigurationControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ConfigureSaml_EnterpriseTier_ValidMetadata_ReturnsSamlConfiguration()
    {
        // Arrange - AC #5: Enterprise tier can configure SAML
        var tenant = TenantBuilder.Default()
            .WithTier(TenantTier.Enterprise)
            .Build();
        await SeedTenant(tenant);

        var request = new
        {
            IdpMetadataXml = ValidOktaMetadata
        };

        // Act
        var response = await PostAsync($"/admin/tenants/{tenant.Id}/saml-config", request);

        // Assert - AC #5: Validates XML and extracts configuration
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<SamlConfigurationDto>(JsonOptions);
        config.Should().NotBeNull();
        config!.TenantId.Should().Be(tenant.Id);
        config.IdpEntityId.Should().Be("http://www.okta.com/exk123");
        config.SsoServiceUrl.Should().Be("https://dev-12345.okta.com/app/dev-12345_sentinelstack_1/exk123/sso/saml");
        config.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task ConfigureSaml_NonEnterpriseTier_Returns403()
    {
        // Arrange - Enterprise tier only requirement (FR79)
        var tenant = TenantBuilder.Default()
            .WithTier(TenantTier.Starter)
            .Build();
        await SeedTenant(tenant);

        var request = new
        {
            IdpMetadataXml = ValidOktaMetadata
        };

        // Act
        var response = await PostAsync($"/admin/tenants/{tenant.Id}/saml-config", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ConfigureSaml_InvalidMetadata_Returns400()
    {
        // Arrange - AC #5: Validate XML schema
        var tenant = TenantBuilder.Default()
            .WithTier(TenantTier.Enterprise)
            .Build();
        await SeedTenant(tenant);

        var request = new
        {
            IdpMetadataXml = "<invalid>xml</invalid>"
        };

        // Act
        var response = await PostAsync($"/admin/tenants/{tenant.Id}/saml-config", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSamlConfig_ConfigExists_ReturnsConfiguration()
    {
        // Arrange
        var tenant = TenantBuilder.Default()
            .WithTier(TenantTier.Enterprise)
            .Build();
        await SeedTenant(tenant);

        // Configure SAML first
        await PostAsync($"/admin/tenants/{tenant.Id}/saml-config", new
        {
            IdpMetadataXml = ValidOktaMetadata
        });

        // Act
        var response = await Client.GetAsync($"/admin/tenants/{tenant.Id}/saml-config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await response.Content.ReadFromJsonAsync<SamlConfigurationDto>(JsonOptions);
        config.Should().NotBeNull();
        config!.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task DeleteSamlConfig_ConfigExists_Returns204()
    {
        // Arrange
        var tenant = TenantBuilder.Default()
            .WithTier(TenantTier.Enterprise)
            .Build();
        await SeedTenant(tenant);

        // Configure SAML first
        await PostAsync($"/admin/tenants/{tenant.Id}/saml-config", new
        {
            IdpMetadataXml = ValidOktaMetadata
        });

        // Act
        var response = await Client.DeleteAsync($"/admin/tenants/{tenant.Id}/saml-config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await Client.GetAsync($"/admin/tenants/{tenant.Id}/saml-config");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
