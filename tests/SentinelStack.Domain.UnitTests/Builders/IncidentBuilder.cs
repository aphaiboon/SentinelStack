using Bogus;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Domain.UnitTests.Builders;

/// <summary>
/// Test data builder for Incident entity using Bogus.
/// </summary>
public class IncidentBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private string? _title;
    private string? _description;
    private IncidentSeverity _severity = IncidentSeverity.Medium;
    private IncidentStatus? _status;
    private Guid? _serviceId;
    private Guid? _assignedToId;
    private Guid? _acknowledgedBy;
    private Guid? _resolvedBy;

    private static readonly Faker Faker = new();

    /// <summary>
    /// Creates a new IncidentBuilder with default values.
    /// </summary>
    public static IncidentBuilder Default() => new();

    /// <summary>
    /// Sets the tenant ID for the incident.
    /// </summary>
    public IncidentBuilder WithTenant(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    /// <summary>
    /// Sets the title for the incident.
    /// </summary>
    public IncidentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the description for the incident.
    /// </summary>
    public IncidentBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Sets the severity for the incident.
    /// </summary>
    public IncidentBuilder WithSeverity(IncidentSeverity severity)
    {
        _severity = severity;
        return this;
    }

    /// <summary>
    /// Sets the status for the incident.
    /// </summary>
    public IncidentBuilder WithStatus(IncidentStatus status)
    {
        _status = status;
        return this;
    }

    /// <summary>
    /// Sets the service ID for the incident.
    /// </summary>
    public IncidentBuilder WithService(Guid serviceId)
    {
        _serviceId = serviceId;
        return this;
    }

    /// <summary>
    /// Sets the assigned user ID for the incident.
    /// </summary>
    public IncidentBuilder WithAssignee(Guid assignedToId)
    {
        _assignedToId = assignedToId;
        return this;
    }

    /// <summary>
    /// Sets the incident as acknowledged by a user.
    /// </summary>
    public IncidentBuilder AsAcknowledged(Guid userId)
    {
        _acknowledgedBy = userId;
        _status = IncidentStatus.Acknowledged;
        return this;
    }

    /// <summary>
    /// Sets the incident as resolved by a user.
    /// </summary>
    public IncidentBuilder AsResolved(Guid userId)
    {
        _resolvedBy = userId;
        _status = IncidentStatus.Resolved;
        return this;
    }

    /// <summary>
    /// Builds the Incident entity with configured or default values.
    /// </summary>
    public Incident Build()
    {
        var title = _title ?? Faker.Company.CatchPhrase();
        var description = _description ?? Faker.Lorem.Paragraph();

        var incident = new Incident(
            tenantId: _tenantId,
            title: title,
            severity: _severity,
            description: description,
            serviceId: _serviceId,
            assignedToId: _assignedToId
        );

        // Apply status transitions if specified
        var actorUserId = _assignedToId ?? Guid.NewGuid();

        if (_acknowledgedBy.HasValue && _status == IncidentStatus.Acknowledged)
        {
            incident.Acknowledge(_acknowledgedBy.Value);
        }

        if (_resolvedBy.HasValue && _status == IncidentStatus.Resolved)
        {
            // Need to acknowledge first before resolving
            if (incident.Status == IncidentStatus.Open)
            {
                incident.Acknowledge(actorUserId);
            }
            incident.Resolve(_resolvedBy.Value);
        }

        if (_status == IncidentStatus.Investigating)
        {
            incident.Acknowledge(actorUserId);
            incident.StartInvestigation(actorUserId);
        }

        if (_status == IncidentStatus.Closed)
        {
            incident.Acknowledge(actorUserId);
            incident.Resolve(actorUserId);
            incident.Close(actorUserId);
        }

        return incident;
    }
}
