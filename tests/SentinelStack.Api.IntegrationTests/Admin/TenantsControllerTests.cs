using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Tenants.DTOs;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.IntegrationTests.Admin;

/// <summary>
/// Integration tests for Admin Tenants controller (Story 2.1).
/// Tests AC #1-#5 from story requirements.
/// </summary>
public class TenantsControllerTests : BaseIntegrationTest
{
    public TenantsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateTenant_ValidData_ReturnsTenant()
    {
        // Arrange
        var request = new
        {
            Name = "Acme Corporation",
            Subdomain = "acme",
            Tier = TenantTier.Enterprise
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDto>(JsonOptions);
        tenant.Should().NotBeNull();
        tenant!.Name.Should().Be("Acme Corporation");
        tenant.Subdomain.Should().Be("acme");
        tenant.Tier.Should().Be(TenantTier.Enterprise);
        tenant.IsolationModel.Should().Be(IsolationModel.SharedSchema); // AC #4
        tenant.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task CreateTenant_HealthcareTier_SetsSchemaPerTenant()
    {
        // Arrange - AC #3: Healthcare tier auto-sets SchemaPerTenant
        var request = new
        {
            Name = "Healthcare Provider",
            Subdomain = "healthco",
            Tier = TenantTier.Healthcare
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDto>(JsonOptions);
        tenant.Should().NotBeNull();
        tenant!.IsolationModel.Should().Be(IsolationModel.SchemaPerTenant); // AC #3
        tenant.SchemaName.Should().StartWith("tenant_");
        tenant.SchemaName.Should().NotContain("-"); // GUID without hyphens
    }

    [Theory]
    [InlineData(TenantTier.Free, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Starter, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Growth, IsolationModel.SharedSchema)]
    [InlineData(TenantTier.Enterprise, IsolationModel.SharedSchema)]
    public async Task CreateTenant_NonHealthcareTiers_SetSharedSchema(TenantTier tier, IsolationModel expectedModel)
    {
        // Arrange - AC #4: Non-healthcare tiers use SharedSchema
        var request = new
        {
            Name = $"Test {tier} Tenant",
            Subdomain = $"test{tier.ToString().ToLower()}{Guid.NewGuid():N}",
            Tier = tier
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var tenant = await response.Content.ReadFromJsonAsync<TenantDto>(JsonOptions);
        tenant!.IsolationModel.Should().Be(expectedModel);
    }

    [Fact]
    public async Task CreateTenant_InvalidSubdomain_Returns400()
    {
        // Arrange - AC #1: Subdomain validation (DNS-safe)
        var request = new
        {
            Name = "Test Company",
            Subdomain = "INVALID_SUBDOMAIN", // Uppercase and underscore not allowed
            Tier = TenantTier.Free
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTenant_DuplicateSubdomain_Returns400()
    {
        // Arrange - AC #1: Subdomain must be unique
        var subdomain = $"unique{Guid.NewGuid():N}";
        var request1 = new
        {
            Name = "First Company",
            Subdomain = subdomain,
            Tier = TenantTier.Free
        };
        var request2 = new
        {
            Name = "Second Company",
            Subdomain = subdomain, // Duplicate
            Tier = TenantTier.Starter
        };

        // Act
        var response1 = await PostAsync("/admin/tenants", request1);
        var response2 = await PostAsync("/admin/tenants", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTenants_ReturnsPagedResults()
    {
        // Arrange - AC #5: List all tenants
        // Create 3 test tenants
        for (int i = 0; i < 3; i++)
        {
            await PostAsync("/admin/tenants", new
            {
                Name = $"Test Tenant {i}",
                Subdomain = $"test{i}{Guid.NewGuid():N}",
                Tier = TenantTier.Free
            });
        }

        // Act
        var response = await Client.GetAsync("/admin/tenants?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedTenantsResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    private record PagedTenantsResponse
    {
        public List<TenantDto> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }

    [Theory]
    [InlineData("abc")] // Minimum length (3)
    [InlineData("test-hyphen-ok")] // With hyphens
    [InlineData("test123numbers")] // With numbers
    [InlineData("a1b2c3d4e5f6g7h8i9")] // Mixed alphanumeric
    public async Task CreateTenant_ValidSubdomainFormats_Succeeds(string validSubdomain)
    {
        // Arrange - AC #1: Valid DNS-safe subdomains
        var request = new
        {
            Name = "Test Company",
            Subdomain = validSubdomain,
            Tier = TenantTier.Free
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData("ab")] // Too short
    [InlineData("-startshyphen")] // Starts with hyphen
    [InlineData("endshyphen-")] // Ends with hyphen
    [InlineData("has space")] // Contains space
    [InlineData("test@domain")] // Special characters
    public async Task CreateTenant_InvalidSubdomainFormats_Returns400(string invalidSubdomain)
    {
        // Arrange - AC #1: Invalid subdomains rejected
        var request = new
        {
            Name = "Test Company",
            Subdomain = invalidSubdomain,
            Tier = TenantTier.Free
        };

        // Act
        var response = await PostAsync("/admin/tenants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
