using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using SentinelStack.Infrastructure.Auth;
using Xunit;

namespace SentinelStack.Infrastructure.Tests.Auth;

public class TokenServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TokenService _tokenService;
    private readonly DateTime _fixedUtcNow = new(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public TokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Secret = "ThisIsAVeryLongSecretKeyForJwtToken123!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };

        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(x => x.UtcNow).Returns(_fixedUtcNow);

        _tokenService = new TokenService(
            Options.Create(_jwtSettings),
            _dateTimeProviderMock.Object);
    }

    private User CreateTestUser() => new(
        tenantId: Guid.NewGuid(),
        email: "test@example.com",
        passwordHash: "hashedPassword",
        displayName: "Test User",
        role: UserRole.Admin);

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be(_jwtSettings.Issuer);
        jwtToken.Audiences.Should().Contain(_jwtSettings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ContainsRequiredClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == user.TenantId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "display_name" && c.Value == user.DisplayName);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var expectedExpiry = _fixedUtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes);

        // Act
        var token = _tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueTokens()
    {
        // Act
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrWhiteSpace();
        token2.Should().NotBeNullOrWhiteSpace();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64EncodedToken()
    {
        // Act
        var token = _tokenService.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrWhiteSpace();

        // Should be valid base64
        var action = () => Convert.FromBase64String(token);
        action.Should().NotThrow();
    }

    [Fact]
    public async Task StoreAndValidateRefreshToken_WorksCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = _fixedUtcNow.AddDays(7);

        // Act
        await _tokenService.StoreRefreshTokenAsync(userId, refreshToken, expiry);
        var validatedUserId = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        validatedUserId.Should().Be(userId);
    }

    [Fact]
    public async Task ValidateRefreshToken_ReturnsNullForExpiredToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = _fixedUtcNow.AddDays(-1); // Already expired

        await _tokenService.StoreRefreshTokenAsync(userId, refreshToken, expiry);

        // Act
        var result = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateRefreshToken_ReturnsNullForUnknownToken()
    {
        // Act
        var result = await _tokenService.ValidateRefreshTokenAsync("unknown-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokens_RemovesAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();
        var expiry = _fixedUtcNow.AddDays(7);

        await _tokenService.StoreRefreshTokenAsync(userId, token1, expiry);
        await _tokenService.StoreRefreshTokenAsync(userId, token2, expiry);

        // Act
        await _tokenService.RevokeRefreshTokensAsync(userId);

        // Assert
        var result1 = await _tokenService.ValidateRefreshTokenAsync(token1);
        var result2 = await _tokenService.ValidateRefreshTokenAsync(token2);

        result1.Should().BeNull();
        result2.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokens_DoesNotAffectOtherUserTokens()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var token1 = _tokenService.GenerateRefreshToken();
        var token2 = _tokenService.GenerateRefreshToken();
        var expiry = _fixedUtcNow.AddDays(7);

        await _tokenService.StoreRefreshTokenAsync(userId1, token1, expiry);
        await _tokenService.StoreRefreshTokenAsync(userId2, token2, expiry);

        // Act
        await _tokenService.RevokeRefreshTokensAsync(userId1);

        // Assert
        var result1 = await _tokenService.ValidateRefreshTokenAsync(token1);
        var result2 = await _tokenService.ValidateRefreshTokenAsync(token2);

        result1.Should().BeNull("token for userId1 should be revoked");
        result2.Should().Be(userId2, "token for userId2 should still be valid");
    }
}
