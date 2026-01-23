using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Incidents.Commands;

/// <summary>
/// Command to resolve an incident.
/// </summary>
public class ResolveIncidentCommand
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveIncidentCommand(
        IIncidentRepository incidentRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _incidentRepository = incidentRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IncidentDto?> ExecuteAsync(Guid incidentId, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepository.GetByIdAsync(incidentId, cancellationToken);

        if (incident == null)
        {
            return null;
        }

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User context is required");

        incident.Resolve(userId);
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
