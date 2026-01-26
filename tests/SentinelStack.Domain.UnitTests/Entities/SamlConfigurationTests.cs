using FluentAssertions;
using SentinelStack.Domain.Entities;
using Xunit;

namespace SentinelStack.Domain.UnitTests.Entities;

public class SamlConfigurationTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesSamlConfiguration()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var idpEntityId = "http://www.okta.com/exk123";
        var ssoServiceUrl = "https://dev-12345.okta.com/app/dev-12345_sentinelstack_1/exk123/sso/saml";
        var signingCertificate = "MIICmzCCAYMCBgGQL...";

        // Act
        var samlConfig = new SamlConfiguration(
            tenantId,
            idpEntityId,
            ssoServiceUrl,
            signingCertificate);

        // Assert
        samlConfig.Id.Should().NotBeEmpty();
        samlConfig.TenantId.Should().Be(tenantId);
        samlConfig.IdpEntityId.Should().Be(idpEntityId);
        samlConfig.SsoServiceUrl.Should().Be(ssoServiceUrl);
        samlConfig.SigningCertificate.Should().Be(signingCertificate);
        samlConfig.Enabled.Should().BeTrue(); // Default enabled
        samlConfig.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyIdpEntityId_ThrowsArgumentException(string? invalidIdpEntityId)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var ssoServiceUrl = "https://example.com/sso";
        var signingCertificate = "cert";

        // Act
        var act = () => new SamlConfiguration(
            tenantId,
            invalidIdpEntityId!,
            ssoServiceUrl,
            signingCertificate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IdP Entity ID is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptySsoServiceUrl_ThrowsArgumentException(string? invalidUrl)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var idpEntityId = "http://www.okta.com/exk123";
        var signingCertificate = "cert";

        // Act
        var act = () => new SamlConfiguration(
            tenantId,
            idpEntityId,
            invalidUrl!,
            signingCertificate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*SSO Service URL is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptySigningCertificate_ThrowsArgumentException(string? invalidCert)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var idpEntityId = "http://www.okta.com/exk123";
        var ssoServiceUrl = "https://example.com/sso";

        // Act
        var act = () => new SamlConfiguration(
            tenantId,
            idpEntityId,
            ssoServiceUrl,
            invalidCert!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Signing certificate is required*");
    }

    [Fact]
    public void Disable_EnabledConfiguration_DisablesIt()
    {
        // Arrange
        var samlConfig = new SamlConfiguration(
            Guid.NewGuid(),
            "http://www.okta.com/exk123",
            "https://example.com/sso",
            "cert");

        // Act
        samlConfig.Disable();

        // Assert
        samlConfig.Enabled.Should().BeFalse();
        samlConfig.UpdatedAtUtc.Should().NotBeNull();
        samlConfig.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Enable_DisabledConfiguration_EnablesIt()
    {
        // Arrange
        var samlConfig = new SamlConfiguration(
            Guid.NewGuid(),
            "http://www.okta.com/exk123",
            "https://example.com/sso",
            "cert");
        samlConfig.Disable();

        // Act
        samlConfig.Enable();

        // Assert
        samlConfig.Enabled.Should().BeTrue();
        samlConfig.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void UpdateMetadata_ValidMetadata_UpdatesConfiguration()
    {
        // Arrange
        var samlConfig = new SamlConfiguration(
            Guid.NewGuid(),
            "http://www.okta.com/old",
            "https://old.example.com/sso",
            "oldcert");

        var newIdpEntityId = "http://www.okta.com/new";
        var newSsoServiceUrl = "https://new.example.com/sso";
        var newSigningCert = "newcert";
        var newMetadataXml = "<EntityDescriptor>...</EntityDescriptor>";

        // Act
        samlConfig.UpdateMetadata(
            newIdpEntityId,
            newSsoServiceUrl,
            newSigningCert,
            newMetadataXml);

        // Assert
        samlConfig.IdpEntityId.Should().Be(newIdpEntityId);
        samlConfig.SsoServiceUrl.Should().Be(newSsoServiceUrl);
        samlConfig.SigningCertificate.Should().Be(newSigningCert);
        samlConfig.IdpMetadataXml.Should().Be(newMetadataXml);
        samlConfig.UpdatedAtUtc.Should().NotBeNull();
    }
}
