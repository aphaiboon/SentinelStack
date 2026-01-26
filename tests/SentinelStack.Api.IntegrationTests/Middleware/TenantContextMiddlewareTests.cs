using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SentinelStack.Api.IntegrationTests.Infrastructure;

namespace SentinelStack.Api.IntegrationTests.Middleware;

/// <summary>
/// Integration tests for TenantContextMiddleware.
/// Tests ADR-007: Tenant Context via X-Tenant-Id Header.
/// </summary>
[Collection("Integration")]
public class TenantContextMiddlewareTests
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    // Use a protected endpoint for testing (not /health which is public)
    private const string ProtectedEndpoint = "/api/v1/services";

    public TenantContextMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task ValidTenantId_WithMatchingJwtClaim_PassesTenantValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var jwt = JwtTokenGenerator.GenerateToken(userId, tenantId, "test@example.com", "Admin");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        // May return 200 (OK) or 404 (NotFound) depending on data, but should not be 400/403
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MissingTenantIdHeader_Returns400BadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var jwt = JwtTokenGenerator.GenerateToken(userId, tenantId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        // No X-Tenant-Id header

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Detail.Should().Be("X-Tenant-Id header is required");
    }

    [Fact]
    public async Task InvalidTenantIdGuid_Returns400BadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var jwt = JwtTokenGenerator.GenerateToken(userId, tenantId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", "not-a-guid");

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(400);
        problemDetails.Detail.Should().Be("X-Tenant-Id header must be a valid GUID");
    }

    [Fact]
    public async Task TenantIdNotInJwtClaim_Returns403Forbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var allowedTenant1 = Guid.NewGuid();
        var allowedTenant2 = Guid.NewGuid();
        var unauthorizedTenant = Guid.NewGuid();

        var jwt = JwtTokenGenerator.GenerateToken(
            userId,
            new List<Guid> { allowedTenant1, allowedTenant2 });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", unauthorizedTenant.ToString()); // Not in allowed list

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(403);
        problemDetails.Detail.Should().Be("User does not have access to this tenant");
    }

    [Fact]
    public async Task MultipleTenants_ValidSelection_PassesTenantValidation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();
        var tenant3 = Guid.NewGuid();

        var jwt = JwtTokenGenerator.GenerateToken(
            userId,
            new List<Guid> { tenant1, tenant2, tenant3 });

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant2.ToString()); // Select tenant2 from allowed list

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        // May return 200 (OK) or 404 (NotFound) depending on data, but should not be 400/403
        response.StatusCode.Should().NotBe(HttpStatusCode.BadRequest);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MissingTenantIdsClaim_Returns403Forbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        // Generate a token WITHOUT tenantIds claim (empty list)
        var jwt = JwtTokenGenerator.GenerateToken(userId, new List<Guid>());

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonOptions);
        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(403);
        problemDetails.Detail.Should().Be("User does not have access to any tenants");
    }

    [Fact]
    public async Task UnauthenticatedRequest_WithTenantHeader_ReturnsErrorStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // No Authorization header
        _client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());

        // Act
        var response = await _client.GetAsync(ProtectedEndpoint);

        // Assert
        // Without authentication, either:
        // - 401 Unauthorized (if endpoint has [Authorize] attribute) OR
        // - 403 Forbidden (if middleware runs and finds no tenantIds claim in empty User)
        // Both are valid rejection behaviors
        (response.StatusCode == HttpStatusCode.Unauthorized ||
         response.StatusCode == HttpStatusCode.Forbidden).Should().BeTrue();
    }

    [Fact]
    public async Task HealthEndpoint_DoesNotRequireTenantId()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        // Health endpoints bypass tenant validation (public endpoint)
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task SwaggerEndpoint_DoesNotRequireTenantId()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        // Swagger bypasses tenant validation (public endpoint)
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
