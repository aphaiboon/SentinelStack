using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Auth.DTOs;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Api.IntegrationTests.Controllers;

[Collection("Integration")]
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;
    private Guid _testTenantId;
    private Guid _testUserId;
    private string _testEmail = null!;
    private const string TestPassword = "TestPassword@123!";

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        // Don't create client here - wait until InitializeAsync
    }

    public async Task InitializeAsync()
    {
        // Create client now that the container is ready
        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        _testTenantId = Guid.NewGuid();
        _testEmail = $"authtest_{Guid.NewGuid():N}@sentinelstack.io";

        var user = new User(
            _testTenantId,
            _testEmail,
            "Auth Test User",
            passwordHasher.HashPassword(TestPassword),
            UserRole.Admin);

        typeof(User).GetProperty("Id")!.SetValue(user, Guid.NewGuid());
        _testUserId = user.Id;

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var users = context.Users.Where(u => u.TenantId == _testTenantId);
        context.Users.RemoveRange(users);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = _testEmail,
            Password = TestPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@sentinelstack.io",
            Password = TestPassword
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = _testEmail,
            Password = "WrongPassword@123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange - First login to get tokens
        var loginRequest = new LoginRequest
        {
            Email = _testEmail,
            Password = TestPassword
        };
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = tokens!.RefreshToken
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
