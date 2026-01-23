using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Application.Incidents.Queries;

/// <summary>
/// Request model for filtering incidents.
/// </summary>
public class GetIncidentsRequest
{
    public IncidentStatus? Status { get; init; }
    public IncidentSeverity? Severity { get; init; }
    public Guid? ServiceId { get; init; }
    public Guid? AssignedToId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response model for paginated incidents.
/// </summary>
public class GetIncidentsResponse
{
    public required IReadOnlyList<IncidentDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>
/// Query to get incidents with filtering and pagination.
/// </summary>
public class GetIncidentsQuery
{
    private readonly IIncidentRepository _incidentRepository;

    public GetIncidentsQuery(IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public async Task<GetIncidentsResponse> ExecuteAsync(GetIncidentsRequest request, CancellationToken cancellationToken = default)
    {
        var incidents = await _incidentRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var filtered = incidents.AsEnumerable();

        if (request.Status.HasValue)
        {
            filtered = filtered.Where(i => i.Status == request.Status.Value);
        }

        if (request.Severity.HasValue)
        {
            filtered = filtered.Where(i => i.Severity == request.Severity.Value);
        }

        if (request.ServiceId.HasValue)
        {
            filtered = filtered.Where(i => i.ServiceId == request.ServiceId.Value);
        }

        if (request.AssignedToId.HasValue)
        {
            filtered = filtered.Where(i => i.AssignedToId == request.AssignedToId.Value);
        }

        var filteredList = filtered.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var paged = filteredList
            .OrderByDescending(i => i.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        return new GetIncidentsResponse
        {
            Items = paged,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
