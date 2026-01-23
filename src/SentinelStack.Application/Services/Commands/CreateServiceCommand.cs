using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Services.Commands;

/// <summary>
/// Command to create a new service.
/// </summary>
public class CreateServiceCommand
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateServiceCommand(
        IServiceRepository serviceRepository,
        ICurrentTenantService tenantService,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _tenantService = tenantService;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDto> ExecuteAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantService.TenantId
            ?? throw new InvalidOperationException("Tenant context is required");

        // Check if service key already exists
        if (await _serviceRepository.KeyExistsAsync(request.ServiceKey, cancellationToken))
        {
            throw new InvalidOperationException($"Service with key '{request.ServiceKey}' already exists");
        }

        var service = new Service(
            tenantId,
            request.Name,
            request.ServiceKey,
            request.Description);

        // Set optional fields
        service.Update(
            request.Name,
            request.Description,
            request.OwnerTeam,
            request.ContactEmail);

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            service.SetCreatedBy(userId.Value);
        }

        await _serviceRepository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(service);
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
