using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Incidents.Commands;

/// <summary>
/// Command to update an existing incident.
/// </summary>
public class UpdateIncidentCommand
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIncidentCommand(
        IIncidentRepository incidentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _incidentRepository = incidentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IncidentDto?> ExecuteAsync(Guid incidentId, UpdateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);

        if (incident == null)
        {
            return null;
        }

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User context is required");

        if (request.Title != null)
        {
            incident.UpdateTitle(request.Title, userId);
        }

        if (request.Description != null)
        {
            incident.UpdateDescription(request.Description, userId);
        }

        if (request.Severity.HasValue)
        {
            incident.UpdateSeverity(request.Severity.Value, userId);
        }

        if (request.AssignedToId.HasValue)
        {
            incident.Assign(request.AssignedToId, userId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
