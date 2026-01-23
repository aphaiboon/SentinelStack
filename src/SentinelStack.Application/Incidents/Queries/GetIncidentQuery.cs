using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Incidents.Queries;

/// <summary>
/// Query to get a single incident by ID.
/// </summary>
public class GetIncidentQuery
{
    private readonly IIncidentRepository _incidentRepository;

    public GetIncidentQuery(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task<IncidentDto?> ExecuteAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);

        if (incident == null)
        {
            return null;
        }

        return MapToDto(incident);
    }

    private static IncidentDto MapToDto(Incident incident) => new()
    {
        Id = incident.Id,
        TenantId = incident.TenantId,
        Title = incident.Title,
        Description = incident.Description,
        Severity = incident.Severity,
        Status = incident.Status,
        ServiceId = incident.ServiceId,
        AssignedToId = incident.AssignedToId,
        AcknowledgedAtUtc = incident.AcknowledgedAtUtc,
        AcknowledgedBy = incident.AcknowledgedBy,
        ResolvedAtUtc = incident.ResolvedAtUtc,
        ResolvedBy = incident.ResolvedBy,
        CreatedAtUtc = incident.CreatedAtUtc,
        CreatedBy = incident.CreatedBy,
        UpdatedAtUtc = incident.UpdatedAtUtc,
        UpdatedBy = incident.UpdatedBy
    };
}
