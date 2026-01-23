using FluentAssertions;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using Xunit;

namespace SentinelStack.Domain.UnitTests.Entities;

public class UserTests
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Constructor_SetsInitialState_Correctly()
    {
        // Arrange & Act
        var user = new User(
            _tenantId,
            "Test@Example.com",
            "Test User",
            "hashedPassword123");

        // Assert
        user.TenantId.Should().Be(_tenantId);
        user.Email.Should().Be("test@example.com"); // Should be lowercased
        user.DisplayName.Should().Be("Test User");
        user.Role.Should().Be(UserRole.Viewer); // Default role
        user.IsActive.Should().BeTrue();
        user.MfaEnabled.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntilUtc.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithRole_SetsRole()
    {
        // Arrange & Act
        var user = new User(
            _tenantId,
            "admin@example.com",
            "Admin User",
            "hashedPassword",
            UserRole.Admin);

        // Assert
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void RecordLogin_ResetsFailedAttemptsAndSetsLastLogin()
    {
        // Arrange
        var user = CreateUser();
        user.RecordFailedLogin(); // Simulate failed attempts
        user.RecordFailedLogin();

        // Act
        user.RecordLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAtUtc.Should().NotBeNull();
        user.LastLoginAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.LockedUntilUtc.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_IncrementsFailedAttempts()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.RecordFailedLogin();
        user.RecordFailedLogin();

        // Assert
        user.FailedLoginAttempts.Should().Be(2);
        user.LockedUntilUtc.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_LocksAccount_AfterMaxAttempts()
    {
        // Arrange
        var user = CreateUser();

        // Act - Exceed max attempts (default is 5)
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.LockedUntilUtc.Should().NotBeNull();
        user.IsLocked.Should().BeTrue();
    }

    [Fact]
    public void RecordFailedLogin_CustomMaxAttempts_LocksAtThreshold()
    {
        // Arrange
        var user = CreateUser();

        // Act - Use custom threshold of 3
        for (int i = 0; i < 3; i++)
        {
            user.RecordFailedLogin(maxAttempts: 3, lockoutMinutes: 30);
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(3);
        user.IsLocked.Should().BeTrue();
    }

    [Fact]
    public void IsLocked_ReturnsFalse_WhenLockoutExpired()
    {
        // Arrange
        var user = CreateUser();
        // Lock the account with a very short lockout
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin(maxAttempts: 5, lockoutMinutes: 0);
        }

        // Assert - With 0 minute lockout, it should already be expired
        user.IsLocked.Should().BeFalse();
    }

    [Fact]
    public void UpdateProfile_UpdatesNameFields()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.UpdateProfile("New Display Name", "John", "Doe");

        // Assert
        user.DisplayName.Should().Be("New Display Name");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
    }

    [Fact]
    public void UpdatePassword_ResetsPasswordAndUnlocksAccount()
    {
        // Arrange
        var user = CreateUser();
        // Lock the account
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Act
        user.UpdatePassword("newHashedPassword");

        // Assert
        user.PasswordHash.Should().Be("newHashedPassword");
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntilUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateRole_ChangesUserRole()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.UpdateRole(UserRole.Manager);

        // Assert
        user.Role.Should().Be(UserRole.Manager);
    }

    [Fact]
    public void EnableMfa_SetsMfaEnabled()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.EnableMfa();

        // Assert
        user.MfaEnabled.Should().BeTrue();
    }

    [Fact]
    public void DisableMfa_ClearsMfaEnabled()
    {
        // Arrange
        var user = CreateUser();
        user.EnableMfa();

        // Act
        user.DisableMfa();

        // Assert
        user.MfaEnabled.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ResetsAccountState()
    {
        // Arrange
        var user = CreateUser();
        user.Deactivate();
        // Lock the account
        for (int i = 0; i < 5; i++)
        {
            user.RecordFailedLogin();
        }

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockedUntilUtc.Should().BeNull();
    }

    private User CreateUser()
    {
        return new User(
            _tenantId,
            "test@example.com",
            "Test User",
            "hashedPassword");
    }
}
