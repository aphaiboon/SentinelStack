using FluentAssertions;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using Xunit;

namespace SentinelStack.Domain.UnitTests.Entities;

public class TenantTests
{
    [Fact]
    public void Constructor_ValidData_CreatesTenant()
    {
        // Arrange
        var name = "Acme Corp";
        var subdomain = "acme";
        var tier = TenantTier.Enterprise;

        // Act
        var tenant = new Tenant(name, subdomain, tier);

        // Assert
        tenant.Id.Should().NotBeEmpty();
        tenant.Name.Should().Be(name);
        tenant.Subdomain.Should().Be(subdomain);
        tenant.Tier.Should().Be(tier);
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.SuspendedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(TenantTier.Free, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Starter, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Growth, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Enterprise, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Healthcare, IsolationModel.SchemaPerTenant)]
    public void Constructor_TierProvided_SetsCorrectIsolationModel(TenantTier tier, IsolationModel expectedModel)
    {
        // Arrange & Act
        var tenant = new Tenant("Test", "test", tier);

        // Assert
        tenant.IsolationModel.Should().Be(expectedModel);
    }

    [Fact]
    public void Constructor_HealthcareTier_GeneratesSchemaName()
    {
        // Arrange & Act
        var tenant = new Tenant("Healthcare Provider", "healthco", TenantTier.Healthcare);

        // Assert
        tenant.IsolationModel.Should().Be(IsolationModel.SchemaPerTenant);
        tenant.SchemaName.Should().NotBeNullOrEmpty();
        tenant.SchemaName.Should().StartWith("tenant_");
        tenant.SchemaName.Should().NotContain("-"); // GUID hyphens removed
    }

    [Fact]
    public void Constructor_NonHealthcareTier_DoesNotGenerateSchemaName()
    {
        // Arrange & Act
        var tenant = new Tenant("Standard Corp", "standard", TenantTier.Enterprise);

        // Assert
        tenant.IsolationModel.Should().Be(IsolationModel.SharedSchema);
        tenant.SchemaName.Should().BeNull();
    }

    [Fact]
    public void Constructor_SubdomainUppercase_ConvertsToLowercase()
    {
        // Arrange
        var subdomain = "ACME";

        // Act
        var tenant = new Tenant("Acme", subdomain, TenantTier.Free);

        // Assert
        tenant.Subdomain.Should().Be("acme");
    }

    [Theory]
    [InlineData("")] // Empty
    [InlineData("  ")] // Whitespace
    [InlineData("ab")] // Too short (< 3 chars)
    [InlineData("a")] // Too short
    [InlineData("this-is-a-very-long-subdomain-name-that-exceeds-sixty-three-characters-limit")] // > 63 chars
    [InlineData("test_underscore")] // Underscore not allowed
    [InlineData("-startswithhyphen")] // Cannot start with hyphen
    [InlineData("endswithhyphen-")] // Cannot end with hyphen
    [InlineData("has space")] // Spaces not allowed
    [InlineData("test@domain")] // Special characters not allowed
    public void Constructor_InvalidSubdomain_ThrowsArgumentException(string invalidSubdomain)
    {
        // Arrange & Act
        var action = () => new Tenant("Test", invalidSubdomain, TenantTier.Free);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("subdomain");
    }

    [Theory]
    [InlineData("abc")] // Minimum length
    [InlineData("test-hyphen")] // With hyphen
    [InlineData("test123")] // With numbers
    [InlineData("a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6a7b8c9")] // Maximum length (63)
    [InlineData("start123end")] // Starts and ends with alphanumeric
    public void Constructor_ValidSubdomain_DoesNotThrow(string validSubdomain)
    {
        // Arrange & Act
        var action = () => new Tenant("Test", validSubdomain, TenantTier.Free);

        // Assert
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_EmptyName_ThrowsArgumentException(string invalidName)
    {
        // Arrange & Act
        var action = () => new Tenant(invalidName, "test", TenantTier.Free);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Suspend_ActiveTenant_SuspendsTenant()
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);

        // Act
        tenant.Suspend();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.SuspendedAtUtc.Should().NotBeNull();
        tenant.SuspendedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        tenant.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Suspend_AlreadySuspended_DoesNotChangeTimestamp()
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);
        tenant.Suspend();
        var firstSuspendedAt = tenant.SuspendedAtUtc;

        // Act
        tenant.Suspend(); // Suspend again

        // Assert
        tenant.Status.Should().Be(TenantStatus.Suspended);
        tenant.SuspendedAtUtc.Should().Be(firstSuspendedAt);
    }

    [Fact]
    public void Activate_SuspendedTenant_ActivatesTenant()
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);
        tenant.Suspend();

        // Act
        tenant.Activate();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.SuspendedAtUtc.Should().BeNull();
        tenant.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Activate_AlreadyActive_DoesNotChangeTimestamp()
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);

        // Act
        tenant.Activate();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Active);
        tenant.UpdatedAtUtc.Should().BeNull(); // No update needed
    }

    [Fact]
    public void UpdateName_ValidName_UpdatesName()
    {
        // Arrange
        var tenant = new Tenant("Old Name", "test", TenantTier.Free);
        var newName = "New Name";

        // Act
        tenant.UpdateName(newName);

        // Assert
        tenant.Name.Should().Be(newName);
        tenant.UpdatedAtUtc.Should().NotBeNull();
        tenant.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void UpdateName_EmptyName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);

        // Act
        var action = () => tenant.UpdateName(invalidName);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Delete_ActiveTenant_SoftDeletesTenant()
    {
        // Arrange
        var tenant = new Tenant("Test", "test", TenantTier.Free);

        // Act
        tenant.Delete();

        // Assert
        tenant.Status.Should().Be(TenantStatus.Deleted);
        tenant.UpdatedAtUtc.Should().NotBeNull();
    }
}
