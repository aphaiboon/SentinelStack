using SentinelStack.Domain.Common;

namespace SentinelStack.Domain.Entities;

/// <summary>
/// Represents a service or system that can have incidents.
/// </summary>
public class Service : AuditableEntity, ITenantEntity
{
    /// <summary>
    /// The tenant this service belongs to.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Name of the service.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description of the service.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Unique key/slug for the service (e.g., "api-gateway", "database").
    /// </summary>
    public string ServiceKey { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this service is active and monitored.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The team or group responsible for this service.
    /// </summary>
    public string? OwnerTeam { get; private set; }

    /// <summary>
    /// Email address for service notifications.
    /// </summary>
    public string? ContactEmail { get; private set; }

    /// <summary>
    /// Slack channel for service incidents (Beta feature).
    /// </summary>
    public string? SlackChannel { get; private set; }

    /// <summary>
    /// Custom metadata for the service (JSON).
    /// </summary>
    public string? Metadata { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private Service() { }

    /// <summary>
    /// Creates a new service.
    /// </summary>
    public Service(
        Guid tenantId,
        string name,
        string serviceKey,
        string? description = null)
    {
        TenantId = tenantId;
        Name = name;
        ServiceKey = serviceKey.ToLowerInvariant();
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// Updates the service information.
    /// </summary>
    public void Update(
        string name,
        string? description,
        string? ownerTeam,
        string? contactEmail)
    {
        Name = name;
        Description = description;
        OwnerTeam = ownerTeam;
        ContactEmail = contactEmail;
    }

    /// <summary>
    /// Sets the Slack channel for this service.
    /// </summary>
    public void SetSlackChannel(string? slackChannel)
    {
        SlackChannel = slackChannel;
    }

    /// <summary>
    /// Sets custom metadata for the service.
    /// </summary>
    public void SetMetadata(string? metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Deactivates the service.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Activates the service.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
