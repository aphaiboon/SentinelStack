using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Services.Queries;

/// <summary>
/// Query to get a single service by ID or key.
/// </summary>
public class GetServiceQuery
{
    private readonly IServiceRepository _serviceRepository;

    public GetServiceQuery(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<ServiceDto?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        return service == null ? null : MapToDto(service);
    }

    public async Task<ServiceDto?> ExecuteByKeyAsync(string serviceKey, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByKeyAsync(serviceKey, cancellationToken);
        return service == null ? null : MapToDto(service);
    }

    private static ServiceDto MapToDto(Service service) => new()
    {
        Id = service.Id,
        TenantId = service.TenantId,
        Name = service.Name,
        ServiceKey = service.ServiceKey,
        Description = service.Description,
        IsActive = service.IsActive,
        OwnerTeam = service.OwnerTeam,
        ContactEmail = service.ContactEmail,
        SlackChannel = service.SlackChannel,
        CreatedAtUtc = service.CreatedAtUtc,
        CreatedBy = service.CreatedBy,
        UpdatedAtUtc = service.UpdatedAtUtc,
        UpdatedBy = service.UpdatedBy
    };
}
