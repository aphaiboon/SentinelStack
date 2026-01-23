using SentinelStack.Domain.Common;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Defines an escalation policy with ordered steps for incident escalation.
/// </summary>
public class EscalationPolicy : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this policy belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Name of the escalation policy.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description of the policy.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Whether this policy is active.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Whether this is the default policy for new incidents.
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// The service this policy is associated with (null = applies to all).
    /// </summary>
    public Guid? ServiceId { get; private set; }

    /// <summary>
    /// Ordered list of escalation steps.
    /// </summary>
    private readonly List<EscalationStep> _steps = new();
    public IReadOnlyList<EscalationStep> Steps => _steps.AsReadOnly();

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private EscalationPolicy() { }

    /// <summary>
    /// Creates a new escalation policy.
    /// </summary>
    public EscalationPolicy(
        Guid tenantId,
        string name,
        string? description = null,
        Guid? serviceId = null)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        ServiceId = serviceId;
        IsActive = true;
        IsDefault = false;
    }

    /// <summary>
    /// Updates the policy information.
    /// </summary>
    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// Sets this policy as the default.
    /// </summary>
    public void SetAsDefault()
    {
        IsDefault = true;
    }

    /// <summary>
    /// Removes default status from this policy.
    /// </summary>
    public void RemoveDefault()
    {
        IsDefault = false;
    }

    /// <summary>
    /// Activates the policy.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }

    /// <summary>
    /// Deactivates the policy.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Adds an escalation step to the policy.
    /// </summary>
    public void AddStep(EscalationStep step)
    {
        _steps.Add(step);
    }

    /// <summary>
    /// Removes an escalation step from the policy.
    /// </summary>
    public void RemoveStep(EscalationStep step)
    {
        _steps.Remove(step);
    }

    /// <summary>
    /// Associates this policy with a service.
    /// </summary>
    public void AssociateWithService(Guid? serviceId)
    {
        ServiceId = serviceId;
    }
}
