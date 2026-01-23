using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Services.Commands;

/// <summary>
/// Command to update an existing service.
/// </summary>
public class UpdateServiceCommand
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICurrentUserService _userService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceCommand(
        IServiceRepository serviceRepository,
        ICurrentUserService userService,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _userService = userService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDto?> ExecuteAsync(Guid id, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(id, cancellationToken);
        if (service == null)
        {
            return null;
        }

        service.Update(
            request.Name,
            request.Description,
            request.OwnerTeam,
            request.ContactEmail);

        if (request.SlackChannel != null)
        {
            service.SetSlackChannel(request.SlackChannel);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                service.Activate();
            }
            else
            {
                service.Deactivate();
            }
        }

        var userId = _userService.UserId;
        if (userId.HasValue)
        {
            service.SetUpdatedBy(userId.Value);
        }

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
