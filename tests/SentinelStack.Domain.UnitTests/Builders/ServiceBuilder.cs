using Bogus;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test data builder for Service entity using Bogus.
/// </summary>
public class ServiceBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string? _name;
    private string? _serviceKey;
    private string? _description;
    private string? _ownerTeam;
    private string? _contactEmail;
    private string? _slackChannel;
    private bool _isActive = true;

    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new ServiceBuilder with default values.
    /// </summary>
    public static ServiceBuilder Default() => new();

    /// <summary>
    /// Sets the tenant ID for the service.
    /// </summary>
    public ServiceBuilder WithTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// Sets the name for the service.
    /// </summary>
    public ServiceBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the service key for the service.
    /// </summary>
    public ServiceBuilder WithServiceKey(string serviceKey)
    {
        _serviceKey = serviceKey;
        return this;
    }

    /// <summary>
    /// Sets the description for the service.
    /// </summary>
    public ServiceBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the owner team for the service.
    /// </summary>
    public ServiceBuilder WithOwner(string ownerTeam, string? contactEmail = null)
    {
        _ownerTeam = ownerTeam;
        _contactEmail = contactEmail;
        return this;
    }

    /// <summary>
    /// Sets the Slack channel for the service.
    /// </summary>
    public ServiceBuilder WithSlackChannel(string slackChannel)
    {
        _slackChannel = slackChannel;
        return this;
    }

    /// <summary>
    /// Sets the service as inactive.
    /// </summary>
    public ServiceBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Builds the Service entity with configured or default values.
    /// </summary>
    public Service Build()
    {
        var name = _name ?? Faker.Commerce.ProductName();
        var serviceKey = _serviceKey ?? Faker.Lorem.Slug();
        var description = _description ?? Faker.Lorem.Sentence();

        var service = new Service(
            tenantId: _tenantId,
            name: name,
            serviceKey: serviceKey,
            description: description
        );

        // Apply optional configurations
        if (_ownerTeam != null || _contactEmail != null)
        {
            service.Update(
                name: name,
                description: description,
                ownerTeam: _ownerTeam,
                contactEmail: _contactEmail
            );
        }

        if (_slackChannel != null)
        {
            service.SetSlackChannel(_slackChannel);
        }

        if (!_isActive)
        {
            service.Deactivate();
        }

        return service;
    }
}
