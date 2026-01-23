using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Incidents.Commands;

/// <summary>
/// Command to create a new incident.
/// </summary>
public class CreateIncidentCommand
{
    private readonly IIncidentRepository _incidentRepository;
    private readonly ICurrentTenantService _tenantService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIncidentCommand(
        IIncidentRepository incidentRepository,
        ICurrentTenantService tenantService,
        IUnitOfWork unitOfWork)
    {
        _incidentRepository = incidentRepository;
        _tenantService = tenantService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IncidentDto> ExecuteAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        var incident = new Incident(
            tenantId: tenantId,
            title: request.Title,
            severity: request.Severity,
            description: request.Description,
            serviceId: request.ServiceId,
            assignedToId: request.AssignedToId);

        await _incidentRepository.AddAsync(incident, cancellationToken);
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
