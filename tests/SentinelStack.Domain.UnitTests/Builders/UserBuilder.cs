using Bogus;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test data builder for User entity using Bogus.
/// </summary>
public class UserBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string? _email;
    private string? _displayName;
    private string? _firstName;
    private string? _lastName;
    private string _password = "Password123!";
    private UserRole _role = UserRole.Viewer;
    private bool _isActive = true;
    private bool _mfaEnabled = false;

    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new UserBuilder with default values.
    /// </summary>
    public static UserBuilder Default() => new();

    /// <summary>
    /// Sets the tenant ID for the user.
    /// </summary>
    public UserBuilder WithTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// Sets the email for the user.
    /// </summary>
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Sets the display name for the user.
    /// </summary>
    public UserBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    /// <summary>
    /// Sets the first and last name for the user.
    /// </summary>
    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    /// <summary>
    /// Sets the password for the user (will be hashed with BCrypt).
    /// </summary>
    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Sets the role for the user.
    /// </summary>
    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    /// <summary>
    /// Sets the user as inactive.
    /// </summary>
    public UserBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Sets the user with MFA enabled.
    /// </summary>
    public UserBuilder WithMfaEnabled()
    {
        _mfaEnabled = true;
        return this;
    }

    /// <summary>
    /// Builds the User entity with configured or default values.
    /// </summary>
    public User Build()
    {
        var email = _email ?? Faker.Internet.Email();
        var displayName = _displayName ?? Faker.Name.FullName();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(_password);

        var user = new User(
            tenantId: _tenantId,
            email: email,
            displayName: displayName,
            passwordHash: passwordHash,
            role: _role
        );

        // Apply optional configurations
        if (_firstName != null || _lastName != null)
        {
            user.UpdateProfile(
                displayName: displayName,
                firstName: _firstName ?? Faker.Name.FirstName(),
                lastName: _lastName ?? Faker.Name.LastName()
            );
        }

        if (!_isActive)
        {
            user.Deactivate();
        }

        if (_mfaEnabled)
        {
            user.EnableMfa();
        }

        return user;
    }
}
