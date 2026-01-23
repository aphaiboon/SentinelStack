using FluentAssertions;
using SentinelStack.Infrastructure.Auth;
using Xunit;

namespace SentinelStack.Infrastructure.Tests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void HashPassword_ReturnsHashWithCorrectFormat()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        var parts = hash.Split(':');
        parts.Should().HaveCount(3, "hash should be in format iterations:salt:hash");
        int.Parse(parts[0]).Should().BeGreaterThanOrEqualTo(100000, "iterations should be at least 100,000");
    }

    [Fact]
    public void HashPassword_DifferentSaltForSamePassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "different salts should produce different hashes");
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _hasher.HashPassword(password);

        // Act
        var result = _hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "SecurePassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _hasher.HashPassword(correctPassword);

        // Act
        var result = _hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForMalformedHash()
    {
        // Arrange
        var password = "SecurePassword123!";
        var malformedHash = "notavalidhash";

        // Act
        var result = _hasher.VerifyPassword(password, malformedHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForHashWithInvalidIterations()
    {
        // Arrange
        var password = "SecurePassword123!";
        var invalidHash = "invalid:YWJjZGVm:YWJjZGVm";

        // Act
        var result = _hasher.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForHashWithInvalidBase64()
    {
        // Arrange
        var password = "SecurePassword123!";
        var invalidHash = "100000:not-base64!:also-not-base64!";

        // Act
        var result = _hasher.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("short")]
    [InlineData("")]
    public void HashPassword_HandlesVariousPasswordLengths(string password)
    {
        // Act
        var hash = _hasher.HashPassword(password);
        var result = _hasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        // Arrange
        var password = "CaseSensitive";
        var hash = _hasher.HashPassword(password);

        // Act
        var correctCase = _hasher.VerifyPassword("CaseSensitive", hash);
        var wrongCase = _hasher.VerifyPassword("casesensitive", hash);

        // Assert
        correctCase.Should().BeTrue();
        wrongCase.Should().BeFalse();
    }
}
