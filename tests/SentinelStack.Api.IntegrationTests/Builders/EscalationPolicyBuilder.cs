using Bogus;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test data builder for EscalationPolicy entity using Bogus.
/// </summary>
public class EscalationPolicyBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string? _name;
    private string? _description;
    private Guid? _serviceId;
    private bool _isDefault = false;
    private bool _isActive = true;
    private readonly List<EscalationStep> _steps = new();

    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new EscalationPolicyBuilder with default values.
    /// </summary>
    public static EscalationPolicyBuilder Default() => new();

    /// <summary>
    /// Sets the tenant ID for the escalation policy.
    /// </summary>
    public EscalationPolicyBuilder WithTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// Sets the name for the escalation policy.
    /// </summary>
    public EscalationPolicyBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the description for the escalation policy.
    /// </summary>
    public EscalationPolicyBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Associates the policy with a service.
    /// </summary>
    public EscalationPolicyBuilder ForService(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    /// <summary>
    /// Marks the policy as the default.
    /// </summary>
    public EscalationPolicyBuilder AsDefault()
    {
        _isDefault = true;
        return this;
    }

    /// <summary>
    /// Marks the policy as inactive.
    /// </summary>
    public EscalationPolicyBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Adds an escalation step to the policy.
    /// </summary>
    public EscalationPolicyBuilder WithStep(EscalationStep step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Adds multiple escalation steps to the policy.
    /// </summary>
    public EscalationPolicyBuilder WithSteps(params EscalationStep[] steps)
    {
        _steps.AddRange(steps);
        return this;
    }

    /// <summary>
    /// Builds the EscalationPolicy entity with configured or default values.
    /// </summary>
    public EscalationPolicy Build()
    {
        var name = _name ?? Faker.Company.CatchPhrase();
        var description = _description ?? Faker.Lorem.Sentence();

        var policy = new EscalationPolicy(
            tenantId: _tenantId,
            name: name,
            description: description,
            serviceId: _serviceId
        );

        // Apply optional configurations
        if (_isDefault)
        {
            policy.SetAsDefault();
        }

        if (!_isActive)
        {
            policy.Deactivate();
        }

        // Add steps to the policy
        foreach (var step in _steps)
        {
            policy.AddStep(step);
        }

        return policy;
    }
}
